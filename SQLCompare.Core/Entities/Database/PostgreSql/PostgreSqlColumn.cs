namespace SQLCompare.Core.Entities.Database.PostgreSql
{
    /// <summary>
    /// Specific PostgreSqlColumn column definition
    /// </summary>
    public class PostgreSqlColumn : ABaseDbColumn
    {
        /// <summary>
        /// Gets or sets the column ordinal position
        /// </summary>
        public int? OrdinalPosition { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column is nullable
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// Gets or sets the column numeric scale
        /// </summary>
        public int? NumericScale { get; set; }

        /// <summary>
        /// Gets or sets the column character maximum lenght
        /// </summary>
        public int? CharacterMaxLenght { get; set; }

        /// <summary>
        /// Gets or sets the column date time precision
        /// </summary>
        public int? DateTimePrecision { get; set; }

        /// <summary>
        /// Gets or sets the column interval type
        /// </summary>
        public string IntervalType { get; set; }

        /// <summary>
        /// Gets or sets the column utd name
        /// </summary>
        public string UdtName { get; set; }

        /// <summary>
        /// Gets or sets the numeric precision
        /// </summary>
        public int? NumericPrecision { get; set; }
    }
}