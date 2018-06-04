using System;
using System.Collections.Generic;

namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information of database classes
    /// </summary>
    public abstract class ABaseDb
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
        public List<ABaseDbTable> Tables { get; } = new List<ABaseDbTable>();
    }
}