namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic options of database column classes
    /// </summary>
    public abstract class BaseDbColumn
    {
        /// <summary>
        /// Gets or sets the database column name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the column ordinal position
        /// </summary>
        public int? OrdinalPosition { get; set; }

        /// <summary>
        /// Gets or sets the column default value
        /// </summary>
        public string ColumnDefault { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column is nullable
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// Gets or sets the column data type
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets the column character maximum lenght
        /// </summary>
        public int? CharacterMaxLenght { get; set; }

        /// <summary>
        /// Gets or sets the column character octet lenght
        /// </summary>
        public int? CharacterOctetLenght { get; set; }

        /// <summary>
        /// Gets or sets the column numeric scale
        /// </summary>
        public int? NumericScale { get; set; }

        /// <summary>
        /// Gets or sets the colulmn date time precision
        /// </summary>
        public short? DateTimePrecision { get; set; }

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