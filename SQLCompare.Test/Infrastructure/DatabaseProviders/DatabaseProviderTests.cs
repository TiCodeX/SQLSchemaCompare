using FluentAssertions;
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
        /// Test the retrieval of the MicrosoftSQL 'sakila' database
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void GetMicrosoftSqlDatabase()
        {
            var mssqldbp = this.dbFixture.GetMicrosoftSqlDatabaseProvider();
            var db = mssqldbp.GetDatabase();
            db.Should().NotBeNull();
        }

        /// <summary>
        /// Test the retrieval of the MySQL 'sakila' database
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void GetMySqlDatabase()
        {
            var mysqldbp = this.dbFixture.GetMySqlDatabaseProvider();
            var db = mysqldbp.GetDatabase();
            db.Should().NotBeNull();
        }

        /// <summary>
        /// Test the retrieval of PostgreSQL 'sakila' database
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void GetPostgreSqlDatabase()
        {
            var pgsqldbp = this.dbFixture.GetPostgreDatabaseProvider();
            var db = pgsqldbp.GetDatabase();
            db.Should().NotBeNull();
        }
    }
}
