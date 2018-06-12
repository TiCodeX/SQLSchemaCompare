namespace SQLCompare.Core.Entities.Database.MicrosoftSql
{
    /// <summary>
    /// Specific MicrosoftSql foreign key definition
    /// </summary>
    public class MicrosoftSqlForeignKey : ABaseDbConstraint
    {
        /// <summary>
        /// List of possible action for foreign keys for ON DELETE and ON UPDATE
        /// </summary>
        public enum ReferentialAction
        {
            /// <summary>
            /// No action
            /// </summary>
            NOACTION = 0,

            /// <summary>
            /// Cascade
            /// </summary>
            CASCADE = 1,

            /// <summary>
            /// Set null
            /// </summary>
            SETNULL = 2,

            /// <summary>
            /// Set default
            /// </summary>
            SETDEFAULT = 3
        }

        /// <summary>
        /// Gets or sets table column
        /// </summary>
        public string TableColumn { get; set; }

        /// <summary>
        /// Gets or sets referenced table schema
        /// </summary>
        public string ReferencedTableSchema { get; set; }

        /// <summary>
        /// Gets or sets referenced table name
        /// </summary>
        public string ReferencedTableName { get; set; }

        /// <summary>
        /// Gets or sets referenced table column
        /// </summary>
        public string ReferencedTableColumn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the foreign key is deferrable
        /// </summary>
        public bool IsDeferrable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the foreign key is initially deferred
        /// </summary>
        public bool InitiallyDeferred { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the foreign key is Ms shipped
        /// </summary>
        public bool IsMsShipped { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the foreign key is published
        /// </summary>
        public bool IsPublished { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the foreign key is schema published
        /// </summary>
        public bool IsSchemaPublished { get; set; }

        /// <summary>
        /// Gets or sets the key index id
        /// </summary>
        public int? KeyIndexId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the foreign key is disabled
        /// </summary>
        public bool IsDisabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the foreign key is not for replication
        /// </summary>
        public bool IsNotForReplication { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the foreign key is not trusted
        /// </summary>
        public bool IsNotTrusted { get; set; }

        /// <summary>
        /// Gets or sets the delete referential action
        /// </summary>
        public ReferentialAction DeleteReferentialAction { get; set; }

        /// <summary>
        /// Gets or sets the update referential action
        /// </summary>
        public ReferentialAction UpdateReferentialAction { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the foreign key is system named
        /// </summary>
        public bool IsSystemNamed { get; set; }
    }
}
