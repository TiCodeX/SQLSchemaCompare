using System;

namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Specific MySql table definition
    /// </summary>
    public class MySqlTable : BaseDbTable
    {
        /// <summary>
        /// Gets or sets the table engine
        /// </summary>
        public string Engine { get; set; }

        /// <summary>
        /// Gets or sets the table version
        /// </summary>
        public long? Version { get; set; }

        /// <summary>
        /// Gets or sets the table row format
        /// </summary>
        public string RowFormat { get; set; }

        /// <summary>
        /// Gets or sets the table auto increment
        /// </summary>
        public ulong? AutoIncrement { get; set; }

        /// <summary>
        /// Gets or sets the table creation date
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// Gets or sets the table collation
        /// </summary>
        public string TableCollation { get; set; }

        /// <summary>
        /// Gets or sets the table create options
        /// </summary>
        public string CreateOptions { get; set; }

        /// <summary>
        /// Gets or sets the table comment
        /// </summary>
        public string TableComment { get; set; }
    }
}