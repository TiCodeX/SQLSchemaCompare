namespace SQLCompare.Core.Entities.Compare
{
    /// <summary>
    /// Represent the SQL scripts of a specific item
    /// </summary>
    public class CompareResultItemScripts
    {
        /// <summary>
        /// Gets or sets the creation script of the source item
        /// </summary>
        public string SourceCreateScript { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the creation script of the target item
        /// </summary>
        public string TargetCreateScript { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the alter script
        /// </summary>
        public string AlterScript { get; set; } = string.Empty;
    }
}
