namespace SQLCompare.Core.Entities.Database.PostgreSql
{
    /// <summary>
    /// Specific PostgreSql index definition
    /// </summary>
    public class PostgreSqlIndex : ABaseDbIndex
    {
        /// <summary>
        /// Gets or sets ordinal position
        /// </summary>
        public int OrdinalPosition { get; set; }
    }
}