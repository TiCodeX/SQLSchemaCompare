using System;

namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.PostgreSql
{
    /// <summary>
    /// Specific ostgreSql user definition
    /// </summary>
    public class PostgreSqlUser : ABaseDbUser
    {
        /// <summary>
        /// Gets or sets a value indicating whether is super user
        /// </summary>
        public bool SuperUser { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether can create databases
        /// </summary>
        public bool CreateDB { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether can create roles
        /// </summary>
        public bool CreateRole { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is inherited
        /// </summary>
        public bool Inherit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is replication
        /// </summary>
        public bool Replication { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether can bypass RLS
        /// </summary>
        public bool BypassRLS { get; set; }

        /// <summary>
        /// Gets or sets the connection limit
        /// </summary>
        public int ConnectionLimit { get; set; }

        /// <summary>
        /// Gets or sets the valid until
        /// </summary>
        public DateTime ValidUntil { get; set; }
    }
}
