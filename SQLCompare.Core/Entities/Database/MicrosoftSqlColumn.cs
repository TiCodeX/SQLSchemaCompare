namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Specific MicrosoftSql column definition
    /// </summary>
    public class MicrosoftSqlColumn : BaseDbColumn
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
        /// Gets or sets the column character octet lenght
        /// </summary>
        public int? CharacterOctetLenght { get; set; }

        /// <summary>
        /// Gets or sets the column date time precision
        /// </summary>
        public short? DateTimePrecision { get; set; }

#pragma warning disable SA1600 // Elements must be documented
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public byte? NumericPrecision { get; set; }

        public short? NumericPrecisionRadix { get; set; }

        public string CharachterSetCatalog { get; set; }

        public string CharacterSetSchema { get; set; }

        public string CollationCatalog { get; set; }

        public string CollationSchema { get; set; }

        public string DomainCatalog { get; set; }

        public string DomainSchema { get; set; }

        public string DomainName { get; set; }

        public bool IsIdentity { get; set; }

        public long IdentitySeed { get; set; }

        public long IdentityIncrement { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements must be documented

    }
}