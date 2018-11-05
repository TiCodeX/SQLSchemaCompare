namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.PostgreSql
{
    /// <summary>
    /// Specific PostgreSql index definition
    /// </summary>
    public class PostgreSqlSequence : ABaseDbSequence
    {
        /// <summary>
        /// Gets or sets the cache
        /// </summary>
        public long Cache { get; set; }
    }
}
