namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.MySql
{
    /// <summary>
    /// Specific MySql trigger definition
    /// </summary>
    public class MySqlTrigger : ABaseDbTrigger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlTrigger"/> class
        /// </summary>
        public MySqlTrigger()
        {
            this.AlterScriptSupported = false;
        }
    }
}
