namespace SQLCompare.Core.Entities.Database.PostgreSql
{
    /// <summary>
    /// Specific PostgreSql foreign key definition
    /// </summary>
    public class PostgreSqlForeignKey : PostgreSqlIndex
    {
        /// <summary>
        /// Gets or sets the match option
        /// </summary>
        public string MatchOption { get; set; }

        /// <summary>
        /// Gets or sets the update rule
        /// </summary>
        public string UpdateRule { get; set; }

        /// <summary>
        /// Gets or sets the delete rule
        /// </summary>
        public string DeleteRule { get; set; }

        /// <summary>
        /// Gets or sets the referenced table catalog
        /// </summary>
        public string ReferencedTableCatalog { get; set; }

        /// <summary>
        /// Gets or sets the referenced table schema
        /// </summary>
        public string ReferencedTableSchema { get; set; }

        /// <summary>
        /// Gets or sets the referenced table name
        /// </summary>
        public string ReferencedTableName { get; set; }

        /// <summary>
        /// Gets or sets the referenced column name
        /// </summary>
        public string ReferencedColumnName { get; set; }
    }
}
