namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Specific MicrosoftSql column definition
    /// </summary>
    public class MicrosoftSqlColumn : BaseDbColumn
    {
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