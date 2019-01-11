namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.MicrosoftSql
{
    /// <summary>
    /// Specific MicrosoftSql user definition
    /// </summary>
    public class MicrosoftSqlUser : ABaseDbUser
    {
        /// <summary>
        /// Gets or sets the Principal Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the default schema name
        /// </summary>
        public string DefaultSchemaName { get; set; }
    }
}
