namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Specific MySql index definition
    /// </summary>
    public class MySqlIndex : ABaseDbConstraint
    {
        /// <summary>
        /// Gets or sets the column name
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets ordinal position
        /// </summary>
        public uint OrdinalPosition { get; set; }

        /// <summary>
        /// Gets or sets the position in unique constraint
        /// </summary>
        public uint? PositionInUniqueConstraint { get; set; }

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