using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SQLCompare.Infrastructure.EntityFramework;
using SQLCompare.Infrastructure.SqlScripters;
using Xunit;
using Xunit.Abstractions;

namespace SQLCompare.Test.Infrastructure.SqlScripters
{
    public class MicrosoftSqlScripterTests : BaseTests<MicrosoftSqlScripterTests>
    {
        public MicrosoftSqlScripterTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void ScriptCreateTables()
        {
            using (var c = new MicrosoftSqlDatabaseContext("localhost\\SQLEXPRESS", "BrokerPro", "brokerpro", "brokerpro05"))
            {
                var x = new MicrosoftSqlScripter(Logger, null);

                Logger.LogInformation("MicrosoftSqlDatabaseContext:");
                foreach (var table in c.Tables.Include(t => t.Columns))
                {
                    Logger.LogInformation($"=> {table.TableName}");
                    Logger.LogInformation(x.ScriptCreateTable(table));
                }
            }
        }
    }
}
