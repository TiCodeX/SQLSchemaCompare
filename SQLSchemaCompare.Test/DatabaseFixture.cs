namespace TiCodeX.SQLSchemaCompare.Test
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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
    using TiCodeX.SQLSchemaCompare.Infrastructure.DatabaseUtilities;
    using TiCodeX.SQLSchemaCompare.Infrastructure.SqlScripters;
    using TiCodeX.SQLSchemaCompare.Services;
    using Xunit.Sdk;

    /// <summary>
    /// Creates the sakila/pagila databases for the tests
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public abstract class DatabaseFixture : IDisposable
    {
        /// <summary>
        /// Force executing the docker tests without having to set the environment variable
        /// </summary>
        public const bool ForceDockerTests = false;

        /// <summary>
        /// The database name prefix
        /// </summary>
        public const string DatabaseNamePrefix = "tcx_test_";

        /// <summary>
        /// Whether the servers are already initialized
        /// </summary>
        private bool serversInitialized = false;

        /// <summary>
        /// Gets the cipher service
        /// </summary>
        protected ICipherService CipherService { get; } = new CipherService();

        /// <summary>
        /// Gets the logger factory
        /// </summary>
        protected ILoggerFactory LoggerFactory { get; private set; } = new XunitLoggerFactory(null);

        /// <summary>
        /// Gets the logger
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Sets the test output helper
        /// </summary>
        /// <param name="logFactory">The test output helper</param>
        public void SetLoggerFactory(ILoggerFactory logFactory)
        {
            if (logFactory == null)
            {
                throw new ArgumentNullException(nameof(logFactory));
            }

            this.LoggerFactory = logFactory;
            this.Logger = logFactory.CreateLogger(nameof(DatabaseFixture));
        }

        /// <summary>
        /// Initializes the servers
        /// </summary>
        /// <param name="serverPorts">The server ports</param>
        public void InitServers(IEnumerable<object[]> serverPorts)
        {
            if (serverPorts == null)
            {
                throw new ArgumentNullException(nameof(serverPorts));
            }

            if (this.serversInitialized)
            {
                return;
            }

            foreach (var serverPort in serverPorts)
            {
                // Delete any leftover database from previous tests
                var mssqldbp = this.GetDatabaseProvider(string.Empty, (ushort)serverPort[0]);
                foreach (var dbName in mssqldbp.GetDatabaseList().Where(x => x.StartsWith(DatabaseNamePrefix)))
                {
                    this.DropDatabase(dbName, (ushort)serverPort[0]);
                }

                this.CreateSakilaDatabase("sakila", (ushort)serverPort[0]);
            }

            this.serversInitialized = true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets the database provider
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        /// <param name="port">The port to connect to the database</param>
        /// <returns>The provider</returns>
        public IDatabaseProvider GetDatabaseProvider(string databaseName, ushort port)
        {
            var dpf = new DatabaseProviderFactory(this.LoggerFactory, this.CipherService);
            return dpf.Create(this.GetDatabaseProviderOptions(databaseName, port));
        }

        /// <summary>
        /// Executes the SQL script
        /// </summary>
        /// <param name="script">The script to execute</param>
        /// <param name="databaseName">Name of the database</param>
        /// <param name="port">The port to connect to the database</param>
        public void ExecuteScript(string script, string databaseName, ushort port)
        {
            try
            {
                this.ExecuteScriptCore(script, databaseName, port);
            }
            catch
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Failed executing script on database '{databaseName}' (port: {port}):");
                sb.AppendLine("######################################################################################################");
                sb.AppendLine(script);
                sb.AppendLine("######################################################################################################");
                this.Logger.LogError(sb.ToString());
                throw;
            }
        }

        /// <summary>
        /// Executes the SQL script
        /// </summary>
        /// <param name="script">The script to execute</param>
        /// <param name="databaseName">Name of the database</param>
        /// <param name="port">The port to connect to the database</param>
        public abstract void ExecuteScriptCore(string script, string databaseName, ushort port);

        /// <summary>
        /// Gets the database provider options
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        /// <param name="port">The port to connect to the database</param>
        /// <returns>The provider options</returns>
        public abstract ADatabaseProviderOptions GetDatabaseProviderOptions(string databaseName, ushort port);

        /// <summary>
        /// Creates the sakila database
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        /// <param name="port">The port to connect to the database</param>
        public abstract void CreateSakilaDatabase(string databaseName, ushort port);

        /// <summary>
        /// Drops the database
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        /// <param name="port">The port to connect to the database</param>
        public abstract void DropDatabase(string databaseName, ushort port);

        /// <summary>
        /// Drops the and create database
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        /// <param name="port">The port to connect to the database</param>
        public abstract void DropAndCreateDatabase(string databaseName, ushort port);

        /// <summary>
        /// Generates the name of the database.
        /// </summary>
        /// <returns>The generated database name</returns>
        internal static string GenerateDatabaseName()
        {
            return $"{DatabaseNamePrefix}{Guid.NewGuid():N}";
        }

        /// <summary>
        /// Compares the databases
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="sourceDatabaseName">Name of the source database.</param>
        /// <param name="targetDatabaseName">Name of the target database.</param>
        /// <param name="port">The port to connect to the database</param>
        internal void CompareDatabases(DatabaseType type, string sourceDatabaseName, string targetDatabaseName, ushort port)
        {
            var sourceProvider = this.GetDatabaseProvider(sourceDatabaseName, port);
            var targetProvider = this.GetDatabaseProvider(targetDatabaseName, port);

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
                case DatabaseType.MariaDb:
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

            var schemas = sourceDb.Schemas.OrderBy(x => x.Name);
            var clonedSchemas = targetDb.Schemas.OrderBy(x => x.Name);
            schemas.Should().BeEquivalentTo(clonedSchemas, options =>
            {
                options.Excluding(x => x.Database);
                return options;
            });

            var tables = sourceDb.Tables.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, tableType, CultureInfo.InvariantCulture)).ToList();
            var clonedTables = targetDb.Tables.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, tableType, CultureInfo.InvariantCulture)).ToList();
            tables.Should().BeEquivalentTo(clonedTables, options =>
            {
                options.Excluding(x => ((ABaseDbTable)x).ModifyDate);
                options.Excluding(x => ((ABaseDbTable)x).Columns);
                options.Excluding(x => ((ABaseDbTable)x).ForeignKeys);
                options.Excluding(x => ((ABaseDbTable)x).ReferencingForeignKeys);
                options.Excluding(x => ((ABaseDbTable)x).Indexes);
                options.Excluding(x => ((ABaseDbObject)x).Database);
                options.Excluding(x => new Regex("^Triggers\\[.+\\]\\.Database$").IsMatch(x.Path));
                options.Excluding(x => new Regex("^Constraints\\[.+\\]\\.Database$").IsMatch(x.Path));
                options.Excluding(x => new Regex("^PrimaryKeys\\[.+\\]\\.Database$").IsMatch(x.Path));
                return options;
            });
            for (var i = 0; i < tables.Count; i++)
            {
                var table = (ABaseDbTable)tables[i];
                var clonedTable = (ABaseDbTable)clonedTables[i];

                var tableColumns = table.Columns.OrderBy(x => x.Name).Select(x => Convert.ChangeType(x, columnType, CultureInfo.InvariantCulture));
                var clonedTableColumns = clonedTable.Columns.OrderBy(x => x.Name).Select(x => Convert.ChangeType(x, columnType, CultureInfo.InvariantCulture));
                tableColumns.Should().BeEquivalentTo(clonedTableColumns, options =>
                {
                    options.Excluding(x => ((ABaseDbColumn)x).OrdinalPosition);
                    options.Excluding(x => ((ABaseDbObject)x).Database);

                    return options;
                });

                var tableForeignKeys = table.ForeignKeys.OrderBy(x => x.ColumnName).Select(x => Convert.ChangeType(x, foreignKeyType, CultureInfo.InvariantCulture));
                var clonedTableForeignKeys = clonedTable.ForeignKeys.OrderBy(x => x.ColumnName).Select(x => Convert.ChangeType(x, foreignKeyType, CultureInfo.InvariantCulture));
                tableForeignKeys.Should().BeEquivalentTo(clonedTableForeignKeys, options =>
                {
                    options.Excluding(x => ((ABaseDbObject)x).Database);

                    return options;
                });

                var tableReferencingForeignKeys = table.ReferencingForeignKeys.OrderBy(x => x.ColumnName).Select(x => Convert.ChangeType(x, foreignKeyType, CultureInfo.InvariantCulture));
                var clonedTableReferencingForeignKeys = clonedTable.ReferencingForeignKeys.OrderBy(x => x.ColumnName).Select(x => Convert.ChangeType(x, foreignKeyType, CultureInfo.InvariantCulture));
                tableReferencingForeignKeys.Should().BeEquivalentTo(clonedTableReferencingForeignKeys, options =>
                {
                    options.Excluding(x => ((ABaseDbObject)x).Database);

                    return options;
                });

                var tableIndexes = table.Indexes.OrderBy(x => x.ColumnName).Select(x => Convert.ChangeType(x, indexType, CultureInfo.InvariantCulture));
                var clonedTableIndexes = clonedTable.Indexes.OrderBy(x => x.ColumnName).Select(x => Convert.ChangeType(x, indexType, CultureInfo.InvariantCulture));
                tableIndexes.Should().BeEquivalentTo(clonedTableIndexes, options =>
                {
                    options.Excluding(x => ((ABaseDbObject)x).Database);

                    return options;
                });
            }

            var views = sourceDb.Views.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, viewType, CultureInfo.InvariantCulture)).ToList();
            var clonedViews = targetDb.Views.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, viewType, CultureInfo.InvariantCulture)).ToList();
            views.Should().BeEquivalentTo(clonedViews, options =>
            {
                options.Excluding(x => ((ABaseDbView)x).Indexes);
                options.Excluding(x => ((ABaseDbObject)x).Database);
                return options;
            });
            for (var i = 0; i < views.Count; i++)
            {
                var viewIndexes = ((ABaseDbView)views[i]).Indexes.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, indexType, CultureInfo.InvariantCulture));
                var clonedViewIndexes = ((ABaseDbView)clonedViews[i]).Indexes.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, indexType, CultureInfo.InvariantCulture));
                viewIndexes.Should().BeEquivalentTo(clonedViewIndexes, options =>
                {
                    options.Excluding(x => ((ABaseDbObject)x).Database);

                    return options;
                });
            }

            var functions = sourceDb.Functions.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, functionType, CultureInfo.InvariantCulture));
            var clonedFunctions = targetDb.Functions.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, functionType, CultureInfo.InvariantCulture));
            functions.Should().BeEquivalentTo(clonedFunctions, options =>
            {
                if (functionType == typeof(PostgreSqlFunction))
                {
                    options.Excluding(x => ((PostgreSqlFunction)x).ReturnType);
                }

                options.Excluding(x => ((ABaseDbObject)x).Database);
                return options;
            });

            var storedProcedures = sourceDb.StoredProcedures.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, storedProcedureType, CultureInfo.InvariantCulture));
            var clonedStoredProcedures = targetDb.StoredProcedures.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, storedProcedureType, CultureInfo.InvariantCulture));
            storedProcedures.Should().BeEquivalentTo(clonedStoredProcedures, options =>
            {
                options.Excluding(x => ((ABaseDbObject)x).Database);

                return options;
            });

            var dataTypes = dataType == typeof(ABaseDbDataType) ?
                sourceDb.DataTypes.Where(x => x.IsUserDefined).OrderBy(x => x.Schema).ThenBy(x => x.Name) :
                sourceDb.DataTypes.Where(x => x.IsUserDefined).OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, dataType, CultureInfo.InvariantCulture));
            var clonedDataTypes = dataType == typeof(ABaseDbDataType) ?
                targetDb.DataTypes.Where(x => x.IsUserDefined).OrderBy(x => x.Schema).ThenBy(x => x.Name) :
                targetDb.DataTypes.Where(x => x.IsUserDefined).OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, dataType, CultureInfo.InvariantCulture));
            dataTypes.Should().BeEquivalentTo(clonedDataTypes, options =>
            {
                if (sourceDb is PostgreSqlDb)
                {
                    options.Excluding(x => ((PostgreSqlDataType)x).TypeId);
                    options.Excluding(x => ((PostgreSqlDataType)x).ArrayTypeId);
                    options.Excluding(x => ((PostgreSqlDataType)x).ArrayType.Database);
                }
                else if (sourceDb is MicrosoftSqlDb)
                {
                    options.Excluding(x => ((MicrosoftSqlDataType)x).SystemType.Database);
                }

                options.Excluding(x => ((ABaseDbObject)x).Database);

                return options;
            });

            var sequences = sourceDb.Sequences.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, sequenceType, CultureInfo.InvariantCulture));
            var clonedSequences = targetDb.Sequences.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, sequenceType, CultureInfo.InvariantCulture));
            sequences.Should().BeEquivalentTo(clonedSequences, options =>
            {
                options.Excluding(x => ((ABaseDbObject)x).Database);
                options.Excluding(x => ((ABaseDbSequence)x).IsAutoGenerated);

                return options;
            });
        }

        /// <summary>
        /// Performs the compare task and wait the result
        /// </summary>
        /// <param name="projectService">The project service</param>
        internal void PerformCompareAndWaitResult(IProjectService projectService)
        {
            using var taskService = new TaskService();
            var dbCompareService = new DatabaseCompareService(
                this.LoggerFactory,
                projectService,
                new DatabaseService(new DatabaseProviderFactory(this.LoggerFactory, new CipherService())),
                new DatabaseScripterFactory(this.LoggerFactory),
                new DatabaseMapper(),
                new DatabaseFilter(),
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
        /// <param name="port">The port to connect to the database</param>
        /// <param name="expectedDifferentItems">Amount of expected different items</param>
        internal void ExecuteFullAlterScriptAndCompare(DatabaseType databaseType, string sourceDatabaseName, string targetDatabaseName, ushort port, int? expectedDifferentItems = null)
        {
            this.ExecuteFullOrAllAlterScriptsAndCompare(databaseType, sourceDatabaseName, targetDatabaseName, port, true, expectedDifferentItems);
        }

        /// <summary>
        /// Executes all the alter scripts and compare
        /// </summary>
        /// <param name="databaseType">The database type</param>
        /// <param name="sourceDatabaseName">Name of the source database</param>
        /// <param name="targetDatabaseName">Name of the target database</param>
        /// <param name="port">The port to connect to the database</param>
        /// <param name="expectedDifferentItems">Amount of expected different items</param>
        internal void ExecuteAllAlterScriptsAndCompare(DatabaseType databaseType, string sourceDatabaseName, string targetDatabaseName, ushort port, int? expectedDifferentItems = null)
        {
            this.ExecuteFullOrAllAlterScriptsAndCompare(databaseType, sourceDatabaseName, targetDatabaseName, port, false, expectedDifferentItems);
        }

        /// <summary>
        /// Executes the full or all the alter scripts and compare
        /// </summary>
        /// <param name="databaseType">The database type</param>
        /// <param name="sourceDatabaseName">Name of the source database</param>
        /// <param name="targetDatabaseName">Name of the target database</param>
        /// <param name="port">The port to connect to the database</param>
        /// <param name="performFullAlterScript">Whether to run the full alter script or the alter script of every different item</param>
        /// <param name="expectedDifferentItems">Amount of expected different items</param>
        /// <param name="reverseExecutionOrder">Reverse execution order of the alter scripts</param>
        internal void ExecuteFullOrAllAlterScriptsAndCompare(DatabaseType databaseType, string sourceDatabaseName, string targetDatabaseName, ushort port, bool performFullAlterScript, int? expectedDifferentItems = null, bool reverseExecutionOrder = false)
        {
            // Perform the compare
            var projectService = new ProjectService(null, this.LoggerFactory);
            projectService.NewProject(databaseType);
            projectService.Project.SourceProviderOptions = this.GetDatabaseProviderOptions(sourceDatabaseName, port);
            projectService.Project.TargetProviderOptions = this.GetDatabaseProviderOptions(targetDatabaseName, port);
            this.PerformCompareAndWaitResult(projectService);

            if (expectedDifferentItems.HasValue)
            {
                projectService.Project.Result.DifferentItems.Count.Should().Be(expectedDifferentItems);
            }

            var scriptsToRun = new List<string>();
            if (performFullAlterScript)
            {
                scriptsToRun.Add(projectService.Project.Result.FullAlterScript);
            }
            else
            {
                scriptsToRun.AddRange(projectService.Project.Result.OnlyTargetItems.Select(x => x.Scripts.AlterScript));
                scriptsToRun.AddRange(projectService.Project.Result.OnlySourceItems.Select(x => x.Scripts.AlterScript));

                var diffItemsScripts = projectService.Project.Result.DifferentItems.Select(x => x.Scripts.AlterScript).ToList();
                if (reverseExecutionOrder)
                {
                    diffItemsScripts.Reverse();
                }

                scriptsToRun.AddRange(diffItemsScripts);
            }

            foreach (var script in scriptsToRun)
            {
                script.Should().NotBeNullOrWhiteSpace();

                switch (databaseType)
                {
                    case DatabaseType.MicrosoftSql:
                    case DatabaseType.MySql:
                    case DatabaseType.MariaDb:
                        this.ExecuteScript(script, targetDatabaseName, port);
                        break;

                    case DatabaseType.PostgreSql:
                        var postgreSqlScript = new StringBuilder();
                        postgreSqlScript.AppendLine("SET check_function_bodies = false;");
                        postgreSqlScript.AppendLine(script);
                        this.ExecuteScript(postgreSqlScript.ToString(), targetDatabaseName, port);
                        break;
                }
            }

            // Swap source/target for the comparison
            (sourceDatabaseName, targetDatabaseName) = (targetDatabaseName, sourceDatabaseName);

            this.CompareDatabases(databaseType, sourceDatabaseName, targetDatabaseName, port);
        }

        /// <summary>
        /// Alters the target database then executes the full alter script and compare and also all the alter scripts and compare
        /// </summary>
        /// <param name="databaseType">The database type</param>
        /// <param name="alterScript">The script to alter the target database before the migration/comparison</param>
        /// <param name="port">The port to connect to the database</param>
        /// <param name="expectedDifferentItems">Amount of expected different items</param>
        /// <param name="reverseExecutionOrder">Reverse execution order of the alter scripts</param>
        internal void AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType databaseType, string alterScript, ushort port, int expectedDifferentItems = 1, bool reverseExecutionOrder = false)
        {
            this.AlterTargetDatabaseExecuteFullOrAllAlterScriptsAndCompare(databaseType, alterScript, port, true, expectedDifferentItems, reverseExecutionOrder);
            this.AlterTargetDatabaseExecuteFullOrAllAlterScriptsAndCompare(databaseType, alterScript, port, false, expectedDifferentItems, reverseExecutionOrder);
        }

        /// <summary>
        /// Alters the target database then executes the full or all alter scripts and compare
        /// </summary>
        /// <param name="databaseType">The database type</param>
        /// <param name="alterScript">The script to alter the target database before the migration/comparison</param>
        /// <param name="port">The port to connect to the database</param>
        /// <param name="performFullAlterScript">Whether to run the full alter script or the alter script of every different item</param>
        /// <param name="expectedDifferentItems">Amount of expected different items</param>
        /// <param name="reverseExecutionOrder">Reverse execution order of the alter scripts</param>
        internal void AlterTargetDatabaseExecuteFullOrAllAlterScriptsAndCompare(DatabaseType databaseType, string alterScript, ushort port, bool performFullAlterScript, int expectedDifferentItems = 1, bool reverseExecutionOrder = false)
        {
            const string sourceDatabaseName = "sakila";
            var targetDatabaseName = GenerateDatabaseName();

            try
            {
                this.CreateSakilaDatabase(targetDatabaseName, port);

                this.ExecuteScript(alterScript, targetDatabaseName, port);

                this.ExecuteFullOrAllAlterScriptsAndCompare(databaseType, sourceDatabaseName, targetDatabaseName, port, performFullAlterScript, expectedDifferentItems, reverseExecutionOrder);
            }
            finally
            {
                this.DropDatabase(targetDatabaseName, port);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.LoggerFactory?.Dispose();
            }
        }
    }
}
