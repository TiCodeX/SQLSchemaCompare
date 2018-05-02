using Microsoft.Extensions.Logging;
using SQLCompare.Infrastructure.EntityFramework;
using Xunit;
using Xunit.Abstractions;

namespace SQLCompare.Test.Infrastructure.EntityFramework
{
    public class DatabaseContextTests : BaseTests<DatabaseContextTests>
    {
        public DatabaseContextTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void DatabaseConnection()
        {
            using (var c = new MicrosoftSqlDatabaseContext("localhost\\SQLEXPRESS", "BrokerProGlobal", "brokerpro", "brokerpro05"))
            {
                Logger.LogInformation("MicrosoftSqlDatabaseContext:");
                foreach (var table in c.Tables)
                {
                    Logger.LogInformation($"=> {table.TableName}");
                }
            }

            Logger.LogInformation(string.Empty);

            using (var c = new PostgreSqlDatabaseContext("localhost", "world", "postgres", "test1234"))
            {
                Logger.LogInformation("PostgreSqlDatabaseContext:");
                foreach (var table in c.Tables)
                {
                    Logger.LogInformation($"=> {table.TableName}");
                }
            }

            Logger.LogInformation(string.Empty);

            using (var c = new MySqlDatabaseContext("localhost", "employees", "admin", "test1234"))
            {
                Logger.LogInformation("MySqlDatabaseContext:");
                foreach (var table in c.Tables)
                {
                    Logger.LogInformation($"=> {table.TableName}");
                }
            }
        }
    }
}
