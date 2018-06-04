using System;
using System.Collections.Generic;

namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database table classes
    /// </summary>
    public abstract class ABaseDbTable
    {
        /// <summary>
        /// Gets or sets database table name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the database table schema
        /// </summary>
        public string TableSchema { get; set; }

        /// <summary>
        /// Gets or sets the database table catalog
        /// </summary>
        public string TableCatalog { get; set; }

        /// <summary>
        /// Gets or sets database table last modification date
        /// </summary>
        public DateTime? ModifyDate { get; set; }

        /// <summary>
        /// Gets the database table's columns
        /// </summary>
        public List<ABaseDbColumn> Columns { get; } = new List<ABaseDbColumn>();
    }
}