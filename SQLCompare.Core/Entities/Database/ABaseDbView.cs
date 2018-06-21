namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database table classes
    /// </summary>
    public abstract class ABaseDbView
    {
        /// <summary>
        /// Gets or sets database view name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the database table schema
        /// </summary>
        public string TableSchema { get; set; }

        /// <summary>
        /// Gets or sets the database table catalog
        /// </summary>
        public string TableCatalog { get; set; }

        /// <summary>
        /// Gets or sets the database view definition script
        /// </summary>
        public string ViewDefinition { get; set; }
    }
}