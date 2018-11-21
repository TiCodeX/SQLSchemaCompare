using System.Collections.Generic;

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
        /// <remarks>Used only by the DatabaseProvider to group the indexes and fill the ColumnDescending list</remarks>
        public bool IsDescending { get; set; }

        /// <summary>
        /// Gets whether the column is descending, sorted like the ColumnNames list
        /// </summary>
        public List<bool> ColumnDescending { get; } = new List<bool>();
    }
}
