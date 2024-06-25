namespace TiCodeX.SQLSchemaCompare.Core.Entities.Compare
{
    /// <summary>
    /// Represent the result of a comparison
    /// </summary>
    public class CompareResult
    {
        /// <summary>
        /// Gets or sets the source full script
        /// </summary>
        public string SourceFullScript { get; set; }

        /// <summary>
        /// Gets or sets the target full script
        /// </summary>
        public string TargetFullScript { get; set; }

        /// <summary>
        /// Gets or sets the full alter script
        /// </summary>
        public string FullAlterScript { get; set; }

        /// <summary>
        /// Gets the list of different items
        /// </summary>
        public List<ABaseCompareResultItem> DifferentItems { get; } = new List<ABaseCompareResultItem>();

        /// <summary>
        /// Gets the list of items only on the source
        /// </summary>
        public List<ABaseCompareResultItem> OnlySourceItems { get; } = new List<ABaseCompareResultItem>();

        /// <summary>
        /// Gets the list of items only on the target
        /// </summary>
        public List<ABaseCompareResultItem> OnlyTargetItems { get; } = new List<ABaseCompareResultItem>();

        /// <summary>
        /// Gets the list of items which are the same in both
        /// </summary>
        public List<ABaseCompareResultItem> SameItems { get; } = new List<ABaseCompareResultItem>();
    }
}
