using System;
using FluentAssertions;
using SQLCompare.Core.Entities.Database.MySql;
using SQLCompare.Core.Entities.Database.PostgreSql;
using SQLCompare.Infrastructure.DatabaseProviders;
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
            dbList.Should().Contain("brokerpro");
            dbList.Should().Contain("brokerpro_web");
            dbList.Should().Contain("BrokerProGlobal");
            dbList.Should().Contain("msdb");
            dbList.Should().Contain("master");
            dbList.Should().Contain("sakila");

            var mysqldbp = this.dbFixture.GetMySqlDatabaseProvider(false);
            dbList = mysqldbp.GetDatabaseList();
            dbList.Should().Contain("sys");
            dbList.Should().Contain("mysql");
            dbList.Should().Contain("sakila");

            var pgsqldbp = this.dbFixture.GetPostgreDatabaseProvider(false);
            dbList = pgsqldbp.GetDatabaseList();
            dbList.Should().Contain("postgres");
            dbList.Should().Contain("sakila");
        }

        /// <summary>
        /// Test the retrieval of specific database with all the databases
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void GetMySqlDatabase()
        {
            var dpf = new DatabaseProviderFactory(this.LoggerFactory);

            var mysqldbp = this.dbFixture.GetMySqlDatabaseProvider();

            var db = mysqldbp.GetDatabase();
        }

        /// <summary>
        /// Test the retrieval of specific database with all the databases
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void GetPostgreSqlDatabase()
        {
            var dpf = new DatabaseProviderFactory(this.LoggerFactory);

            var pgsqldbp = this.dbFixture.GetPostgreDatabaseProvider();

            var db = pgsqldbp.GetDatabase();
        }

        /// <summary>
        /// Test the retrieval of specific database with all the databases
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void GetDatabase()
        {
            var mssqldbp = this.dbFixture.GetMicrosoftSqlDatabaseProvider();
            var db = mssqldbp.GetDatabase();
            Assert.Equal("brokerpro", db.Name);
            Assert.Equal(218, db.Tables.Count);
            var table = db.Tables.Find(x => x.Name.Equals("DeletedDocumentReference", StringComparison.Ordinal));
            Assert.True(table.Columns.Count == 4);
            Assert.Contains(table.Columns, x => x.Name.Equals("OriginalTable", StringComparison.Ordinal));
            Assert.True(table.PrimaryKeys.Count == 3);
            Assert.Contains(table.PrimaryKeys, x => x.Name.Equals("PK_DeletedDocumentReference", StringComparison.Ordinal));
            Assert.True(table.ForeignKeys.Count == 2);
            Assert.Contains(table.ForeignKeys, x => x.Name.Equals("FK_DeletedDocumentReference_DeletedDocument", StringComparison.Ordinal));
            table = db.Tables.Find(x => x.Name.Equals("debitors", StringComparison.Ordinal));
            var dsf = new DatabaseScripterFactory(this.LoggerFactory);
            var script = dsf.Create(db, new SQLCompare.Core.Entities.Project.ProjectOptions());
            var t = script.GenerateCreateTableScript(table);
            Assert.True(t != null);

            var mysqldbp = this.dbFixture.GetMySqlDatabaseProvider();
            db = mysqldbp.GetDatabase();
            Assert.Equal("sakila", db.Name);
            Assert.Equal(18, db.Tables.Count);
            table = db.Tables.Find(x => x.Name.Equals("film", StringComparison.Ordinal));
            Assert.True(table.Columns.Count == 13);
            Assert.Contains(table.Columns, x => x.Name.Equals("language_id", StringComparison.Ordinal));
            table = db.Tables.Find(x => x.Name.Equals("film_actor", StringComparison.Ordinal));
            Assert.True(table.PrimaryKeys.Count == 2);
            Assert.Contains(table.PrimaryKeys, x => ((MySqlPrimaryKey)x).ColumnName.Equals("actor_id", StringComparison.Ordinal));
            Assert.Contains(table.PrimaryKeys, x => ((MySqlPrimaryKey)x).ColumnName.Equals("film_id", StringComparison.Ordinal));
            table = db.Tables.Find(x => x.Name.Equals("payment", StringComparison.Ordinal));
            Assert.True(table.ForeignKeys.Count == 3);
            Assert.Contains(table.ForeignKeys, x => x.Name.Equals("fk_payment_customer", StringComparison.Ordinal));
            Assert.Contains(table.ForeignKeys, x => x.Name.Equals("fk_payment_rental", StringComparison.Ordinal));
            Assert.Contains(table.ForeignKeys, x => x.Name.Equals("fk_payment_staff", StringComparison.Ordinal));

            var pgsqldbp = this.dbFixture.GetPostgreDatabaseProvider();
            db = pgsqldbp.GetDatabase();
            Assert.Equal("pagila", db.Name);
            Assert.Equal(22, db.Tables.Count);
            table = db.Tables.Find(x => x.Name.Equals("film", StringComparison.Ordinal));
            Assert.Equal(14, table.Columns.Count);
            Assert.Contains(table.Columns, x => x.Name.Equals("language_id", StringComparison.Ordinal));
            table = db.Tables.Find(x => x.Name.Equals("film_category", StringComparison.Ordinal));
            Assert.True(table.PrimaryKeys.Count == 2);
            Assert.Contains(table.PrimaryKeys, x => ((PostgreSqlPrimaryKey)x).ColumnName.Equals("film_id", StringComparison.Ordinal) && x.Name.Equals("film_category_pkey", StringComparison.Ordinal));
            Assert.Contains(table.PrimaryKeys, x => ((PostgreSqlPrimaryKey)x).ColumnName.Equals("category_id", StringComparison.Ordinal) && x.Name.Equals("film_category_pkey", StringComparison.Ordinal));
            table = db.Tables.Find(x => x.Name.Equals("rental", StringComparison.Ordinal));
            Assert.True(table.ForeignKeys.Count == 3);
            Assert.Contains(table.ForeignKeys, x => x.Name.Equals("rental_customer_id_fkey", StringComparison.Ordinal));
            Assert.Contains(table.ForeignKeys, x => x.Name.Equals("rental_inventory_id_fkey", StringComparison.Ordinal));
            Assert.Contains(table.ForeignKeys, x => x.Name.Equals("rental_staff_id_fkey", StringComparison.Ordinal));
        }
    }
}
