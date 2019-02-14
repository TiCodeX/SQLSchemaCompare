using System;
using System.Collections.Generic;
using TiCodeX.SQLSchemaCompare.Core.Enums;

namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database table classes
    /// </summary>
    public abstract class ABaseDbTable : ABaseDbObject
    {
        /// <inheritdoc />
        public override DatabaseObjectType ObjectType { get; } = DatabaseObjectType.Table;

        /// <summary>
        /// Gets or sets database table last modification date
        /// </summary>
        public DateTime? ModifyDate { get; set; }

        /// <summary>
        /// Gets the database table's columns
        /// </summary>
        public List<ABaseDbColumn> Columns { get; } = new List<ABaseDbColumn>();

        /// <summary>
        /// Gets the database table's foreign keys
        /// </summary>
        public List<ABaseDbForeignKey> ForeignKeys { get; } = new List<ABaseDbForeignKey>();

        /// <summary>
        /// Gets the foreign keys referencing this table
        /// </summary>
        public List<ABaseDbForeignKey> ReferencingForeignKeys { get; } = new List<ABaseDbForeignKey>();

        /// <summary>
        /// Gets the database table's primary keys
        /// </summary>
        public List<ABaseDbPrimaryKey> PrimaryKeys { get; } = new List<ABaseDbPrimaryKey>();

        /// <summary>
        /// Gets the database table's indexes
        /// </summary>
        public List<ABaseDbIndex> Indexes { get; } = new List<ABaseDbIndex>();

        /// <summary>
        /// Gets the database table's constraints
        /// </summary>
        public List<ABaseDbConstraint> Constraints { get; } = new List<ABaseDbConstraint>();

        /// <summary>
        /// Gets the database's triggers
        /// </summary>
        public List<ABaseDbTrigger> Triggers { get; } = new List<ABaseDbTrigger>();
    }
}