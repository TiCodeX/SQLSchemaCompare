using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Infrastructure.DatabaseProviders;
using SQLCompare.Infrastructure.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace SQLCompare.Test.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Test class for the DatabaseProviderTests
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
        public void GetDatabaseList()
        {
            DatabaseProviderFactory dpf = new DatabaseProviderFactory(this.LoggerFactory);

            MicrosoftSqlDatabaseProvider mssqldbp = (MicrosoftSqlDatabaseProvider)dpf.Create(new MicrosoftSqlDatabaseProviderOptions { Hostname = "localhost\\SQLEXPRESS", UseWindowsAuthentication = true });
            var dbList = mssqldbp.GetDatabaseList();
            Assert.Contains<string>("brokerpro", dbList);
            Assert.Contains<string>("brokerpro_web", dbList);
            Assert.Contains<string>("BrokerProGlobal", dbList);
            Assert.Contains<string>("msdb", dbList);
            Assert.Contains<string>("master", dbList);

            MySqlDatabaseProvider mysqldbp = (MySqlDatabaseProvider)dpf.Create(new MySqlDatabaseProviderOptions { Hostname = "localhost", Username = "admin", Password = "test1234", UseSSL = true });

            dbList = mysqldbp.GetDatabaseList();
            Assert.Contains<string>("sakila", dbList);
            Assert.Contains<string>("employees", dbList);
            Assert.Contains<string>("sys", dbList);
            Assert.Contains<string>("mysql", dbList);

            PostgreSqlDatabaseProvider pgsqldbp = (PostgreSqlDatabaseProvider)dpf.Create(new PostgreSqlDatabaseProviderOptions { Hostname = "localhost", Username = "postgres", Password = "test1234" });

            dbList = pgsqldbp.GetDatabaseList();
            Assert.Contains<string>("pagila", dbList);
            Assert.Contains<string>("postgres", dbList);
        }

        /// <summary>
        /// Test the retrieval of specific database with all the databases
        /// </summary>
        [Fact]
        public void GetDatabase()
        {
            DatabaseProviderFactory dpf = new DatabaseProviderFactory(this.LoggerFactory);

            MicrosoftSqlDatabaseProvider mssqldbp = (MicrosoftSqlDatabaseProvider)dpf.Create(new MicrosoftSqlDatabaseProviderOptions { Hostname = "localhost\\SQLEXPRESS", Database = "BrokerProGlobal", UseWindowsAuthentication = true });
            var db = mssqldbp.GetDatabase();
            Assert.Equal("BrokerProGlobal", db.Name);
            Assert.True(db.Tables.Count == 4);
            var table = db.Tables.Find(x => x.Name.Equals("CloudBackupHistory", StringComparison.Ordinal));
            Assert.True(table.Columns.Count == 17);
            Assert.Contains(table.Columns, x => x.Name.Equals("ProcessStatus", StringComparison.Ordinal));

            MySqlDatabaseProvider mysqldbp = (MySqlDatabaseProvider)dpf.Create(new MySqlDatabaseProviderOptions { Hostname = "localhost", Database = "sakila", Username = "admin", Password = "test1234", UseSSL = true });

            db = mysqldbp.GetDatabase();
            Assert.Equal("sakila", db.Name);
            Assert.True(db.Tables.Count == 16);
            table = db.Tables.Find(x => x.Name.Equals("film", StringComparison.Ordinal));
            Assert.True(table.Columns.Count == 13);
            Assert.Contains(table.Columns, x => x.Name.Equals("language_id", StringComparison.Ordinal));

            PostgreSqlDatabaseProvider pgsqldbp = (PostgreSqlDatabaseProvider)dpf.Create(new PostgreSqlDatabaseProviderOptions { Hostname = "localhost", Database = "pagila", Username = "postgres", Password = "test1234" });

            db = pgsqldbp.GetDatabase();
            Assert.Equal("pagila", db.Name);
            Assert.True(db.Tables.Count == 21);
            table = db.Tables.Find(x => x.Name.Equals("film", StringComparison.Ordinal));
            Assert.True(table.Columns.Count == 14);
            Assert.Contains(table.Columns, x => x.Name.Equals("language_id", StringComparison.Ordinal));
        }
    }
}
