using SQLCompare.Infrastructure.EntityFramework;
using Xunit;
using Xunit.Abstractions;

namespace SQLCompare.Test.Infrastructure.EntityFramework
{
    public class DatabaseContextTests
    {
        private readonly ITestOutputHelper _output;

        public DatabaseContextTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void DatabaConnection()
        {
            using (var c = new MicrosoftSqlDatabaseContext("localhost\\SQLEXPRESS", "BrokerProGlobal", "brokerpro", "brokerpro05"))
            {
                _output.WriteLine("MicrosoftSqlDatabaseContext:");
                foreach (var table in c.Tables)
                {
                    _output.WriteLine($"=> {table.TableName}");
                }
            }

            _output.WriteLine(string.Empty);

            using (var c = new PostgreSqlDatabaseContext("localhost", "world", "postgres", "test1234"))
            {
                _output.WriteLine("PostgreSqlDatabaseContext:");
                foreach (var table in c.Tables)
                {
                    _output.WriteLine($"=> {table.TableName}");
                }
            }

            _output.WriteLine(string.Empty);

            using (var c = new MySqlDatabaseContext("localhost", "employees", "admin", "test1234"))
            {
                _output.WriteLine("MySqlDatabaseContext:");
                foreach (var table in c.Tables)
                {
                    _output.WriteLine($"=> {table.TableName}");
                }
            }
        }


    }
}
