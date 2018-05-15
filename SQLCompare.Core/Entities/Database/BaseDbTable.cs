using System;
using System.Collections.Generic;

namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic options for database table classes
    /// </summary>
    public abstract class BaseDbTable
    {
        /// <summary>
        /// Gets or sets database table name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets database table last modification date
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets the database table's columns
        /// </summary>
        public List<BaseDbColumn> Columns { get; } = new List<BaseDbColumn>();
    }
}