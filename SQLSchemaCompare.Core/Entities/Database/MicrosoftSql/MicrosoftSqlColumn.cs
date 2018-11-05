namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.MicrosoftSql
{
    /// <summary>
    /// Specific MicrosoftSql column definition
    /// </summary>
    public class MicrosoftSqlColumn : ABaseDbColumn
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
        /// Gets or sets the column character maximum length
        /// </summary>
        public int? CharacterMaxLength { get; set; }

        /// <summary>
        /// Gets or sets the column date time precision
        /// </summary>
        public short? DateTimePrecision { get; set; }

        /// <summary>
        /// Gets or sets the  numeric precision
        /// </summary>
        public byte? NumericPrecision { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column is identity
        /// </summary>
        public bool IsIdentity { get; set; }

        /// <summary>
        /// Gets or sets the identity seed
        /// </summary>
        public long IdentitySeed { get; set; }

        /// <summary>
        /// Gets or sets the identity increment
        /// </summary>
        public long IdentityIncrement { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column is computed
        /// </summary>
        public bool IsComputed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column is a ROWGUIDCOL
        /// </summary>
        public bool IsRowGuidCol { get; set; }

        /// <summary>
        /// Gets or sets the definition of a computed column
        /// </summary>
        public string Definition { get; set; }

        /// <summary>
        /// Gets or sets the schema of the user defined data type
        /// </summary>
        public string UserDefinedDataTypeSchema { get; set; }

        /// <summary>
        /// Gets or sets the user defined data type
        /// </summary>
        public string UserDefinedDataType { get; set; }
    }
}