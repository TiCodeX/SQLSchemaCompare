namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Specific MicrosoftSql primary key definition
    /// </summary>
    public class MicrosoftSqlIndex : ABaseDbConstraint
    {
        /// <summary>
        /// Gets or sets the column name
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets ordinal position
        /// </summary>
        public int? OrdinalPosition { get; set; }

        /// <summary>
        /// Gets or sets the index type
        /// </summary>
        public byte Type { get; set; }

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
        /// Gets or sets a value indicating whether the index is primary key
        /// </summary>
        public bool? IsPrimaryKey { get; set; }

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

        /// <summary>
        /// Gets or sets a value indicating whether duplicate key messages are suppressed
        /// </summary>
        public bool? SuppressDupKeyMessages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the index is auto created
        /// </summary>
        public bool? AutoCreated { get; set; }
    }
}
