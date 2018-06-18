using SQLCompare.Core.Entities.Database.MicrosoftSql;
using SQLCompare.Core.Entities.Project;
using SQLCompare.Infrastructure.SqlScripters;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace SQLCompare.Test.Infrastructure.SqlScripter
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
        /// Test for the script column function
        /// </summary>
        /// <param name="options">The project options</param>
        /// <param name="column">The MicrosoftSqlColumn </param>
        /// <param name="expectedResult">The expected sql result</param>
        [Theory]
        [UnitTest]
        [ExcelData(@"Datasources\ScriptColumnTest.xlsx")]
        public void ScriptColumn(ProjectOptions options, MicrosoftSqlColumn column, string expectedResult)
        {
            MicrosoftSqlScriptHelper helper = new MicrosoftSqlScriptHelper(options);
            Assert.Equal(helper.ScriptColumn(column), expectedResult);
        }
    }
}
