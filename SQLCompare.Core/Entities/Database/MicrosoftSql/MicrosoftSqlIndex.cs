namespace SQLCompare.Core.Entities.Database.MicrosoftSql
{
    /// <summary>
    /// Specific MicrosoftSql index definition
    /// </summary>
    public class MicrosoftSqlIndex : ABaseDbIndex
    {
        /// <summary>
        /// List of possible types of index
        /// </summary>
        public enum IndexType
        {
            /// <summary>
            /// Heap type
            /// </summary>
            Heap = 0,

            /// <summary>
            /// Clustered type
            /// </summary>
            Clustered = 1,

            /// <summary>
            /// Nonclustered type
            /// </summary>
            Nonclustered = 2,

            /// <summary>
            /// XML type
            /// </summary>
            XML = 3,

            /// <summary>
            /// Spatial type
            /// </summary>
            Spatial = 4,

            /// <summary>
            /// Clustered columnstore type
            /// Applies to: SQL Server 2014 (12.x) through SQL Server 2017
            /// </summary>
            ClusteredColumnstore = 5,

            /// <summary>
            /// Nonclustered columnstore type
            /// Applies to: SQL Server 2012 (11.x) through SQL Server 2017
            /// </summary>
            NonclusteredColumnstore = 6,

            /// <summary>
            /// Nonclustered hash type
            /// Applies to: SQL Server 2014 (12.x) through SQL Server 2017
            /// </summary>
            NonclusteredHash = 7,
        }

        /// <summary>
        /// Gets or sets ordinal position
        /// </summary>
        public int OrdinalPosition { get; set; }

        /// <summary>
        /// Gets or sets the index type
        /// </summary>
        public IndexType Type { get; set; }

        /// <summary>
        /// Gets or sets the type description
        /// </summary>
        public string TypeDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the index is unique
        /// </summary>
        public bool? IsUnique { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the index ignore duplicate key
        /// </summary>
        public bool? IgnoreDupKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the index is unique constraint
        /// </summary>
        public bool? IsUniqueContraint { get; set; }

        /// <summary>
        /// Gets or sets the index fill factor
        /// </summary>
        public byte? IndexFillFactor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the index is padded
        /// </summary>
        public bool? IsPadded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the index is disabled
        /// </summary>
        public bool? IsDisabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the index is hypothetical
        /// </summary>
        public bool? IsHypothetical { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the index is ignored in optimization
        /// </summary>
        public bool? IsIgnoredInOptimization { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the index allows row locks
        /// </summary>
        public bool? AllowRowLocks { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the index allows page locks
        /// </summary>
        public bool? AllowPageLocks { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the index has filter
        /// </summary>
        public bool? HasFilter { get; set; }

        /// <summary>
        /// Gets or sets the filter definition
        /// </summary>
        public string FilterDefinition { get; set; }

        /// <summary>
        /// Gets or sets the index compression delay
        /// </summary>
        public int? CompressionDelay { get; set; }
    }
}
