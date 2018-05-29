namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Specific PostgreSqlColumn column definition
    /// </summary>
    public class PostgreSqlColumn : BaseDbColumn
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
        public int? DateTimePrecision { get; set; }

        /// <summary>
        /// Gets or sets the column interval type
        /// </summary>
        public string IntervalType { get; set; }

        /// <summary>
        /// Gets or sets the column interval precision
        /// </summary>
        public int? IntervalPrecision { get; set; }

        /// <summary>
        /// Gets or sets the column udt catalog
        /// </summary>
        public string UdtCatalog { get; set; }

        /// <summary>
        /// Gets or sets the column utd schema
        /// </summary>
        public string UdtSchema { get; set; }

        /// <summary>
        /// Gets or sets the column utd name
        /// </summary>
        public string UdtName { get; set; }

        /// <summary>
        /// Gets or sets the column scope catalog
        /// </summary>
        public string ScopeCatalog { get; set; }

        /// <summary>
        /// Gets or sets the column scope schema
        /// </summary>
        public string ScopeSchema { get; set; }

        /// <summary>
        /// Gets or sets the column scope name
        /// </summary>
        public string ScopeName { get; set; }

        /// <summary>
        /// Gets or sets the column maximum cardinality
        /// </summary>
        public int? MaximumCardinality { get; set; }

        /// <summary>
        /// Gets or sets the column dtd identifier
        /// </summary>
        public string DtdIdentifier { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column is self referencing
        /// </summary>
        public bool IsSelfReferencing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column is identity
        /// </summary>
        public bool IsIdentity { get; set; }

        /// <summary>
        /// Gets or sets the column identity generation
        /// </summary>
        public string IdentityGeneration { get; set; }

        /// <summary>
        /// Gets or sets the column identity start
        /// </summary>
        public string IdentitiyStart { get; set; }

        /// <summary>
        /// Gets or sets the column identity increment
        /// </summary>
        public string IdentityIncrement { get; set; }

        /// <summary>
        /// Gets or sets the column identity maximum
        /// </summary>
        public string IdentityMaximum { get; set; }

        /// <summary>
        /// Gets or sets the column identity minimum
        /// </summary>
        public string IdentityMinimum { get; set; }

        /// <summary>
        /// Gets or sets the column identity cycle
        /// </summary>
        public string IdentityCycle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column is generated
        /// </summary>
        public bool IsGenerated { get; set; }

        /// <summary>
        /// Gets or sets the column generation expression
        /// </summary>
        public string GenerationExpression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column is updatable
        /// </summary>
        public bool IsUpdatable { get; set; }

#pragma warning disable SA1600 // Elements must be documented
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int? NumericPrecision { get; set; }

        public int? NumericPrecisionRadix { get; set; }

        public string CharachterSetCatalog { get; set; }

        public string CharacterSetSchema { get; set; }

        public string CollationCatalog { get; set; }

        public string CollationSchema { get; set; }

        public string DomainCatalog { get; set; }

        public string DomainSchema { get; set; }

        public string DomainName { get; set; }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements must be documented

    }
}