namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic options of database column classes
    /// </summary>
    public abstract class BaseDbColumn
    {
        /// <summary>
        /// Gets or sets the database column name
        /// </summary>
        public string Name { get; set; }
    }
}