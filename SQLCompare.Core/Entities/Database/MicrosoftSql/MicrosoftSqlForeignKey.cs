namespace SQLCompare.Core.Entities.Database.MicrosoftSql
{
    /// <summary>
    /// Specific MicrosoftSql foreign key definition
    /// </summary>
    public class MicrosoftSqlForeignKey : ABaseDbForeignKey
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
        /// Gets or sets the delete referential action
        /// </summary>
        public ReferentialAction DeleteReferentialAction { get; set; }

        /// <summary>
        /// Gets or sets the update referential action
        /// </summary>
        public ReferentialAction UpdateReferentialAction { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the foreign key is disabled
        /// </summary>
        public bool Disabled { get; set; }
    }
}
