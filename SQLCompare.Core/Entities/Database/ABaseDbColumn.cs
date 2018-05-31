namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic options of database column classes
    /// </summary>
    public abstract class ABaseDbColumn
    {
        /// <summary>
        /// Gets or sets the database catalog name
        /// </summary>
        public string CatalogName { get; set; }

        /// <summary>
        /// Gets or sets the database schema name
        /// </summary>
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the database table name
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the database column name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the column default value
        /// </summary>
        public string ColumnDefault { get; set; }

        /// <summary>
        /// Gets or sets the column data type
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets the column character set name
        /// </summary>
        public string CharacterSetName { get; set; }

        /// <summary>
        /// Gets or sets the column collation name
        /// </summary>
        public string CollationName { get; set; }
    }
}