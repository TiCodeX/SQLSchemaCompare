namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Specific PostgreSql index definition
    /// </summary>
    public class PostgreSqlIndex : ABaseDbConstraint
    {
        /// <summary>
        /// Gets or sets the column name
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets ordinal position
        /// </summary>
        public int OrdinalPosition { get; set; }

        /// <summary>
        /// Gets or sets the position in unique constraint
        /// </summary>
        public int? PositionInUniqueConstraint { get; set; }
    }
}