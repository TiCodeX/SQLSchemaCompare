﻿using System.Collections.Generic;

namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information of database classes
    /// </summary>
    public abstract class ABaseDb : ABaseDbObject
    {
        /// <summary>
        /// Gets the database tables
        /// </summary>
        public List<ABaseDbTable> Tables { get; } = new List<ABaseDbTable>();

        /// <summary>
        /// Gets the database indexes
        /// </summary>
        public List<ABaseDbIndex> Indexes { get; } = new List<ABaseDbIndex>();

        /// <summary>
        /// Gets the database constraints
        /// </summary>
        public List<ABaseDbConstraint> Constraints { get; } = new List<ABaseDbConstraint>();

        /// <summary>
        /// Gets the database foreign keys
        /// </summary>
        public List<ABaseDbForeignKey> ForeignKeys { get; } = new List<ABaseDbForeignKey>();

        /// <summary>
        /// Gets the database triggers
        /// </summary>
        public List<ABaseDbTrigger> Triggers { get; } = new List<ABaseDbTrigger>();

        /// <summary>
        /// Gets the database views
        /// </summary>
        public List<ABaseDbView> Views { get; } = new List<ABaseDbView>();

        /// <summary>
        /// Gets the database functions
        /// </summary>
        public List<ABaseDbFunction> Functions { get; } = new List<ABaseDbFunction>();

        /// <summary>
        /// Gets the database stored procedures
        /// </summary>
        public List<ABaseDbStoredProcedure> StoredProcedures { get; } = new List<ABaseDbStoredProcedure>();

        /// <summary>
        /// Gets the database data types
        /// </summary>
        public List<ABaseDbDataType> DataTypes { get; } = new List<ABaseDbDataType>();

        /// <summary>
        /// Gets the database sequences
        /// </summary>
        public List<ABaseDbSequence> Sequences { get; } = new List<ABaseDbSequence>();
    }
}