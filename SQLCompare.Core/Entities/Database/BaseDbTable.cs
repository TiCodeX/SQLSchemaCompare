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
        /// Gets or sets the database schema name
        /// </summary>
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the database catalog name
        /// </summary>
        public string CatalogName { get; set; }

        /// <summary>
        /// Gets or sets database table last modification date
        /// </summary>
        public DateTime? ModifyDate { get; set; }

        /// <summary>
        /// Gets the database table's columns
        /// </summary>
        public List<BaseDbColumn> Columns { get; } = new List<BaseDbColumn>();
    }
}