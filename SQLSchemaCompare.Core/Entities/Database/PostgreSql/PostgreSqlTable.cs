namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.PostgreSql
{
    /// <summary>
    /// Specific PostgreSql table definition
    /// </summary>
    public class PostgreSqlTable : ABaseDbTable
    {
        /// <summary>
        /// Gets or sets the inherited table schema
        /// </summary>
        public string InheritedTableSchema { get; set; }

        /// <summary>
        /// Gets or sets the inherited table name
        /// </summary>
        public string InheritedTableName { get; set; }
    }
}