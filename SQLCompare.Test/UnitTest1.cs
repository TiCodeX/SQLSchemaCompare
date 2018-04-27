using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SQLCompare.Infrastructure.EntityFramework;
using SQLCompare.Infrastructure.SqlScripters;
using Xunit;
using Xunit.Abstractions;

namespace SQLCompare.Test
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _output;

        public UnitTest1(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Test1()
        {
            using (var c = new MicrosoftSqlDatabaseContext("localhost\\SQLEXPRESS", "BrokerPro", "brokerpro", "brokerpro05"))
            {
                var mock = new Mock<ILogger>();

                var x = new MicrosoftSqlScripter(mock.Object, null);
                
                _output.WriteLine("MicrosoftSqlDatabaseContext:");
                foreach (var table in c.Tables.Include(t => t.Columns))
                {
                    _output.WriteLine($"=> {table.TableName}");
                    _output.WriteLine(x.ScriptCreateTable(table));
                }



            }
        }
    }
}
