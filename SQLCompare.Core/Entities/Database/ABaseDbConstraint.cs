namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database constraint classes
    /// </summary>
    public class ABaseDbConstraint
    {
        /// <summary>
        /// Gets or sets the constraint catalog
        /// </summary>
        public string ConstraintCatalog { get; set; }

        /// <summary>
        /// Gets or sets the constraint schema
        /// </summary>
        public string ConstraintSchema { get; set; }

        /// <summary>
        /// Gets or sets the primary key name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the table catalog
        /// </summary>
        public string TableCatalog { get; set; }

        /// <summary>
        /// Gets or sets the table schema
        /// </summary>
        public string TableSchema { get; set; }

        /// <summary>
        /// Gets or sets the table name
        /// </summary>
        public string TableName { get; set; }
    }
}