﻿using System.Linq;
using FluentAssertions;
using TiCodeX.SQLSchemaCompare.Core.Entities;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace TiCodeX.SQLSchemaCompare.Test.Infrastructure.DatabaseProviders
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

            db.Tables.Count.Should().Be(18);
            var table = db.Tables.FirstOrDefault(x => x.Name == "film");
            table.Should().NotBeNull();
            table.Columns.Count.Should().Be(13);
            table.Columns.Select(x => x.Name).Should().Contain("language_id");

            table = db.Tables.FirstOrDefault(x => x.Name == "film_category");
            table.Should().NotBeNull();
            table.PrimaryKeys.Count.Should().Be(1);
            table.PrimaryKeys.First().ColumnNames.Should().Contain("film_id");
            table.PrimaryKeys.First().ColumnNames.Should().Contain("category_id");

            table = db.Tables.FirstOrDefault(x => x.Name == "rental");
            table.ForeignKeys.Count.Should().Be(3);
            table.ForeignKeys.Should().Contain(x => x.Name == "fk_rental_customer");
            table.ForeignKeys.Should().Contain(x => x.Name == "fk_rental_inventory");
            table.ForeignKeys.Should().Contain(x => x.Name == "fk_rental_staff");

            db.Views.Count.Should().Be(6);
            db.Views.Should().ContainSingle(x => x.Name == "film_list");

            db.Functions.Count.Should().Be(2);
            db.StoredProcedures.Count.Should().Be(1);

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

            db.Tables.Count.Should().Be(17);
            var table = db.Tables.FirstOrDefault(x => x.Name == "film");
            table.Should().NotBeNull();
            table.Columns.Count.Should().Be(13);
            table.Columns.Select(x => x.Name).Should().Contain("language_id");

            table = db.Tables.FirstOrDefault(x => x.Name == "film_actor");
            table.Should().NotBeNull();
            table.PrimaryKeys.Count.Should().Be(1);
            table.PrimaryKeys.First().ColumnNames.Should().Contain("actor_id");
            table.PrimaryKeys.First().ColumnNames.Should().Contain("film_id");

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

            db.DataTypes.Count.Should().Be(462);

            db.Tables.Count.Should().Be(22);
            var table = db.Tables.FirstOrDefault(x => x.Name == "film");
            table.Should().NotBeNull();
            table.Columns.Count.Should().Be(14);
            table.Columns.Select(x => x.Name).Should().Contain("language_id");

            table = db.Tables.FirstOrDefault(x => x.Name == "film_category");
            table.Should().NotBeNull();
            table.PrimaryKeys.Count.Should().Be(1);
            table.PrimaryKeys.First().ColumnNames.Should().Contain("film_id");
            table.PrimaryKeys.First().ColumnNames.Should().Contain("category_id");

            table = db.Tables.FirstOrDefault(x => x.Name == "rental");
            table.ForeignKeys.Count.Should().Be(3);
            table.ForeignKeys.Should().Contain(x => x.Name == "rental_customer_id_fkey");
            table.ForeignKeys.Should().Contain(x => x.Name == "rental_inventory_id_fkey");
            table.ForeignKeys.Should().Contain(x => x.Name == "rental_staff_id_fkey");

            db.Views.Count.Should().Be(7);
            db.Views.Should().ContainSingle(x => x.Name == "film_list");

            db.Functions.Count.Should().Be(13);
            db.Functions.Should().ContainSingle(x => x.Name == "last_day");

            db.StoredProcedures.Should().BeEmpty();
        }

        /// <summary>
        /// Test various PostgreSQL versions (9.3, 9.4, 9.5, 9.6, 10, 11)
        /// </summary>
        [Fact(Skip = "Requires docker")]
        [IntegrationTest]
        public void GetPostgreSqlDatabaseAllVersions()
        {
            const string databaseName = "sakila";
            const short startPort = 26001;
            const short endPort = 26006;

            // Create the databases
            for (var port = startPort; port <= endPort; port++)
            {
                var dpo = this.dbFixture.GetPostgreSqlDatabaseProviderOptions(string.Empty);
                dpo.Port = port;
                this.dbFixture.CreatePostgreSqlSakilaDatabase(databaseName, dpo);
            }

            // Retrieve the databases
            for (var port = startPort; port <= endPort; port++)
            {
                var dpo = this.dbFixture.GetPostgreSqlDatabaseProviderOptions(databaseName);
                dpo.Port = port;
                var pgsqldbp = this.dbFixture.GetPostgreSqlDatabaseProvider(databaseName, dpo);
                pgsqldbp.GetDatabase(new TaskInfo("test"));
            }
        }

        /// <summary>
        /// Test various MySQL versions (5.5, 5.6, 5.7. 8.0)
        /// </summary>
        [Fact(Skip = "Requires docker")]
        [IntegrationTest]
        public void GetMySqlDatabaseAllVersions()
        {
            const string databaseName = "sakila";
            const short startPort = 27001;
            const short endPort = 27004;

            // Create the databases
            for (var port = startPort; port <= endPort; port++)
            {
                this.dbFixture.CreateMySqlSakilaDatabase(databaseName, port);
            }

            // Retrieve the databases
            for (var port = startPort; port <= endPort; port++)
            {
                var dpo = this.dbFixture.GetMySqlDatabaseProviderOptions(databaseName);
                dpo.Port = port;
                var pgsqldbp = this.dbFixture.GetMySqlDatabaseProvider(databaseName, dpo);
                pgsqldbp.GetDatabase(new TaskInfo("test"));
            }
        }
    }
}
