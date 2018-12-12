using System.Collections.Generic;
using FluentAssertions;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database.MicrosoftSql;
using TiCodeX.SQLSchemaCompare.Core.Entities.Project;
using TiCodeX.SQLSchemaCompare.Infrastructure.SqlScripters;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace TiCodeX.SQLSchemaCompare.Test.Infrastructure.SqlScripter
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
        [ExcelData(@"Datasources/ScriptMicrosoftSqlColumnTest.xlsx")]
        public void ScriptColumn(ProjectOptions options, MicrosoftSqlColumn column, string expectedResult)
        {
            var helper = new MicrosoftSqlScriptHelper(options);
            helper.ScriptColumn(column, true).Should().Be(expectedResult);
        }

        /// <summary>
        /// Test for the script column function
        /// </summary>
        /// <param name="options">The project options</param>
        /// <param name="dataType">The MicrosoftSqlDataType</param>
        /// <param name="expectedResult">The expected sql result</param>
        [Theory]
        [UnitTest]
        [ExcelData(@"Datasources/ScriptMicrosoftSqlDataTypeTest.xlsx")]
        public void ScriptDataType(ProjectOptions options, MicrosoftSqlDataType dataType, string expectedResult)
        {
            var scripter = new MicrosoftSqlScripter(this.Logger, options);
            dataType.Database = new MicrosoftSqlDb();
            scripter.GenerateCreateScript(dataType).Should().Be(expectedResult);
        }
    }
}
