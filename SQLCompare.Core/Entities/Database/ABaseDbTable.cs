using System;
using System.Collections.Generic;

namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database table classes
    /// </summary>
    public abstract class ABaseDbTable : ABaseDbObject
    {
        /// <summary>
        /// Gets or sets database table last modification date
        /// </summary>
        public DateTime? ModifyDate { get; set; }

        /// <summary>
        /// Gets the database table's columns
        /// </summary>
        public List<ABaseDbColumn> Columns { get; } = new List<ABaseDbColumn>();

        /// <summary>
        /// Gets the database table's foreing keys
        /// </summary>
        public List<ABaseDbConstraint> ForeignKeys { get; } = new List<ABaseDbConstraint>();

        /// <summary>
        /// Gets the database table's primary keys
        /// </summary>
        public List<ABaseDbConstraint> PrimaryKeys { get; } = new List<ABaseDbConstraint>();

        /// <summary>
        /// Gets the database table's indexes
        /// </summary>
        public List<ABaseDbConstraint> Indexes { get; } = new List<ABaseDbConstraint>();
    }
}