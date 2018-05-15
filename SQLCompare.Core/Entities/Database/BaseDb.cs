using System;
using System.Collections.Generic;

namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic options of database classes
    /// </summary>
    public abstract class BaseDb
    {
        /// <summary>
        /// Gets or sets the database name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the database last modification date
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets the database's tables
        /// </summary>
        public List<BaseDbTable> Tables { get; } = new List<BaseDbTable>();
    }
}