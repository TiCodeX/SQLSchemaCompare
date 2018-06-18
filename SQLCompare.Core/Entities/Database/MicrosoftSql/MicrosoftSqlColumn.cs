namespace SQLCompare.Core.Entities.Database.MicrosoftSql
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

        /// <summary>
        /// Gets or sets the  numeric precision
        /// </summary>
        public byte? NumericPrecision { get; set; }

        /// <summary>
        /// Gets or sets the  numeric precision radix
        /// </summary>
        public short? NumericPrecisionRadix { get; set; }

        /// <summary>
        /// Gets or sets the  character set catalog
        /// </summary>
        public string CharachterSetCatalog { get; set; }

        /// <summary>
        /// Gets or sets the character set schema
        /// </summary>
        public string CharacterSetSchema { get; set; }

        /// <summary>
        /// Gets or sets the collation catalog
        /// </summary>
        public string CollationCatalog { get; set; }

        /// <summary>
        /// Gets or sets the collation schema
        /// </summary>
        public string CollationSchema { get; set; }

        /// <summary>
        /// Gets or sets the domain catalog
        /// </summary>
        public string DomainCatalog { get; set; }

        /// <summary>
        /// Gets or sets the domain schema
        /// </summary>
        public string DomainSchema { get; set; }

        /// <summary>
        /// Gets or sets the domain name
        /// </summary>
        public string DomainName { get; set; }

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
        /// Gets or sets the definition of a computed column
        /// </summary>
        public string Definition { get; set; }
    }
}