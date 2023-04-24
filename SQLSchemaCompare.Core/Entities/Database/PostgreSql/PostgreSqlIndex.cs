namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.PostgreSql
{
    /// <summary>
    /// Specific PostgreSql index definition
    /// </summary>
    public class PostgreSqlIndex : ABaseDbIndex
    {
        /// <summary>
        /// Gets or sets a value indicating whether the index is unique
        /// </summary>
        public bool IsUnique { get; set; }

        /// <summary>
        /// Gets or sets the index type
        /// </summary>
        public string Type { get; set; }
    }
}
