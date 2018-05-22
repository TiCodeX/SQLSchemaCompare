namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Specific MySql column definition
    /// </summary>
    public class MySqlColumn : BaseDbColumn
    {
        /// <summary>
        /// Gets or sets the column ordinal position
        /// </summary>
        public uint OrdinalPosition { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column is nullable
        /// </summary>
        public string IsNullable { get; set; }

        /// <summary>
        /// Gets or sets the column numeric scale
        /// </summary>
        public uint NumericScale { get; set; }

        /// <summary>
        /// Gets or sets the column character maximum lenght
        /// </summary>
        public long CharacterMaxLenght { get; set; }

        /// <summary>
        /// Gets or sets the column character octet lenght
        /// </summary>
        public long CharacterOctetLenght { get; set; }

        /// <summary>
        /// Gets or sets the column date time precision
        /// </summary>
        public ulong DateTimePrecision { get; set; }

        /// <summary>
        /// Gets or sets the column SRS id
        /// </summary>
        public uint? SrsId { get; set; }

        /// <summary>
        /// Gets or sets the column provileges
        /// </summary>
        public string Privileges { get; set; }

        /// <summary>
        /// Gets or sets the column numeric precision
        /// </summary>
        public uint? NumericPrecision { get; set; }

        /// <summary>
        /// Gets or sets the column generation expression
        /// </summary>
        public string GenerationExpression { get; set; }

        /// <summary>
        /// Gets or sets the column extra information
        /// </summary>
        public string Extra { get; set; }

        /// <summary>
        /// Gets or sets the column type
        /// </summary>
        public string ColumnType { get; set; }

        /// <summary>
        /// Gets or sets the column key
        /// </summary>
        public string ColumnKey { get; set; }

        /// <summary>
        /// Gets or sets the column comment
        /// </summary>
        public string ColumnComment { get; set; }
    }
}