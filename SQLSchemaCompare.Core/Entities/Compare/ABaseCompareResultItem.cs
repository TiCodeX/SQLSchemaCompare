using System;

namespace TiCodeX.SQLSchemaCompare.Core.Entities.Compare
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
        /// Gets or sets the source item name
        /// </summary>
        public string SourceItemName { get; set; }

        /// <summary>
        /// Gets or sets the target item name
        /// </summary>
        public string TargetItemName { get; set; }

        /// <summary>
        /// Gets or sets the item type
        /// </summary>
        public Type ItemType { get; set; }

        /// <summary>
        /// Gets or sets the scripts
        /// </summary>
        public CompareResultItemScripts Scripts { get; set; } = new CompareResultItemScripts();

        /// <summary>
        /// Gets a value indicating whether the items are equal
        /// </summary>
        public bool Equal => this.Scripts.SourceCreateScript == this.Scripts.TargetCreateScript;
    }
}
