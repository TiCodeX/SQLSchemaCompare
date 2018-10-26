using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using SQLCompare.Core.Entities;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Database.MicrosoftSql;
using SQLCompare.Core.Entities.Database.MySql;
using SQLCompare.Core.Entities.Database.PostgreSql;
using SQLCompare.Core.Enums;
using SQLCompare.Core.Interfaces;
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
            var mssqldbp = this.dbFixture.GetMicrosoftSqlDatabaseProvider();
            var dbList = mssqldbp.GetDatabaseList();
            dbList.Should().Contain("msdb");
            dbList.Should().Contain("master");
            dbList.Should().Contain("sakila");

            var mysqldbp = this.dbFixture.GetMySqlDatabaseProvider();
            dbList = mysqldbp.GetDatabaseList();
            dbList.Should().Contain("sys");
            dbList.Should().Contain("mysql");
            dbList.Should().Contain("sakila");

            var pgsqldbp = this.dbFixture.GetPostgreSqlDatabaseProvider();
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
            var mssqldbp = this.dbFixture.GetMicrosoftSqlDatabaseProvider("sakila");
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
            var mysqldbp = this.dbFixture.GetMySqlDatabaseProvider("sakila");
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
            var pgsqldbp = this.dbFixture.GetPostgreSqlDatabaseProvider("sakila");
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
            const string databaseName = "sakila";
            const string clonedDatabaseName = "sakila_clone";

            var mssqldbp = this.dbFixture.GetMicrosoftSqlDatabaseProvider(databaseName);
            var db = mssqldbp.GetDatabase(new TaskInfo("test"));

            var scripterFactory = new DatabaseScripterFactory(this.LoggerFactory);
            var scripter = scripterFactory.Create(db, new SQLCompare.Core.Entities.Project.ProjectOptions());
            var fullScript = scripter.GenerateFullCreateScript(db);

            if (this.exportGeneratedFullScript)
            {
                File.WriteAllText("c:\\temp\\FullScriptMicrosoftSQL.sql", fullScript);
            }

            this.dbFixture.DropAndCreateMicrosoftSqlDatabase(clonedDatabaseName);

            var mssqldbpo = this.dbFixture.GetMicrosoftSqlDatabaseProviderOptions(clonedDatabaseName);
            using (var context = new MicrosoftSqlDatabaseContext(this.LoggerFactory, this.cipherService, mssqldbpo))
            {
                var queries = fullScript.Split(new[] { "GO" + Environment.NewLine }, StringSplitOptions.None);
                foreach (var query in queries.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    context.ExecuteNonQuery(query);
                }
            }

            this.CompareDatabases(DatabaseType.MicrosoftSql, clonedDatabaseName, databaseName);
        }

        /// <summary>
        /// Test cloning PostgreSQL 'sakila' database
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void ClonePostgreSqlDatabase()
        {
            const string databaseName = "sakila";
            const string clonedDatabaseName = "sakila_clone";

            var postgresqldbp = this.dbFixture.GetPostgreSqlDatabaseProvider(databaseName);
            var db = postgresqldbp.GetDatabase(new TaskInfo("test"));

            var scripterFactory = new DatabaseScripterFactory(this.LoggerFactory);
            var scripter = scripterFactory.Create(db, new SQLCompare.Core.Entities.Project.ProjectOptions());
            var fullScript = scripter.GenerateFullCreateScript(db);

            if (this.exportGeneratedFullScript)
            {
                File.WriteAllText("c:\\temp\\FullScriptPostgreSQL.sql", fullScript);
            }

            this.dbFixture.DropAndCreatePostgreSqlDatabase(clonedDatabaseName);

            var postgresqldbpo = this.dbFixture.GetPostgreSqlDatabaseProviderOptions(clonedDatabaseName);
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

            this.CompareDatabases(DatabaseType.PostgreSql, clonedDatabaseName, databaseName);
        }

        /// <summary>
        /// Test cloning MySQL 'sakila' database
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void CloneMySqlDatabase()
        {
            const string databaseName = "sakila";
            const string clonedDatabaseName = "sakila_clone";

            var mysqldbp = this.dbFixture.GetMySqlDatabaseProvider(databaseName);
            var db = mysqldbp.GetDatabase(new TaskInfo("test"));

            var scripterFactory = new DatabaseScripterFactory(this.LoggerFactory);
            var scripter = scripterFactory.Create(db, new SQLCompare.Core.Entities.Project.ProjectOptions());
            var fullScript = scripter.GenerateFullCreateScript(db);

            if (this.exportGeneratedFullScript)
            {
                File.WriteAllText("c:\\temp\\FullScriptMySQL.sql", fullScript);
            }

            var mySqlFullScript = new StringBuilder();
            mySqlFullScript.AppendLine("SET @OLD_UNIQUE_CHECKS =@@UNIQUE_CHECKS, UNIQUE_CHECKS = 0;");
            mySqlFullScript.AppendLine("SET @OLD_FOREIGN_KEY_CHECKS =@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS = 0;");
            mySqlFullScript.AppendLine("SET @OLD_SQL_MODE =@@SQL_MODE, SQL_MODE = 'TRADITIONAL';");
            mySqlFullScript.AppendLine($"DROP SCHEMA IF EXISTS {clonedDatabaseName};");
            mySqlFullScript.AppendLine($"CREATE SCHEMA {clonedDatabaseName};");
            mySqlFullScript.AppendLine($"USE {clonedDatabaseName};");
            mySqlFullScript.AppendLine(fullScript.Replace($"`{databaseName}`.`", $"`{clonedDatabaseName}`.`", StringComparison.InvariantCulture));
            mySqlFullScript.AppendLine("SET SQL_MODE = @OLD_SQL_MODE;");
            mySqlFullScript.AppendLine("SET FOREIGN_KEY_CHECKS = @OLD_FOREIGN_KEY_CHECKS;");
            mySqlFullScript.AppendLine("SET UNIQUE_CHECKS = @OLD_UNIQUE_CHECKS;");

            var pathMySql = Path.GetTempFileName();
            File.WriteAllText(pathMySql, mySqlFullScript.ToString());

            this.dbFixture.ExecuteMySqlScript(pathMySql);

            this.CompareDatabases(DatabaseType.MySql, clonedDatabaseName, databaseName);
        }

        /// <summary>
        /// Test migration script when source db is empty (a.k.a. drop whole target)
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseSourceEmpty()
        {
            const string sourceDatabaseName = "sakila_empty";
            const string targetDatabaseName = "sakila_migrated_to_empty";

            // Create the empty database
            this.dbFixture.DropAndCreateMicrosoftSqlDatabase(sourceDatabaseName);

            // Create the database with sakila to be migrated to empty
            this.dbFixture.CreateMicrosoftSqlSakilaDatabase(targetDatabaseName);

            // Perform the compare
            var projectService = new ProjectService(null, this.LoggerFactory);
            projectService.NewProject(DatabaseType.MicrosoftSql);
            projectService.Project.SourceProviderOptions = this.dbFixture.GetMicrosoftSqlDatabaseProviderOptions(sourceDatabaseName);
            projectService.Project.TargetProviderOptions = this.dbFixture.GetMicrosoftSqlDatabaseProviderOptions(targetDatabaseName);
            this.PerformCompareAndWaitResult(projectService);
            projectService.Project.Result.FullAlterScript.Should().NotBeNullOrWhiteSpace();

            if (this.exportGeneratedFullScript)
            {
                File.WriteAllText("c:\\temp\\FullDropScriptMicrosoftSQL.sql", projectService.Project.Result.FullAlterScript);
            }

            // Execute the full alter script
            var mssqldbpo = this.dbFixture.GetMicrosoftSqlDatabaseProviderOptions(targetDatabaseName);
            using (var context = new MicrosoftSqlDatabaseContext(this.LoggerFactory, this.cipherService, mssqldbpo))
            {
                var queries = projectService.Project.Result.FullAlterScript.Split(new[] { "GO" + Environment.NewLine }, StringSplitOptions.None);
                foreach (var query in queries.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    context.ExecuteNonQuery(query);
                }
            }

            this.CompareDatabases(DatabaseType.MicrosoftSql, targetDatabaseName, sourceDatabaseName);
        }

        /// <summary>
        /// Test migration script when source db is empty (a.k.a. re-create whole source)
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetEmpty()
        {
            const string sourceDatabaseName = "sakila";
            const string targetDatabaseName = "sakila_migrated_from_empty";

            this.dbFixture.DropAndCreateMicrosoftSqlDatabase(targetDatabaseName);

            // Perform the compare
            var projectService = new ProjectService(null, this.LoggerFactory);
            projectService.NewProject(DatabaseType.MicrosoftSql);
            projectService.Project.SourceProviderOptions = this.dbFixture.GetMicrosoftSqlDatabaseProviderOptions(sourceDatabaseName);
            projectService.Project.TargetProviderOptions = this.dbFixture.GetMicrosoftSqlDatabaseProviderOptions(targetDatabaseName);
            this.PerformCompareAndWaitResult(projectService);
            projectService.Project.Result.FullAlterScript.Should().NotBeNullOrWhiteSpace();

            // Execute the full alter script
            var mssqldbpo = this.dbFixture.GetMicrosoftSqlDatabaseProviderOptions(targetDatabaseName);
            using (var context = new MicrosoftSqlDatabaseContext(this.LoggerFactory, this.cipherService, mssqldbpo))
            {
                var queries = projectService.Project.Result.FullAlterScript.Split(new[] { "GO" + Environment.NewLine }, StringSplitOptions.None);
                foreach (var query in queries.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    context.ExecuteNonQuery(query);
                }
            }

            this.CompareDatabases(DatabaseType.MicrosoftSql, targetDatabaseName, sourceDatabaseName);
        }

        private void CompareDatabases(DatabaseType type, string sourceDatabaseName, string targetDatabaseName)
        {
            IDatabaseProvider sourceProvider = null;
            IDatabaseProvider targetProvider = null;
            switch (type)
            {
                case DatabaseType.MicrosoftSql:
                    sourceProvider = this.dbFixture.GetMicrosoftSqlDatabaseProvider(sourceDatabaseName);
                    targetProvider = this.dbFixture.GetMicrosoftSqlDatabaseProvider(targetDatabaseName);
                    break;
                case DatabaseType.MySql:
                    sourceProvider = this.dbFixture.GetMySqlDatabaseProvider(sourceDatabaseName);
                    targetProvider = this.dbFixture.GetMySqlDatabaseProvider(targetDatabaseName);
                    break;
                case DatabaseType.PostgreSql:
                    sourceProvider = this.dbFixture.GetPostgreSqlDatabaseProvider(sourceDatabaseName);
                    targetProvider = this.dbFixture.GetPostgreSqlDatabaseProvider(targetDatabaseName);
                    break;
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

        private void PerformCompareAndWaitResult(IProjectService projectService)
        {
            var taskService = new TaskService();
            var dbCompareService = new DatabaseCompareService(
                this.LoggerFactory,
                projectService,
                new DatabaseService(new DatabaseProviderFactory(this.LoggerFactory, new CipherService())),
                new DatabaseScripterFactory(this.LoggerFactory),
                taskService);
            dbCompareService.StartCompare();

            while (!taskService.CurrentTaskInfos.All(x => x.Status == TaskStatus.RanToCompletion ||
                                                          x.Status == TaskStatus.Faulted ||
                                                          x.Status == TaskStatus.Canceled))
            {
                Thread.Sleep(200);
            }

            taskService.CurrentTaskInfos.Any(x => x.Status == TaskStatus.Faulted ||
                                                  x.Status == TaskStatus.Canceled).Should().BeFalse();
            projectService.Project.Result.Should().NotBeNull();
        }
    }
}
