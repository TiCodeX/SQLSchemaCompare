using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TiCodeX.SQLSchemaCompare.Core.Entities;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database.MicrosoftSql;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database.MySql;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database.PostgreSql;
using TiCodeX.SQLSchemaCompare.Core.Entities.DatabaseProvider;
using TiCodeX.SQLSchemaCompare.Core.Enums;
using TiCodeX.SQLSchemaCompare.Core.Interfaces;
using TiCodeX.SQLSchemaCompare.Core.Interfaces.Services;
using TiCodeX.SQLSchemaCompare.Infrastructure.DatabaseProviders;
using TiCodeX.SQLSchemaCompare.Infrastructure.EntityFramework;
using TiCodeX.SQLSchemaCompare.Infrastructure.SqlScripters;
using TiCodeX.SQLSchemaCompare.Services;
using Xunit.Sdk;

namespace TiCodeX.SQLSchemaCompare.Test
{
    /// <summary>
    /// Creates the sakila/pagila databases for the tests
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class DatabaseFixture : IDisposable
    {
        private static readonly object InitializeSakilaDatabaseLock = new object();
        private static bool initializeSakilaDatabase = true;
        private readonly ICipherService cipherService = new CipherService();
        private readonly bool exportGeneratedFullScript = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ExportGeneratedFullScript"));
        private ILoggerFactory loggerFactory = new XunitLoggerFactory(null);

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseFixture"/> class
        /// </summary>
        public DatabaseFixture()
        {
            // TODO: Use ICollectionFixture<> instead of IClassFixture<> in order to remove this lock to prevent multiple executions
            lock (InitializeSakilaDatabaseLock)
            {
                if (!initializeSakilaDatabase)
                {
                    return;
                }

                // MicrosoftSQL
                this.CreateMicrosoftSqlSakilaDatabase("sakila");

                // MySQL
                this.CreateMySqlSakilaDatabase("sakila");

                // PostgreSQL
                this.CreatePostgreSqlSakilaDatabase("sakila");

                initializeSakilaDatabase = false;
            }
        }

        /// <summary>
        /// Sets the test output helper
        /// </summary>
        /// <param name="logFactory">The test output helper</param>
        public void SetLoggerFactory(ILoggerFactory logFactory)
        {
            this.loggerFactory = logFactory;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.loggerFactory.Dispose();
        }

        /// <summary>
        /// Executes my SQL script
        /// </summary>
        /// <param name="path">The path</param>
        internal void ExecuteMySqlScript(string path)
        {
            var standardOutput = new StringBuilder();
            var standardError = new StringBuilder();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "C:\\Program Files\\MySQL\\MySQL Server 8.0\\bin\\mysql.exe",
                    Arguments = $"--user root -ptest1234 -e \"SOURCE {path}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                }
            };
            process.OutputDataReceived += (sender, data) => standardOutput.AppendLine(data.Data);
            process.ErrorDataReceived += (sender, data) => standardError.AppendLine(data.Data);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit((int)TimeSpan.FromMinutes(5).TotalMilliseconds);

            if (process.ExitCode == 0)
            {
                return;
            }

            var exceptionMessage = new StringBuilder();
            exceptionMessage.AppendLine($"Failed executing script '{path}'");
            exceptionMessage.AppendLine("Standard Output:");
            exceptionMessage.AppendLine(standardOutput.ToString());
            exceptionMessage.AppendLine("Standard Error:");
            exceptionMessage.AppendLine(standardError.ToString());
            throw new XunitException(exceptionMessage.ToString());
        }

        /// <summary>
        /// Drops the microsoft SQL database
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        internal void DropMicrosoftSqlDatabase(string databaseName)
        {
            using (var context = new MicrosoftSqlDatabaseContext(this.loggerFactory, this.cipherService, this.GetMicrosoftSqlDatabaseProviderOptions(string.Empty)))
            {
                var dropDbQuery = new StringBuilder();
                dropDbQuery.AppendLine($"IF EXISTS(select * from sys.databases where name= '{databaseName}')");
                dropDbQuery.AppendLine("BEGIN");
                dropDbQuery.AppendLine($"  ALTER DATABASE {databaseName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE"); // Close existing connections
                dropDbQuery.AppendLine($"  DROP DATABASE {databaseName}");
                dropDbQuery.AppendLine("END");
                context.ExecuteNonQuery(dropDbQuery.ToString());
            }
        }

        /// <summary>
        /// Drops my SQL database
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        internal void DropMySqlDatabase(string databaseName)
        {
            var query = new StringBuilder();
            query.AppendLine($"DROP SCHEMA IF EXISTS `{databaseName}`;");
            var pathMySql = Path.GetTempFileName();
            File.WriteAllText(pathMySql, query.ToString());
            this.ExecuteMySqlScript(pathMySql);
        }

        /// <summary>
        /// Drops the postgre SQL database
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        internal void DropPostgreSqlDatabase(string databaseName)
        {
            using (var context = new PostgreSqlDatabaseContext(this.loggerFactory, this.cipherService, this.GetPostgreSqlDatabaseProviderOptions(string.Empty)))
            {
                var dropDbQuery = new StringBuilder();
                dropDbQuery.AppendLine($"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname='{databaseName}';");
                dropDbQuery.AppendLine($"DROP DATABASE IF EXISTS {databaseName};");
                context.ExecuteNonQuery(dropDbQuery.ToString());
            }
        }

        /// <summary>
        /// Drops and create the microsoft database
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        internal void DropAndCreateMicrosoftSqlDatabase(string databaseName)
        {
            this.DropMicrosoftSqlDatabase(databaseName);

            using (var context = new MicrosoftSqlDatabaseContext(this.loggerFactory, this.cipherService, this.GetMicrosoftSqlDatabaseProviderOptions(string.Empty)))
            {
                context.ExecuteNonQuery($"CREATE DATABASE {databaseName}");
            }
        }

        /// <summary>
        /// Drops and create the mysql database
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        internal void DropAndCreateMySqlDatabase(string databaseName)
        {
            this.DropMySqlDatabase(databaseName);

            var query = new StringBuilder();
            query.AppendLine($"CREATE SCHEMA `{databaseName}`;");
            var pathMySql = Path.GetTempFileName();
            File.WriteAllText(pathMySql, query.ToString());
            this.ExecuteMySqlScript(pathMySql);
        }

        /// <summary>
        /// Drops and create the postgresql database
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        internal void DropAndCreatePostgreSqlDatabase(string databaseName)
        {
            this.DropPostgreSqlDatabase(databaseName);

            using (var context = new PostgreSqlDatabaseContext(this.loggerFactory, this.cipherService, this.GetPostgreSqlDatabaseProviderOptions(string.Empty)))
            {
                var query = new StringBuilder();
                query.AppendLine($"CREATE DATABASE {databaseName};");
                context.ExecuteNonQuery(query.ToString());
            }
        }

        /// <summary>
        /// Creates the sakila database
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        internal void CreateMicrosoftSqlSakilaDatabase(string databaseName)
        {
            this.DropAndCreateMicrosoftSqlDatabase(databaseName);

            using (var context = new MicrosoftSqlDatabaseContext(this.loggerFactory, this.cipherService, this.GetMicrosoftSqlDatabaseProviderOptions(databaseName)))
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Datasources\\sakila-schema-microsoftsql.sql");
                var queries = File.ReadAllText(path).Split(new[] { "GO" + Environment.NewLine }, StringSplitOptions.None);
                foreach (var query in queries.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    context.ExecuteNonQuery(query);
                }
            }
        }

        /// <summary>
        /// Creates the sakila database
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        internal void CreateMySqlSakilaDatabase(string databaseName)
        {
            this.DropAndCreateMySqlDatabase(databaseName);

            var sakilaScript = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Datasources\\sakila-schema-mysql.sql"));

            sakilaScript = sakilaScript.Replace("USE sakila;", $"USE {databaseName};", StringComparison.InvariantCulture);
            sakilaScript = sakilaScript.Replace("sakila.", $"{databaseName}.", StringComparison.InvariantCulture);

            var pathMySql = Path.GetTempFileName();
            File.WriteAllText(pathMySql, sakilaScript);
            this.ExecuteMySqlScript(pathMySql);
        }

        /// <summary>
        /// Creates the sakila database
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        internal void CreatePostgreSqlSakilaDatabase(string databaseName)
        {
            this.DropAndCreatePostgreSqlDatabase(databaseName);

            using (var context = new PostgreSqlDatabaseContext(this.loggerFactory, this.cipherService, this.GetPostgreSqlDatabaseProviderOptions(databaseName)))
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Datasources\\sakila-schema-postgresql.sql");
                context.ExecuteNonQuery(File.ReadAllText(path));
            }
        }

        /// <summary>
        /// Gets the Microsoft SQL database provider
        /// </summary>
        /// <param name="databaseName">The database name to connect</param>
        /// <returns>The Microsoft SQL database provider</returns>
        internal MicrosoftSqlDatabaseProvider GetMicrosoftSqlDatabaseProvider(string databaseName = "")
        {
            var dpf = new DatabaseProviderFactory(this.loggerFactory, this.cipherService);
            return (MicrosoftSqlDatabaseProvider)dpf.Create(this.GetMicrosoftSqlDatabaseProviderOptions(databaseName));
        }

        /// <summary>
        /// Gets the MySQL database provider
        /// </summary>
        /// <param name="databaseName">The database name to connect</param>
        /// <returns>The MySQL SQL database provider</returns>
        internal MySqlDatabaseProvider GetMySqlDatabaseProvider(string databaseName = "")
        {
            var dpf = new DatabaseProviderFactory(this.loggerFactory, this.cipherService);
            return (MySqlDatabaseProvider)dpf.Create(this.GetMySqlDatabaseProviderOptions(databaseName));
        }

        /// <summary>
        /// Gets the PostgreSQL database provider
        /// </summary>
        /// <param name="databaseName">The database name to connect</param>
        /// <returns>The PostgreSQL SQL database provider</returns>
        internal PostgreSqlDatabaseProvider GetPostgreSqlDatabaseProvider(string databaseName = "")
        {
            var dpf = new DatabaseProviderFactory(this.loggerFactory, this.cipherService);
            return (PostgreSqlDatabaseProvider)dpf.Create(this.GetPostgreSqlDatabaseProviderOptions(databaseName));
        }

        /// <summary>
        /// Gets the Microsoft SQL database provider options
        /// </summary>
        /// <param name="databaseName">The database name to connect</param>
        /// <returns>The Microsoft SQL database provider options</returns>
        internal MicrosoftSqlDatabaseProviderOptions GetMicrosoftSqlDatabaseProviderOptions(string databaseName)
        {
            return new MicrosoftSqlDatabaseProviderOptions
            {
                Hostname = "localhost\\SQLEXPRESS",
                Database = databaseName,
                UseWindowsAuthentication = true,
            };
        }

        /// <summary>
        /// Gets the MySQL database provider options
        /// </summary>
        /// <param name="databaseName">The database name to connect</param>
        /// <returns>The MySQL database provider options</returns>
        internal MySqlDatabaseProviderOptions GetMySqlDatabaseProviderOptions(string databaseName)
        {
            return new MySqlDatabaseProviderOptions
            {
                Hostname = "localhost",
                Database = databaseName,
                Username = "root",
                Password = this.cipherService.EncryptString("test1234"),
                UseSSL = Environment.MachineName != "DESKTOP-VH0A18B", // debe's MySql Server doesn't support SSL
            };
        }

        /// <summary>
        /// Gets the PostgreSQL database provider options
        /// </summary>
        /// <param name="databaseName">The database name to connect</param>
        /// <returns>The PostgreSQL database provider options</returns>
        internal PostgreSqlDatabaseProviderOptions GetPostgreSqlDatabaseProviderOptions(string databaseName)
        {
            return new PostgreSqlDatabaseProviderOptions
            {
                Hostname = "localhost",
                Database = databaseName,
                Username = "postgres",
                Password = this.cipherService.EncryptString("test1234"),
            };
        }

        /// <summary>
        /// Compares the databases
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="sourceDatabaseName">Name of the source database.</param>
        /// <param name="targetDatabaseName">Name of the target database.</param>
        internal void CompareDatabases(DatabaseType type, string sourceDatabaseName, string targetDatabaseName)
        {
            IDatabaseProvider sourceProvider;
            IDatabaseProvider targetProvider;
            switch (type)
            {
                case DatabaseType.MicrosoftSql:
                    sourceProvider = this.GetMicrosoftSqlDatabaseProvider(sourceDatabaseName);
                    targetProvider = this.GetMicrosoftSqlDatabaseProvider(targetDatabaseName);
                    break;
                case DatabaseType.MySql:
                    sourceProvider = this.GetMySqlDatabaseProvider(sourceDatabaseName);
                    targetProvider = this.GetMySqlDatabaseProvider(targetDatabaseName);
                    break;
                case DatabaseType.PostgreSql:
                    sourceProvider = this.GetPostgreSqlDatabaseProvider(sourceDatabaseName);
                    targetProvider = this.GetPostgreSqlDatabaseProvider(targetDatabaseName);
                    break;
                default:
                    throw new NotImplementedException();
            }

            var sourceDb = sourceProvider.GetDatabase(new TaskInfo("test"));
            var targetDb = targetProvider.GetDatabase(new TaskInfo("test"));

            var tableType = typeof(ABaseDbTable);
            var columnType = typeof(ABaseDbColumn);
            var foreignKeyType = typeof(ABaseDbForeignKey);
            var indexType = typeof(ABaseDbIndex);
            var viewType = typeof(ABaseDbView);
            var functionType = typeof(ABaseDbFunction);
            var storedProcedureType = typeof(ABaseDbStoredProcedure);
            var dataType = typeof(ABaseDbDataType);
            var sequenceType = typeof(ABaseDbSequence);
            switch (type)
            {
                case DatabaseType.MicrosoftSql:
                    tableType = typeof(MicrosoftSqlTable);
                    columnType = typeof(MicrosoftSqlColumn);
                    foreignKeyType = typeof(MicrosoftSqlForeignKey);
                    indexType = typeof(MicrosoftSqlIndex);
                    viewType = typeof(MicrosoftSqlView);
                    functionType = typeof(MicrosoftSqlFunction);
                    storedProcedureType = typeof(MicrosoftSqlStoredProcedure);
                    dataType = typeof(MicrosoftSqlDataType);
                    sequenceType = typeof(MicrosoftSqlSequence);
                    break;

                case DatabaseType.MySql:
                    tableType = typeof(MySqlTable);
                    columnType = typeof(MySqlColumn);
                    foreignKeyType = typeof(MySqlForeignKey);
                    indexType = typeof(MySqlIndex);
                    viewType = typeof(MySqlView);
                    functionType = typeof(MySqlFunction);
                    storedProcedureType = typeof(MySqlStoredProcedure);

                    // No specific data type
                    // No specific sequence type
                    break;

                case DatabaseType.PostgreSql:
                    tableType = typeof(PostgreSqlTable);
                    columnType = typeof(PostgreSqlColumn);
                    foreignKeyType = typeof(PostgreSqlForeignKey);
                    indexType = typeof(PostgreSqlIndex);
                    viewType = typeof(PostgreSqlView);
                    functionType = typeof(PostgreSqlFunction);

                    // No specific stored procedure type
                    // TODO: cast to specific PostgreSQL data types
                    sequenceType = typeof(PostgreSqlSequence);
                    break;
            }

            var tables = sourceDb.Tables.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, tableType, CultureInfo.InvariantCulture)).ToList();
            var clonedTables = targetDb.Tables.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, tableType, CultureInfo.InvariantCulture)).ToList();
            tables.Should().BeEquivalentTo(clonedTables, options =>
            {
                options.Excluding(x => ((ABaseDbTable)x).Database);
                options.Excluding(x => ((ABaseDbTable)x).ModifyDate);
                options.Excluding(x => ((ABaseDbTable)x).Columns);
                options.Excluding(x => new Regex("^PrimaryKeys\\[.+\\]\\.TableDatabase$").IsMatch(x.SelectedMemberPath));
                options.Excluding(x => new Regex("^PrimaryKeys\\[.+\\]\\.Database$").IsMatch(x.SelectedMemberPath));
                options.Excluding(x => ((ABaseDbTable)x).ForeignKeys);
                options.Excluding(x => ((ABaseDbTable)x).ReferencingForeignKeys);
                options.Excluding(x => ((ABaseDbTable)x).Indexes);
                options.Excluding(x => new Regex("^Constraints\\[.+\\]\\.TableDatabase$").IsMatch(x.SelectedMemberPath));
                options.Excluding(x => new Regex("^Constraints\\[.+\\]\\.Database$").IsMatch(x.SelectedMemberPath));
                options.Excluding(x => new Regex("^Triggers\\[.+\\]\\.TableDatabase$").IsMatch(x.SelectedMemberPath));
                options.Excluding(x => new Regex("^Triggers\\[.+\\]\\.Database$").IsMatch(x.SelectedMemberPath));
                return options;
            });
            for (var i = 0; i < tables.Count; i++)
            {
                var table = (ABaseDbTable)tables[i];
                var clonedTable = (ABaseDbTable)clonedTables[i];

                var tableColumns = table.Columns.OrderBy(x => x.Name).Select(x => Convert.ChangeType(x, columnType, CultureInfo.InvariantCulture));
                var clonedTableColumns = clonedTable.Columns.OrderBy(x => x.Name).Select(x => Convert.ChangeType(x, columnType, CultureInfo.InvariantCulture));
                tableColumns.Should().BeEquivalentTo(clonedTableColumns, options => options.Excluding(x => ((ABaseDbColumn)x).Database));

                var tableForeignKeys = table.ForeignKeys.OrderBy(x => x.ColumnName).Select(x => Convert.ChangeType(x, foreignKeyType, CultureInfo.InvariantCulture));
                var clonedTableForeignKeys = clonedTable.ForeignKeys.OrderBy(x => x.ColumnName).Select(x => Convert.ChangeType(x, foreignKeyType, CultureInfo.InvariantCulture));
                tableForeignKeys.Should().BeEquivalentTo(clonedTableForeignKeys, options =>
                {
                    options.Excluding(x => ((ABaseDbForeignKey)x).TableDatabase);
                    options.Excluding(x => ((ABaseDbForeignKey)x).Database);
                    return options;
                });

                var tableReferencingForeignKeys = table.ReferencingForeignKeys.OrderBy(x => x.ColumnName).Select(x => Convert.ChangeType(x, foreignKeyType, CultureInfo.InvariantCulture));
                var clonedTableReferencingForeignKeys = clonedTable.ReferencingForeignKeys.OrderBy(x => x.ColumnName).Select(x => Convert.ChangeType(x, foreignKeyType, CultureInfo.InvariantCulture));
                tableReferencingForeignKeys.Should().BeEquivalentTo(clonedTableReferencingForeignKeys, options =>
                {
                    options.Excluding(x => ((ABaseDbForeignKey)x).TableDatabase);
                    options.Excluding(x => ((ABaseDbForeignKey)x).Database);
                    return options;
                });

                var tableIndexes = table.Indexes.OrderBy(x => x.ColumnName).Select(x => Convert.ChangeType(x, indexType, CultureInfo.InvariantCulture));
                var clonedTableIndexes = clonedTable.Indexes.OrderBy(x => x.ColumnName).Select(x => Convert.ChangeType(x, indexType, CultureInfo.InvariantCulture));
                tableIndexes.Should().BeEquivalentTo(clonedTableIndexes, options =>
                {
                    options.Excluding(x => ((ABaseDbForeignKey)x).TableDatabase);
                    options.Excluding(x => ((ABaseDbForeignKey)x).Database);
                    return options;
                });
            }

            var views = sourceDb.Views.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, viewType, CultureInfo.InvariantCulture)).ToList();
            var clonedViews = targetDb.Views.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, viewType, CultureInfo.InvariantCulture)).ToList();
            views.Should().BeEquivalentTo(clonedViews, options =>
            {
                options.Excluding(x => ((ABaseDbView)x).Database);
                options.Excluding(x => ((ABaseDbView)x).Indexes);
                return options;
            });
            for (var i = 0; i < views.Count; i++)
            {
                var viewIndexes = ((ABaseDbView)views[i]).Indexes.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, indexType, CultureInfo.InvariantCulture));
                var clonedViewIndexes = ((ABaseDbView)clonedViews[i]).Indexes.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, indexType, CultureInfo.InvariantCulture));
                viewIndexes.Should().BeEquivalentTo(clonedViewIndexes, options =>
                {
                    options.Excluding(x => ((ABaseDbIndex)x).TableDatabase);
                    options.Excluding(x => ((ABaseDbIndex)x).Database);
                    return options;
                });
            }

            var functions = sourceDb.Functions.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, functionType, CultureInfo.InvariantCulture));
            var clonedFunctions = targetDb.Functions.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, functionType, CultureInfo.InvariantCulture));
            functions.Should().BeEquivalentTo(clonedFunctions, options =>
            {
                options.Excluding(x => ((ABaseDbFunction)x).Database);

                if (functionType == typeof(PostgreSqlFunction))
                {
                    options.Excluding(x => ((PostgreSqlFunction)x).ReturnType);
                }

                return options;
            });

            var storedProcedures = sourceDb.StoredProcedures.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, storedProcedureType, CultureInfo.InvariantCulture));
            var clonedStoredProcedures = targetDb.StoredProcedures.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, storedProcedureType, CultureInfo.InvariantCulture));
            storedProcedures.Should().BeEquivalentTo(clonedStoredProcedures, options => options.Excluding(x => ((ABaseDbStoredProcedure)x).Database));

            var dataTypes = dataType == typeof(ABaseDbDataType) ?
                sourceDb.DataTypes.Where(x => x.IsUserDefined).OrderBy(x => x.Schema).ThenBy(x => x.Name) :
                sourceDb.DataTypes.Where(x => x.IsUserDefined).OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, dataType, CultureInfo.InvariantCulture));
            var clonedDataTypes = dataType == typeof(ABaseDbDataType) ?
                targetDb.DataTypes.Where(x => x.IsUserDefined).OrderBy(x => x.Schema).ThenBy(x => x.Name) :
                targetDb.DataTypes.Where(x => x.IsUserDefined).OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, dataType, CultureInfo.InvariantCulture));
            dataTypes.Should().BeEquivalentTo(clonedDataTypes, options =>
            {
                options.Excluding(x => ((ABaseDbDataType)x).Database);

                if (dataType == typeof(MicrosoftSqlDataType))
                {
                    options.Excluding(x => ((MicrosoftSqlDataType)x).SystemType.Database);
                }

                if (sourceDb is PostgreSqlDb)
                {
                    options.Excluding(x => ((PostgreSqlDataType)x).TypeId);
                    options.Excluding(x => ((PostgreSqlDataType)x).ArrayTypeId);
                }

                return options;
            });

            var sequences = sourceDb.Sequences.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, sequenceType, CultureInfo.InvariantCulture));
            var clonedSequences = targetDb.Sequences.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, sequenceType, CultureInfo.InvariantCulture));
            sequences.Should().BeEquivalentTo(clonedSequences, options => options.Excluding(x => ((ABaseDbSequence)x).Database));
        }

        /// <summary>
        /// Performs the compare task and wait the result
        /// </summary>
        /// <param name="projectService">The project service</param>
        internal void PerformCompareAndWaitResult(IProjectService projectService)
        {
            var taskService = new TaskService();
            var dbCompareService = new DatabaseCompareService(
                this.loggerFactory,
                projectService,
                new DatabaseService(new DatabaseProviderFactory(this.loggerFactory, new CipherService())),
                new DatabaseScripterFactory(this.loggerFactory),
                taskService);
            dbCompareService.StartCompare();

            while (!taskService.CurrentTaskInfos.All(x => x.Status == TaskStatus.RanToCompletion ||
                                                          x.Status == TaskStatus.Faulted ||
                                                          x.Status == TaskStatus.Canceled))
            {
                Thread.Sleep(200);
            }

            if (taskService.CurrentTaskInfos.Any(x => x.Status == TaskStatus.Faulted ||
                                                      x.Status == TaskStatus.Canceled))
            {
                var exception = taskService.CurrentTaskInfos.FirstOrDefault(x => x.Status == TaskStatus.Faulted ||
                                                                                 x.Status == TaskStatus.Canceled)?.Exception;
                if (exception != null)
                {
                    throw exception;
                }

                throw new XunitException("Unknown error during compare task");
            }

            projectService.Project.Result.Should().NotBeNull();
        }

        /// <summary>
        /// Executes the full alter script and compare
        /// </summary>
        /// <param name="databaseType">The database type</param>
        /// <param name="sourceDatabaseName">Name of the source database</param>
        /// <param name="targetDatabaseName">Name of the target database</param>
        /// <param name="exportFile">The export file</param>
        internal void ExecuteFullAlterScriptAndCompare(DatabaseType databaseType, string sourceDatabaseName, string targetDatabaseName, string exportFile = "")
        {
            ADatabaseProviderOptions sourceProviderOptions;
            ADatabaseProviderOptions targetProviderOptions;
            switch (databaseType)
            {
                case DatabaseType.MicrosoftSql:
                    sourceProviderOptions = this.GetMicrosoftSqlDatabaseProviderOptions(sourceDatabaseName);
                    targetProviderOptions = this.GetMicrosoftSqlDatabaseProviderOptions(targetDatabaseName);
                    break;
                case DatabaseType.MySql:
                    sourceProviderOptions = this.GetMySqlDatabaseProviderOptions(sourceDatabaseName);
                    targetProviderOptions = this.GetMySqlDatabaseProviderOptions(targetDatabaseName);
                    break;
                case DatabaseType.PostgreSql:
                    sourceProviderOptions = this.GetPostgreSqlDatabaseProviderOptions(sourceDatabaseName);
                    targetProviderOptions = this.GetPostgreSqlDatabaseProviderOptions(targetDatabaseName);
                    break;
                default:
                    throw new NotImplementedException();
            }

            // Perform the compare
            var projectService = new ProjectService(null, this.loggerFactory);
            projectService.NewProject(databaseType);
            projectService.Project.SourceProviderOptions = sourceProviderOptions;
            projectService.Project.TargetProviderOptions = targetProviderOptions;
            this.PerformCompareAndWaitResult(projectService);

            projectService.Project.Result.FullAlterScript.Should().NotBeNullOrWhiteSpace();

            // Execute the full alter script
            switch (databaseType)
            {
                case DatabaseType.MicrosoftSql:

                    if (this.exportGeneratedFullScript && !string.IsNullOrWhiteSpace(exportFile))
                    {
                        File.WriteAllText($"c:\\temp\\{exportFile}", projectService.Project.Result.FullAlterScript);
                    }

                    var mssqldbpo = this.GetMicrosoftSqlDatabaseProviderOptions(targetDatabaseName);
                    using (var context = new MicrosoftSqlDatabaseContext(this.loggerFactory, this.cipherService, mssqldbpo))
                    {
                        var queries = projectService.Project.Result.FullAlterScript.Split(new[] { "GO" + Environment.NewLine }, StringSplitOptions.None);
                        foreach (var query in queries.Where(x => !string.IsNullOrWhiteSpace(x)))
                        {
                            context.ExecuteNonQuery(query);
                        }
                    }

                    break;

                case DatabaseType.MySql:
                    // Execute the full alter script
                    var mySqlFullAlterScript = new StringBuilder();
                    /*mySqlFullAlterScript.AppendLine("SET @OLD_UNIQUE_CHECKS =@@UNIQUE_CHECKS, UNIQUE_CHECKS = 0;");
                    mySqlFullAlterScript.AppendLine("SET @OLD_FOREIGN_KEY_CHECKS =@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS = 0;");
                    mySqlFullAlterScript.AppendLine("SET @OLD_SQL_MODE =@@SQL_MODE, SQL_MODE = 'TRADITIONAL';");*/
                    mySqlFullAlterScript.AppendLine($"USE {targetDatabaseName};");
                    mySqlFullAlterScript.AppendLine(projectService.Project.Result.FullAlterScript);
                    /*mySqlFullAlterScript.AppendLine("SET SQL_MODE = @OLD_SQL_MODE;");
                    mySqlFullAlterScript.AppendLine("SET FOREIGN_KEY_CHECKS = @OLD_FOREIGN_KEY_CHECKS;");
                    mySqlFullAlterScript.AppendLine("SET UNIQUE_CHECKS = @OLD_UNIQUE_CHECKS;");*/

                    if (this.exportGeneratedFullScript && !string.IsNullOrWhiteSpace(exportFile))
                    {
                        File.WriteAllText($"c:\\temp\\{exportFile}", mySqlFullAlterScript.ToString());
                    }

                    var pathMySql = Path.GetTempFileName();
                    File.WriteAllText(pathMySql, mySqlFullAlterScript.ToString());
                    this.ExecuteMySqlScript(pathMySql);

                    break;

                case DatabaseType.PostgreSql:

                    // Execute the full alter script
                    using (var context = new PostgreSqlDatabaseContext(this.loggerFactory, this.cipherService, this.GetPostgreSqlDatabaseProviderOptions(targetDatabaseName)))
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("SET check_function_bodies = false;");

                        if (projectService.Project.Result.FullAlterScript.Contains("group_concat", StringComparison.InvariantCulture))
                        {
                            var firstFunctionFound = false;
                            var firstViewFound = false;
                            foreach (var line in projectService.Project.Result.FullAlterScript.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                            {
                                if (line.Contains("DROP FUNCTION", StringComparison.Ordinal) && !firstFunctionFound)
                                {
                                    // TODO: implement drop aggregate in scripter
                                    sb.AppendLine("DROP AGGREGATE group_concat(text);");
                                    firstFunctionFound = true;
                                }

                                if (line.Contains("CREATE VIEW", StringComparison.Ordinal) && !firstViewFound)
                                {
                                    // TODO: implement create aggregate in scripter
                                    sb.AppendLine("CREATE AGGREGATE group_concat(text)(");
                                    sb.AppendLine("    SFUNC = _group_concat,");
                                    sb.AppendLine("    STYPE = text");
                                    sb.AppendLine(");");
                                    firstViewFound = true;
                                }

                                sb.AppendLine(line);
                            }
                        }
                        else
                        {
                            sb.Append(projectService.Project.Result.FullAlterScript);
                        }

                        if (this.exportGeneratedFullScript && !string.IsNullOrWhiteSpace(exportFile))
                        {
                            File.WriteAllText($"c:\\temp\\{exportFile}", sb.ToString());
                        }

                        context.ExecuteNonQuery(sb.ToString());
                    }

                    break;
            }

            this.CompareDatabases(databaseType, targetDatabaseName, sourceDatabaseName);
        }

        /// <summary>
        /// Alters the target database then executes the full alter script and compare
        /// </summary>
        /// <param name="databaseType">The database type</param>
        /// <param name="alterScript">The script to alter the target database before the migration/comparison</param>
        internal void AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType databaseType, string alterScript)
        {
            const string sourceDatabaseName = "sakila";
            var targetDatabaseName = $"tcx_test_{Guid.NewGuid():N}";

            try
            {
                switch (databaseType)
                {
                    case DatabaseType.MicrosoftSql:
                        this.CreateMicrosoftSqlSakilaDatabase(targetDatabaseName);

                        // Do some changes in the target database
                        using (var context = new MicrosoftSqlDatabaseContext(this.loggerFactory, this.cipherService, this.GetMicrosoftSqlDatabaseProviderOptions(targetDatabaseName)))
                        {
                            context.ExecuteNonQuery(alterScript);
                        }

                        break;

                    case DatabaseType.MySql:
                        this.CreateMySqlSakilaDatabase(targetDatabaseName);

                        var alterScriptTarget = new StringBuilder();
                        alterScriptTarget.AppendLine($"USE {targetDatabaseName};");
                        alterScriptTarget.AppendLine(alterScript);

                        var pathMySql = Path.GetTempFileName();
                        File.WriteAllText(pathMySql, alterScriptTarget.ToString());
                        this.ExecuteMySqlScript(pathMySql);

                        break;

                    case DatabaseType.PostgreSql:
                        this.CreatePostgreSqlSakilaDatabase(targetDatabaseName);

                        using (var context = new PostgreSqlDatabaseContext(this.loggerFactory, this.cipherService, this.GetPostgreSqlDatabaseProviderOptions(targetDatabaseName)))
                        {
                            context.ExecuteNonQuery(alterScript);
                        }

                        break;
                }

                this.ExecuteFullAlterScriptAndCompare(databaseType, sourceDatabaseName, targetDatabaseName);
            }
            finally
            {
                switch (databaseType)
                {
                    case DatabaseType.MicrosoftSql:
                        this.DropMicrosoftSqlDatabase(targetDatabaseName);
                        break;
                    case DatabaseType.MySql:
                        this.DropMySqlDatabase(targetDatabaseName);
                        break;
                    case DatabaseType.PostgreSql:
                        this.DropPostgreSqlDatabase(targetDatabaseName);
                        break;
                }
            }
        }
    }
}
