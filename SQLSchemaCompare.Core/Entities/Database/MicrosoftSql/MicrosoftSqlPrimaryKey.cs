namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.MicrosoftSql
{
    /// <summary>
    /// Specific MicrosoftSql primary key definition
    /// </summary>
    /// <seealso cref="TiCodeX.SQLSchemaCompare.Core.Entities.Database.MicrosoftSql.MicrosoftSqlIndex" />
    public class MicrosoftSqlPrimaryKey : ABaseDbPrimaryKey
    {
        /// <summary>
        /// Gets or sets the type description
        /// </summary>
        public string TypeDescription { get; set; }
    }
}
