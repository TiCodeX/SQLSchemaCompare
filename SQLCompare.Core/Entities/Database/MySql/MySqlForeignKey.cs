namespace SQLCompare.Core.Entities.Database.MySql
{
    /// <summary>
    /// Specific MySql foreign key definition
    /// </summary>
    public class MySqlForeignKey : MySqlIndex
    {
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

        /// <summary>
        /// Gets or sets the update rule
        /// </summary>
        public string UpdateRule { get; set; }

        /// <summary>
        /// Gets or sets the delete rule
        /// </summary>
        public string DeleteRule { get; set; }
    }
}
