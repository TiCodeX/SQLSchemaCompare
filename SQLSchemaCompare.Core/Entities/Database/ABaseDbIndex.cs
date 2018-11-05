namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database index classes
    /// </summary>
    public class ABaseDbIndex : ABaseDbConstraint
    {
        /// <summary>
        /// Gets or sets a value indicating whether the index is primary key
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is descending
        /// </summary>
        public bool IsDescending { get; set; }
    }
}
