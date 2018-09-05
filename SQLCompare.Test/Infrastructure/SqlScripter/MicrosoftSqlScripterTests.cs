using System.Collections.Generic;
using FluentAssertions;
using SQLCompare.Core.Entities.Database;
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
        /// <param name="column">The MicrosoftSqlColumn</param>
        /// <param name="expectedResult">The expected sql result</param>
        [Theory]
        [UnitTest]
        [ExcelData(@"Datasources\ScriptMicrosoftSqlColumnTest.xlsx")]
        public void ScriptColumn(ProjectOptions options, MicrosoftSqlColumn column, string expectedResult)
        {
            var helper = new MicrosoftSqlScriptHelper(options);
            helper.ScriptColumn(column).Should().Be(expectedResult);
        }

        /// <summary>
        /// Test for the script column function
        /// </summary>
        /// <param name="options">The project options</param>
        /// <param name="dataType">The MicrosoftSqlDataType</param>
        /// <param name="expectedResult">The expected sql result</param>
        [Theory]
        [UnitTest]
        [ExcelData(@"Datasources\ScriptMicrosoftSqlDataTypeTest.xlsx")]
        public void ScriptDataType(ProjectOptions options, MicrosoftSqlDataType dataType, string expectedResult)
        {
            var scripter = new MicrosoftSqlScripter(this.Logger, options);
            scripter.GenerateCreateTypeScript(dataType, new List<ABaseDbDataType>()).Should().Be(expectedResult);
        }
    }
}
