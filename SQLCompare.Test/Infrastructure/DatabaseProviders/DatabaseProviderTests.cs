using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using SQLCompare.Core.Entities;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Database.MicrosoftSql;
using SQLCompare.Core.Entities.Database.MySql;
using SQLCompare.Core.Entities.Database.PostgreSql;
using SQLCompare.Core.Interfaces.Services;
using SQLCompare.Infrastructure.DatabaseProviders;
using SQLCompare.Infrastructure.EntityFramework;
using SQLCompare.Infrastructure.SqlScripters;
using SQLCompare.Services;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace SQLCompare.Test.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Test class for the DatabaseProvider
    /// </summary>
    public class DatabaseProviderTests : BaseTests<DatabaseProviderTests>, IClassFixture<DatabaseFixture>
    {
        private readonly bool exportGeneratedFullScript = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ExportGeneratedFullScript"));
        private readonly ICipherService cipherService = new CipherService();
        private readonly DatabaseFixture dbFixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseProviderTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper</param>
        /// <param name="dbFixture">The database fixture</param>
        public DatabaseProviderTests(ITestOutputHelper output, DatabaseFixture dbFixture)
            : base(output)
        {
            this.dbFixture = dbFixture;
            this.dbFixture.SetLoggerFactory(this.LoggerFactory);
        }

        /// <summary>
        /// Test the retrieval of database list with all the databases
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void GetDatabaseList()
        {
            var mssqldbp = this.dbFixture.GetMicrosoftSqlDatabaseProvider(false);
            var dbList = mssqldbp.GetDatabaseList();
            dbList.Should().Contain("msdb");
            dbList.Should().Contain("master");
            dbList.Should().Contain("sakila");

            var mysqldbp = this.dbFixture.GetMySqlDatabaseProvider(false);
            dbList = mysqldbp.GetDatabaseList();
            dbList.Should().Contain("sys");
            dbList.Should().Contain("mysql");
            dbList.Should().Contain("sakila");

            var pgsqldbp = this.dbFixture.GetPostgreSqlDatabaseProvider(false);
            dbList = pgsqldbp.GetDatabaseList();
            dbList.Should().Contain("postgres");
            dbList.Should().Contain("sakila");
        }

        /// <summary>
        /// Test the retrieval of the MicrosoftSQL 'sakila' database
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void GetMicrosoftSqlDatabase()
        {
            var mssqldbp = this.dbFixture.GetMicrosoftSqlDatabaseProvider();
            var db = mssqldbp.GetDatabase(new TaskInfo("test"));
            db.Should().NotBeNull();
            db.Name.Should().Be("sakila");

            db.Tables.Count.Should().Be(16);
            var table = db.Tables.FirstOrDefault(x => x.Name == "film");
            table.Should().NotBeNull();
            table.Columns.Count.Should().Be(13);
            table.Columns.Select(x => x.Name).Should().Contain("language_id");

            table = db.Tables.FirstOrDefault(x => x.Name == "film_category");
            table.Should().NotBeNull();
            table.PrimaryKeys.Count.Should().Be(2);
            table.PrimaryKeys.Should().Contain(x => x.ColumnName == "film_id");
            table.PrimaryKeys.Should().Contain(x => x.ColumnName == "category_id");

            table = db.Tables.FirstOrDefault(x => x.Name == "rental");
            table.ForeignKeys.Count.Should().Be(3);
            table.ForeignKeys.Should().Contain(x => x.Name == "fk_rental_customer");
            table.ForeignKeys.Should().Contain(x => x.Name == "fk_rental_inventory");
            table.ForeignKeys.Should().Contain(x => x.Name == "fk_rental_staff");

            db.Views.Count.Should().Be(6);
            db.Views.Should().ContainSingle(x => x.Name == "film_list");

            db.Functions.Count.Should().Be(2);
            db.StoredProcedures.Should().BeEmpty();

            db.DataTypes.Should().NotBeNullOrEmpty();
            db.DataTypes.Count.Should().Be(37);

            db.Sequences.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Test the retrieval of the MySQL 'sakila' database
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void GetMySqlDatabase()
        {
            var mysqldbp = this.dbFixture.GetMySqlDatabaseProvider();
            var db = mysqldbp.GetDatabase(new TaskInfo("test"));
            db.Should().NotBeNull();
            db.Name.Should().Be("sakila");

            db.DataTypes.Should().BeEmpty();

            db.Tables.Count.Should().Be(16);
            var table = db.Tables.FirstOrDefault(x => x.Name == "film");
            table.Should().NotBeNull();
            table.Columns.Count.Should().Be(13);
            table.Columns.Select(x => x.Name).Should().Contain("language_id");

            table = db.Tables.FirstOrDefault(x => x.Name == "film_actor");
            table.Should().NotBeNull();
            table.PrimaryKeys.Count.Should().Be(2);
            table.PrimaryKeys.Should().Contain(x => x.ColumnName == "actor_id");
            table.PrimaryKeys.Should().Contain(x => x.ColumnName == "film_id");

            table = db.Tables.FirstOrDefault(x => x.Name == "payment");
            table.ForeignKeys.Count.Should().Be(3);
            table.ForeignKeys.Should().Contain(x => x.Name == "fk_payment_customer");
            table.ForeignKeys.Should().Contain(x => x.Name == "fk_payment_rental");
            table.ForeignKeys.Should().Contain(x => x.Name == "fk_payment_staff");

            db.Views.Count.Should().Be(7);
            db.Views.Should().ContainSingle(x => x.Name == "film_list");

            db.Functions.Count.Should().Be(3);
            db.Functions.Should().ContainSingle(x => x.Name == "get_customer_balance");

            db.StoredProcedures.Count.Should().Be(3);
            db.StoredProcedures.Should().ContainSingle(x => x.Name == "film_in_stock");
        }

        /// <summary>
        /// Test the retrieval of PostgreSQL 'sakila' database
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void GetPostgreSqlDatabase()
        {
            var pgsqldbp = this.dbFixture.GetPostgreSqlDatabaseProvider();
            var db = pgsqldbp.GetDatabase(new TaskInfo("test"));
            db.Should().NotBeNull();
            db.Name.Should().Be("sakila");

            db.DataTypes.Count.Should().Be(459);

            db.Tables.Count.Should().Be(21);
            var table = db.Tables.FirstOrDefault(x => x.Name == "film");
            table.Should().NotBeNull();
            table.Columns.Count.Should().Be(14);
            table.Columns.Select(x => x.Name).Should().Contain("language_id");

            table = db.Tables.FirstOrDefault(x => x.Name == "film_category");
            table.Should().NotBeNull();
            table.PrimaryKeys.Count.Should().Be(2);
            table.PrimaryKeys.Should().Contain(x => x.ColumnName == "film_id");
            table.PrimaryKeys.Should().Contain(x => x.ColumnName == "category_id");

            table = db.Tables.FirstOrDefault(x => x.Name == "rental");
            table.ForeignKeys.Count.Should().Be(3);
            table.ForeignKeys.Should().Contain(x => x.Name == "rental_customer_id_fkey");
            table.ForeignKeys.Should().Contain(x => x.Name == "rental_inventory_id_fkey");
            table.ForeignKeys.Should().Contain(x => x.Name == "rental_staff_id_fkey");

            db.Views.Count.Should().Be(7);
            db.Views.Should().ContainSingle(x => x.Name == "film_list");

            db.Functions.Count.Should().Be(9);
            db.Functions.Should().ContainSingle(x => x.Name == "last_day");

            db.StoredProcedures.Should().BeEmpty();
        }

        /// <summary>
        /// Test cloning MicrosoftSQL 'sakila' database
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void CloneMicrosoftSqlDatabase()
        {
            var mssqldbp = this.dbFixture.GetMicrosoftSqlDatabaseProvider();
            var db = mssqldbp.GetDatabase(new TaskInfo("test"));

            var scripterFactory = new DatabaseScripterFactory(this.LoggerFactory);
            var scripter = scripterFactory.Create(db, new SQLCompare.Core.Entities.Project.ProjectOptions());
            var fullScript = scripter.GenerateFullScript(db);

            if (this.exportGeneratedFullScript)
            {
                File.WriteAllText("c:\\temp\\FullScriptMicrosoftSQL.sql", fullScript);
            }

            var mssqldbpo = this.dbFixture.GetMicrosoftSqlDatabaseProviderOptions();

            var clonedDatabaseName = $"{mssqldbpo.Database}_clone";

            // Connect without a database to drop/create the cloned one
            mssqldbpo.Database = string.Empty;
            using (var context = new MicrosoftSqlDatabaseContext(this.LoggerFactory, this.cipherService, mssqldbpo))
            {
                var dropDbQuery = new StringBuilder();
                dropDbQuery.AppendLine($"IF EXISTS(select * from sys.databases where name= '{clonedDatabaseName}')");
                dropDbQuery.AppendLine("BEGIN");
                dropDbQuery.AppendLine($"  ALTER DATABASE {clonedDatabaseName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE"); // Close existing connections
                dropDbQuery.AppendLine($"  DROP DATABASE {clonedDatabaseName}");
                dropDbQuery.AppendLine("END");
                context.ExecuteNonQuery(dropDbQuery.ToString());

                context.ExecuteNonQuery($"CREATE DATABASE {clonedDatabaseName}");
                context.ExecuteNonQuery($"USE {clonedDatabaseName}");

                var queries = fullScript.Split(new[] { "GO" + Environment.NewLine }, StringSplitOptions.None);
                foreach (var query in queries.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    context.ExecuteNonQuery(query);
                }
            }

            var dpf = new DatabaseProviderFactory(this.LoggerFactory, this.cipherService);
            mssqldbpo.Database = clonedDatabaseName;
            mssqldbp = (MicrosoftSqlDatabaseProvider)dpf.Create(mssqldbpo);

            var clonedDb = mssqldbp.GetDatabase(new TaskInfo("test"));

            CompareDatabase(db, clonedDb);
        }

        /// <summary>
        /// Test cloning PostgreSQL 'sakila' database
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void ClonePostgreSqlDatabase()
        {
            var postgresqldbp = this.dbFixture.GetPostgreSqlDatabaseProvider();
            var db = postgresqldbp.GetDatabase(new TaskInfo("test"));

            var scripterFactory = new DatabaseScripterFactory(this.LoggerFactory);
            var scripter = scripterFactory.Create(db, new SQLCompare.Core.Entities.Project.ProjectOptions());
            var fullScript = scripter.GenerateFullScript(db);

            if (this.exportGeneratedFullScript)
            {
                File.WriteAllText("c:\\temp\\FullScriptPostgreSQL.sql", fullScript);
            }

            var postgresqldbpo = this.dbFixture.GetPostgreSqlDatabaseProviderOptions();

            var clonedDatabaseName = $"{postgresqldbpo.Database}_clone";

            // Connect without a database to drop/create the cloned one
            postgresqldbpo.Database = string.Empty;
            using (var context = new PostgreSqlDatabaseContext(this.LoggerFactory, this.cipherService, postgresqldbpo))
            {
                var dropDbQuery = new StringBuilder();
                dropDbQuery.AppendLine($"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname='{clonedDatabaseName}';");
                dropDbQuery.AppendLine($"DROP DATABASE IF EXISTS {clonedDatabaseName};");
                dropDbQuery.AppendLine($"CREATE DATABASE {clonedDatabaseName};");
                context.ExecuteNonQuery(dropDbQuery.ToString());
            }

            postgresqldbpo.Database = clonedDatabaseName;
            using (var context = new PostgreSqlDatabaseContext(this.LoggerFactory, this.cipherService, postgresqldbpo))
            {
                var sb = new StringBuilder();
                sb.AppendLine("SET check_function_bodies = false;");

                var firstViewFound = false;
                foreach (var line in fullScript.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                {
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

                context.ExecuteNonQuery(sb.ToString());
            }

            var dpf = new DatabaseProviderFactory(this.LoggerFactory, this.cipherService);
            postgresqldbp = (PostgreSqlDatabaseProvider)dpf.Create(postgresqldbpo);

            var clonedDb = postgresqldbp.GetDatabase(new TaskInfo("test"));

            CompareDatabase(db, clonedDb);
        }

        /// <summary>
        /// Test cloning MySQL 'sakila' database
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void CloneMySqlDatabase()
        {
            var mysqldbp = this.dbFixture.GetMySqlDatabaseProvider();
            var db = mysqldbp.GetDatabase(new TaskInfo("test"));

            var scripterFactory = new DatabaseScripterFactory(this.LoggerFactory);
            var scripter = scripterFactory.Create(db, new SQLCompare.Core.Entities.Project.ProjectOptions());
            var fullScript = scripter.GenerateFullScript(db);

            if (this.exportGeneratedFullScript)
            {
                File.WriteAllText("c:\\temp\\FullScriptMySQL.sql", fullScript);
            }

            var mysqldbpo = this.dbFixture.GetMySqlDatabaseProviderOptions();

            var clonedDatabaseName = $"{mysqldbpo.Database}_clone";

            var mySqlFullScript = new StringBuilder();
            mySqlFullScript.AppendLine("SET @OLD_UNIQUE_CHECKS =@@UNIQUE_CHECKS, UNIQUE_CHECKS = 0;");
            mySqlFullScript.AppendLine("SET @OLD_FOREIGN_KEY_CHECKS =@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS = 0;");
            mySqlFullScript.AppendLine("SET @OLD_SQL_MODE =@@SQL_MODE, SQL_MODE = 'TRADITIONAL';");
            mySqlFullScript.AppendLine($"DROP SCHEMA IF EXISTS {clonedDatabaseName};");
            mySqlFullScript.AppendLine($"CREATE SCHEMA {clonedDatabaseName};");
            mySqlFullScript.AppendLine($"USE {clonedDatabaseName};");
            mySqlFullScript.AppendLine(fullScript.Replace($"`{mysqldbpo.Database}`.`", $"`{clonedDatabaseName}`.`", StringComparison.InvariantCulture));
            mySqlFullScript.AppendLine("SET SQL_MODE = @OLD_SQL_MODE;");
            mySqlFullScript.AppendLine("SET FOREIGN_KEY_CHECKS = @OLD_FOREIGN_KEY_CHECKS;");
            mySqlFullScript.AppendLine("SET UNIQUE_CHECKS = @OLD_UNIQUE_CHECKS;");

            var pathMySql = Path.GetTempFileName();
            File.WriteAllText(pathMySql, mySqlFullScript.ToString());

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "C:\\Program Files\\MySQL\\MySQL Server 8.0\\bin\\mysql.exe",
                Arguments = $"--user root -ptest1234 -e \"SOURCE {pathMySql}\"",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            });
            process.Should().NotBeNull();
            process.WaitForExit((int)TimeSpan.FromMinutes(5).TotalMilliseconds);
            process.ExitCode.Should().Be(0);

            mysqldbpo.Database = clonedDatabaseName;
            var dpf = new DatabaseProviderFactory(this.LoggerFactory, this.cipherService);
            mysqldbp = (MySqlDatabaseProvider)dpf.Create(mysqldbpo);

            var clonedDb = mysqldbp.GetDatabase(new TaskInfo("test"));

            CompareDatabase(db, clonedDb);
        }

        private static void CompareDatabase(ABaseDb db, ABaseDb clonedDb)
        {
            var tableType = typeof(ABaseDbTable);
            var columnType = typeof(ABaseDbColumn);
            var foreignKeyType = typeof(ABaseDbForeignKey);
            var indexType = typeof(ABaseDbIndex);
            var viewType = typeof(ABaseDbView);
            var functionType = typeof(ABaseDbFunction);
            var storedProcedureType = typeof(ABaseDbStoredProcedure);
            var dataType = typeof(ABaseDbDataType);
            var sequenceType = typeof(ABaseDbSequence);
            switch (db)
            {
                case MicrosoftSqlDb _:
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

                case MySqlDb _:
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

                case PostgreSqlDb _:
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

            var tables = db.Tables.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, tableType, CultureInfo.InvariantCulture)).ToList();
            var clonedTables = clonedDb.Tables.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, tableType, CultureInfo.InvariantCulture)).ToList();
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

            var views = db.Views.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, viewType, CultureInfo.InvariantCulture)).ToList();
            var clonedViews = clonedDb.Views.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, viewType, CultureInfo.InvariantCulture)).ToList();
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

            var functions = db.Functions.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, functionType, CultureInfo.InvariantCulture));
            var clonedFunctions = clonedDb.Functions.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, functionType, CultureInfo.InvariantCulture));
            functions.Should().BeEquivalentTo(clonedFunctions, options =>
            {
                options.Excluding(x => ((ABaseDbFunction)x).Database);

                if (functionType == typeof(PostgreSqlFunction))
                {
                    options.Excluding(x => ((PostgreSqlFunction)x).ReturnType);
                }

                return options;
            });

            var storedProcedures = db.StoredProcedures.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, storedProcedureType, CultureInfo.InvariantCulture));
            var clonedStoredProcedures = clonedDb.StoredProcedures.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, storedProcedureType, CultureInfo.InvariantCulture));
            storedProcedures.Should().BeEquivalentTo(clonedStoredProcedures, options => options.Excluding(x => ((ABaseDbStoredProcedure)x).Database));

            var dataTypes = dataType == typeof(ABaseDbDataType) ?
                db.DataTypes.Where(x => x.IsUserDefined).OrderBy(x => x.Schema).ThenBy(x => x.Name) :
                db.DataTypes.Where(x => x.IsUserDefined).OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, dataType, CultureInfo.InvariantCulture));
            var clonedDataTypes = dataType == typeof(ABaseDbDataType) ?
                clonedDb.DataTypes.Where(x => x.IsUserDefined).OrderBy(x => x.Schema).ThenBy(x => x.Name) :
                clonedDb.DataTypes.Where(x => x.IsUserDefined).OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, dataType, CultureInfo.InvariantCulture));
            dataTypes.Should().BeEquivalentTo(clonedDataTypes, options =>
            {
                options.Excluding(x => ((ABaseDbDataType)x).Database);

                if (dataType == typeof(MicrosoftSqlDataType))
                {
                    options.Excluding(x => ((MicrosoftSqlDataType)x).SystemType.Database);
                }

                if (db is PostgreSqlDb)
                {
                    options.Excluding(x => ((PostgreSqlDataType)x).TypeId);
                    options.Excluding(x => ((PostgreSqlDataType)x).ArrayTypeId);
                }

                return options;
            });

            var sequences = db.Sequences.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, sequenceType, CultureInfo.InvariantCulture));
            var clonedSequences = clonedDb.Sequences.OrderBy(x => x.Schema).ThenBy(x => x.Name).Select(x => Convert.ChangeType(x, sequenceType, CultureInfo.InvariantCulture));
            sequences.Should().BeEquivalentTo(clonedSequences, options => options.Excluding(x => ((ABaseDbSequence)x).Database));
        }
    }
}
