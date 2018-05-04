using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SQLCompare.Infrastructure.EntityFramework;
using SQLCompare.Infrastructure.SqlScripters;
using Xunit;
using Xunit.Abstractions;

namespace SQLCompare.Test.Infrastructure.SqlScripters
{
    /// <summary>
    /// Test class for the MicrosoftSqlScripter
    /// </summary>
    public class MicrosoftSqlScripterTests : BaseTests<MicrosoftSqlScripterTests>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftSqlScripterTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper</param>
        public MicrosoftSqlScripterTests(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Test the creation of the script
        /// </summary>
        [Fact]
        public void ScriptCreateTables()
        {
            using (var c = new MicrosoftSqlDatabaseContext("localhost\\SQLEXPRESS", "BrokerPro", "brokerpro", "brokerpro05"))
            {
                var x = new MicrosoftSqlScripter(this.Logger, null);

                this.Logger.LogInformation("MicrosoftSqlDatabaseContext:");
                foreach (var table in c.Tables.Include(t => t.Columns))
                {
                    this.Logger.LogInformation($"=> {table.TableName}");
                    this.Logger.LogInformation(x.ScriptCreateTable(table));
                }
            }
        }
    }
}
