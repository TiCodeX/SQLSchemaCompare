using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using SQLCompare.Core.Entities;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Infrastructure.DatabaseProviders;
using SQLCompare.Infrastructure.EntityFramework;
using SQLCompare.Infrastructure.SqlScripters;
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

            db.Views.Count.Should().Be(5);
            db.Views.Should().ContainSingle(x => x.Name == "film_list");

            db.Functions.Should().BeEmpty();
            db.StoredProcedures.Should().BeEmpty();

            db.Triggers.Should().NotBeNullOrEmpty();
            db.Triggers.Count.Should().Be(1);
            db.Triggers.First().Name.Should().Be("reminder1");

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

            var mssqldbpo = this.dbFixture.GetMicrosoftSqlDatabaseProviderOptions();

            var clonedDatabaseName = $"{mssqldbpo.Database}_clone";

            // Connect without a database to drop/create the cloned one
            mssqldbpo.Database = string.Empty;
            using (var context = new MicrosoftSqlDatabaseContext(this.LoggerFactory, mssqldbpo))
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

            var dpf = new DatabaseProviderFactory(this.LoggerFactory);
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

            var postgresqldbpo = this.dbFixture.GetPostgreSqlDatabaseProviderOptions();

            var clonedDatabaseName = $"{postgresqldbpo.Database}_clone";

            // Connect without a database to drop/create the cloned one
            postgresqldbpo.Database = string.Empty;
            using (var context = new PostgreSqlDatabaseContext(this.LoggerFactory, postgresqldbpo))
            {
                var dropDbQuery = new StringBuilder();
                dropDbQuery.AppendLine($"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname='{clonedDatabaseName}';");
                dropDbQuery.AppendLine($"DROP DATABASE IF EXISTS {clonedDatabaseName};");
                dropDbQuery.AppendLine($"CREATE DATABASE {clonedDatabaseName};");
                context.ExecuteNonQuery(dropDbQuery.ToString());
            }

            postgresqldbpo.Database = clonedDatabaseName;
            using (var context = new PostgreSqlDatabaseContext(this.LoggerFactory, postgresqldbpo))
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

            var dpf = new DatabaseProviderFactory(this.LoggerFactory);
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
            var dpf = new DatabaseProviderFactory(this.LoggerFactory);
            mysqldbp = (MySqlDatabaseProvider)dpf.Create(mysqldbpo);

            var clonedDb = mysqldbp.GetDatabase(new TaskInfo("test"));

            CompareDatabase(db, clonedDb);
        }

        private static void CompareDatabase(ABaseDb db, ABaseDb clonedDb)
        {
            // TODO: implement checks for child classes (e.g.: MicrosoftSqlColumn, ...)
            var tables = db.Tables.OrderBy(x => x.Schema).ThenBy(x => x.Name);
            var clonedTables = clonedDb.Tables.OrderBy(x => x.Schema).ThenBy(x => x.Name);
            tables.Should().BeEquivalentTo(clonedTables, options =>
            {
                options.Excluding(x => x.Database);
                options.Excluding(x => x.ModifyDate);
                options.Excluding(x => new Regex("^Columns\\[.+\\]\\.Database$").IsMatch(x.SelectedMemberPath));
                options.Excluding(x => new Regex("^PrimaryKeys\\[.+\\]\\.TableDatabase$").IsMatch(x.SelectedMemberPath));
                options.Excluding(x => new Regex("^PrimaryKeys\\[.+\\]\\.Database$").IsMatch(x.SelectedMemberPath));
                options.Excluding(x => new Regex("^ForeignKeys\\[.+\\]\\.TableDatabase$").IsMatch(x.SelectedMemberPath));
                options.Excluding(x => new Regex("^ForeignKeys\\[.+\\]\\.Database$").IsMatch(x.SelectedMemberPath));
                options.Excluding(x => new Regex("^Indexes\\[.+\\]\\.TableDatabase$").IsMatch(x.SelectedMemberPath));
                options.Excluding(x => new Regex("^Indexes\\[.+\\]\\.Database$").IsMatch(x.SelectedMemberPath));
                options.Excluding(x => new Regex("^Constraints\\[.+\\]\\.TableDatabase$").IsMatch(x.SelectedMemberPath));
                options.Excluding(x => new Regex("^Constraints\\[.+\\]\\.Database$").IsMatch(x.SelectedMemberPath));
                return options;
            });

            var views = db.Views.OrderBy(x => x.Schema).ThenBy(x => x.Name);
            var clonedViews = clonedDb.Views.OrderBy(x => x.Schema).ThenBy(x => x.Name);
            views.Should().BeEquivalentTo(clonedViews, options => options.Excluding(x => x.Database));

            var functions = db.Functions.OrderBy(x => x.Schema).ThenBy(x => x.Name);
            var clonedFunctions = clonedDb.Functions.OrderBy(x => x.Schema).ThenBy(x => x.Name);
            functions.Should().BeEquivalentTo(clonedFunctions, options => options.Excluding(x => x.Database));

            var storedProcedures = db.StoredProcedures.OrderBy(x => x.Schema).ThenBy(x => x.Name);
            var clonedStoredProcedures = clonedDb.StoredProcedures.OrderBy(x => x.Schema).ThenBy(x => x.Name);
            storedProcedures.Should().BeEquivalentTo(clonedStoredProcedures, options => options.Excluding(x => x.Database));

            var triggers = db.Triggers.OrderBy(x => x.Schema).ThenBy(x => x.Name).AsEnumerable();
            var clonedTriggers = clonedDb.Triggers.OrderBy(x => x.Schema).ThenBy(x => x.Name).AsEnumerable();
            triggers.Should().BeEquivalentTo(clonedTriggers, options =>
            {
                options.Excluding(x => x.Database);
                options.Excluding(x => x.TableDatabase);
                return options;
            });

            var dataTypes = db.DataTypes.Where(x => x.IsUserDefined).OrderBy(x => x.Schema).ThenBy(x => x.Name);
            var clonedDataTypes = clonedDb.DataTypes.Where(x => x.IsUserDefined).OrderBy(x => x.Schema).ThenBy(x => x.Name);
            dataTypes.Should().BeEquivalentTo(clonedDataTypes, options => options.Excluding(x => x.Database));

            var sequences = db.Sequences.OrderBy(x => x.Schema).ThenBy(x => x.Name);
            var clonedSequences = clonedDb.Sequences.OrderBy(x => x.Schema).ThenBy(x => x.Name);
            sequences.Should().BeEquivalentTo(clonedSequences, options => options.Excluding(x => x.Database));
        }
    }
}
