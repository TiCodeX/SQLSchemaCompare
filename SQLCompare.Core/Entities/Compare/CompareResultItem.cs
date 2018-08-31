using SQLCompare.Core.Entities.Database;

namespace SQLCompare.Core.Entities.Compare
{
    /// <summary>
    /// Represent the result of the comparison of a specific item
    /// </summary>
    /// <typeparam name="T">Type of the item compared</typeparam>
    public class CompareResultItem<T> : ABaseCompareResultItem
        where T : ABaseDbObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompareResultItem{T}"/> class
        /// </summary>
        public CompareResultItem()
        {
            this.ItemType = typeof(T);
        }

        /// <summary>
        /// Gets or sets the Source item
        /// </summary>
        public T SourceItem { get; set; }

        /// <inheritdoc />
        public override string SourceItemName => this.SourceItem == null ? string.Empty : $"{this.SourceItem.Schema}.{this.SourceItem.Name}";

        /// <summary>
        /// Gets or sets the Target item
        /// </summary>
        public T TargetItem { get; set; }

        /// <inheritdoc />
        public override string TargetItemName => this.TargetItem == null ? string.Empty : $"{this.TargetItem.Schema}.{this.TargetItem.Name}";
    }
}
