namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database constraint classes
    /// </summary>
    public class ABaseDbConstraint : ABaseDbObject
    {
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

        /// <summary>
        /// Gets or sets the column name
        /// </summary>
        public string ColumnName { get; set; }
    }
}