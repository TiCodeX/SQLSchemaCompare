using SQLCompare.Core.Entities.Database.MySql;
using SQLCompare.Core.Entities.Database.PostgreSql;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Infrastructure.DatabaseProviders;
using SQLCompare.Infrastructure.SqlScripters;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace SQLCompare.Test.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Test class for the DatabaseProvider
    /// </summary>
    public class DatabaseProviderTests : BaseTests<DatabaseProviderTests>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseProviderTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper</param>
        public DatabaseProviderTests(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Test the retrieval of database list with all the databases
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void GetDatabaseList()
        {
            DatabaseProviderFactory dpf = new DatabaseProviderFactory(this.LoggerFactory);

            MicrosoftSqlDatabaseProvider mssqldbp = (MicrosoftSqlDatabaseProvider)dpf.Create(new MicrosoftSqlDatabaseProviderOptions { Hostname = "localhost\\SQLEXPRESS", UseWindowsAuthentication = true });
            var dbList = mssqldbp.GetDatabaseList();
            Assert.Contains("brokerpro", dbList);
            Assert.Contains("brokerpro_web", dbList);
            Assert.Contains("BrokerProGlobal", dbList);
            Assert.Contains("msdb", dbList);
            Assert.Contains("master", dbList);

            MySqlDatabaseProvider mysqldbp = (MySqlDatabaseProvider)dpf.Create(new MySqlDatabaseProviderOptions { Hostname = "localhost", Username = "admin", Password = "test1234", UseSSL = true });

            dbList = mysqldbp.GetDatabaseList();
            Assert.Contains("sakila", dbList);
            Assert.Contains("employees", dbList);
            Assert.Contains("sys", dbList);
            Assert.Contains("mysql", dbList);

            PostgreSqlDatabaseProvider pgsqldbp = (PostgreSqlDatabaseProvider)dpf.Create(new PostgreSqlDatabaseProviderOptions { Hostname = "localhost", Username = "postgres", Password = "test1234" });

            dbList = pgsqldbp.GetDatabaseList();
            Assert.Contains("pagila", dbList);
            Assert.Contains("postgres", dbList);
        }

        /// <summary>
        /// Test the retrieval of specific database with all the databases
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void GetDatabase()
        {
            DatabaseProviderFactory dpf = new DatabaseProviderFactory(this.LoggerFactory);

            MicrosoftSqlDatabaseProvider mssqldbp = (MicrosoftSqlDatabaseProvider)dpf.Create(new MicrosoftSqlDatabaseProviderOptions { Hostname = "localhost\\SQLEXPRESS", Database = "brokerpro", UseWindowsAuthentication = true });
            var db = mssqldbp.GetDatabase();
            Assert.Equal("brokerpro", db.Name);
            Assert.True(db.Tables.Count == 217);
            var table = db.Tables.Find(x => x.Name.Equals("DeletedDocumentReference", StringComparison.Ordinal));
            Assert.True(table.Columns.Count == 4);
            Assert.Contains(table.Columns, x => x.Name.Equals("OriginalTable", StringComparison.Ordinal));
            Assert.True(table.PrimaryKeys.Count == 3);
            Assert.Contains(table.PrimaryKeys, x => x.Name.Equals("PK_DeletedDocumentReference", StringComparison.Ordinal));
            Assert.True(table.ForeignKeys.Count == 2);
            Assert.Contains(table.ForeignKeys, x => x.Name.Equals("FK_DeletedDocumentReference_DeletedDocument", StringComparison.Ordinal));
            table = db.Tables.Find(x => x.Name.Equals("debitors", StringComparison.Ordinal));
            DatabaseScripterFactory dsf = new DatabaseScripterFactory(this.LoggerFactory);
            var script = dsf.Create(db, new SQLCompare.Core.Entities.Project.ProjectOptions());
            var t = script.GenerateCreateTableScript(table);
            Assert.True(t != null);

            MySqlDatabaseProvider mysqldbp = (MySqlDatabaseProvider)dpf.Create(new MySqlDatabaseProviderOptions { Hostname = "localhost", Database = "sakila", Username = "admin", Password = "test1234", UseSSL = true });

            db = mysqldbp.GetDatabase();
            Assert.Equal("sakila", db.Name);
            Assert.True(db.Tables.Count == 16);
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

            PostgreSqlDatabaseProvider pgsqldbp = (PostgreSqlDatabaseProvider)dpf.Create(new PostgreSqlDatabaseProviderOptions { Hostname = "localhost", Database = "pagila", Username = "postgres", Password = "test1234" });

            db = pgsqldbp.GetDatabase();
            Assert.Equal("pagila", db.Name);
            Assert.True(db.Tables.Count == 21);
            table = db.Tables.Find(x => x.Name.Equals("film", StringComparison.Ordinal));
            Assert.True(table.Columns.Count == 14);
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
