namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.PostgreSql
{
    /// <summary>
    /// Specific PostgreSql trigger definition
    /// </summary>
    public class PostgreSqlTrigger : ABaseDbTrigger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlTrigger"/> class
        /// </summary>
        public PostgreSqlTrigger()
        {
            this.AlterScriptSupported = false;
        }
    }
}
