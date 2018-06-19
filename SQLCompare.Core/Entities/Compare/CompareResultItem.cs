using System;

namespace SQLCompare.Core.Entities.Compare
{
    /// <summary>
    /// Represent the result of the comparison of a specific item
    /// </summary>
    /// <typeparam name="T">Type of the item compared</typeparam>
    public class CompareResultItem<T>
    {
        /// <summary>
        /// Gets the unique identifier of the result item
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the Source item
        /// </summary>
        public T SourceItem { get; set; }

        /// <summary>
        /// Gets or sets the Target item
        /// </summary>
        public T TargetItem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the items are equal
        /// </summary>
        public bool Equal { get; set; }

        /// <summary>
        /// Gets or sets the creation script of the source item
        /// </summary>
        public string SourceCreateScript { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the creation script of the target item
        /// </summary>
        public string TargetCreateScript { get; set; } = string.Empty;
    }
}
