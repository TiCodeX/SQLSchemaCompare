namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database foreign keys
    /// </summary>
    public class ABaseDbForeignKey : ABaseDbConstraint
    {
        /// <summary>
        /// Gets or sets referenced table schema
        /// </summary>
        public string ReferencedTableSchema { get; set; }

        /// <summary>
        /// Gets or sets referenced table name
        /// </summary>
        public string ReferencedTableName { get; set; }

        /// <summary>
        /// Gets or sets referenced column name
        /// </summary>
        public string ReferencedColumnName { get; set; }
    }
}
