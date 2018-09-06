using System;

namespace SQLCompare.Core.Entities.Database.MicrosoftSql
{
    /// <summary>
    /// Specific MicrosoftSql table definition
    /// </summary>
    public class MicrosoftSqlTable : ABaseDbTable
    {
        /// <summary>
        /// Gets or sets the create date
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// Gets or sets the object identifier
        /// </summary>
        public long ObjectId { get; set; }
    }
}