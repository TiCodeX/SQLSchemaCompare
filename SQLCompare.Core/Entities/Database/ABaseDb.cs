using System;
using System.Collections.Generic;

namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information of database classes
    /// </summary>
    public abstract class ABaseDb : ABaseDbObject
    {
        /// <summary>
        /// Gets or sets the database last modification date
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets the database's tables
        /// </summary>
        public List<ABaseDbTable> Tables { get; } = new List<ABaseDbTable>();

        /// <summary>
        /// Gets the database's views
        /// </summary>
        public List<ABaseDbView> Views { get; } = new List<ABaseDbView>();

        /// <summary>
        /// Gets the database's functions
        /// </summary>
        public List<ABaseDbFunction> Functions { get; } = new List<ABaseDbFunction>();

        /// <summary>
        /// Gets the database's stored procedures
        /// </summary>
        public List<ABaseDbStoredProcedure> StoredProcedures { get; } = new List<ABaseDbStoredProcedure>();

        /// <summary>
        /// Gets the database's data types
        /// </summary>
        public List<ABaseDbDataType> DataTypes { get; } = new List<ABaseDbDataType>();

        /// <summary>
        /// Gets the database's sequences
        /// </summary>
        public List<ABaseDbSequence> Sequences { get; } = new List<ABaseDbSequence>();
    }
}