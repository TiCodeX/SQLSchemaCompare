namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.MySql;

/// <summary>
/// Specific MySql function definition
/// </summary>
public class MySqlFunction : ABaseDbFunction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlFunction"/> class
    /// </summary>
    public MySqlFunction()
    {
        this.AlterScriptSupported = false;
    }
}
