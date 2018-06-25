using System;

namespace SQLCompare.Core.Entities.Compare
{
    /// <summary>
    /// Represent the result of the comparison of a generic item
    /// </summary>
    public abstract class ABaseCompareResultItem
    {
        /// <summary>
        /// Gets the unique identifier of the result item
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets the source item name
        /// </summary>
        public abstract string SourceItemName { get; }

        /// <summary>
        /// Gets the target item name
        /// </summary>
        public abstract string TargetItemName { get; }

        /// <summary>
        /// Gets the item type
        /// </summary>
        public abstract Type ItemType { get; }

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
