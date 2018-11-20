namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.MySql
{
    /// <summary>
    /// Specific MySql stored procedure definition
    /// </summary>
    public class MySqlStoredProcedure : ABaseDbStoredProcedure
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlStoredProcedure"/> class
        /// </summary>
        public MySqlStoredProcedure()
        {
            this.AlterScriptSupported = false;
        }
    }
}
