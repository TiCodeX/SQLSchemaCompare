namespace SQLCompare.Core.Entities.Database.MySql
{
    /// <summary>
    /// Specific MySql index definition
    /// </summary>
    public class MySqlIndex : ABaseDbIndex
    {
        /// <summary>
        /// Gets or sets ordinal position
        /// </summary>
        public uint OrdinalPosition { get; set; }

        /// <summary>
        /// Gets or sets the index type
        /// </summary>
        public string IndexType { get; set; }

        /// <summary>
        /// Gets or sets the constraint type
        /// </summary>
        public string ConstraintType { get; set; }
    }
}