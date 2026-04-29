namespace TiCodeX.SQLSchemaCompare.Test.Infrastructure.SqlScripter;

/// <summary>
/// Test class for the MicrosoftSqlScripter
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MicrosoftSqlScripterTests"/> class.
/// </remarks>
/// <param name="output">The test output helper</param>
public class MicrosoftSqlScripterTests(ITestOutputHelper output) : BaseTests<MicrosoftSqlScripterTests>(output)
{
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
    [ExcelData(@"Datasources/ScriptMicrosoftSqlDataTypeTest.xlsx")]
    public void ScriptDataType(ProjectOptions options, MicrosoftSqlDataType dataType, string expectedResult)
    {
        ArgumentNullException.ThrowIfNull(dataType);

        var scripter = new MicrosoftSqlScripter(this.Logger, options);
        dataType.Database = new MicrosoftSqlDb();
        scripter.GenerateCreateScript(dataType, false).Should().Be(expectedResult);
    }
}
