namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.MySql
{
    /// <summary>
    /// Specific MySql user definition
    /// </summary>
    public class MySqlUser : ABaseDbUser
    {
        /// <summary>
        /// Gets or sets the host
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the authentication plugin
        /// </summary>
        public string AuthPlugin { get; set; }

        /// <summary>
        /// Gets or sets the maximum queries per hour
        /// </summary>
        public uint MaxQueriesPerHour { get; set; }

        /// <summary>
        /// Gets or sets the maximum updates per hour
        /// </summary>
        public uint MaxUpdatesPerHour { get; set; }

        /// <summary>
        /// Gets or sets the maximum connections per hour
        /// </summary>
        public uint MaxConnectionsPerHour { get; set; }

        /// <summary>
        /// Gets or sets the maximum user connections
        /// </summary>
        public uint MaxUserConnections { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the password is expired
        /// </summary>
        public bool PasswordExpired { get; set; }

        /// <summary>
        /// Gets or sets the password lifetime
        /// </summary>
        public ushort PasswordLifetime { get; set; }

        /// <summary>
        /// Gets or sets the password reuse
        /// </summary>
        public ushort PasswordReuse { get; set; }

        /// <summary>
        /// Gets or sets the password require
        /// </summary>
        public ushort PasswordRequire { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the account is locked
        /// </summary>
        public bool AccountLocked { get; set; }
    }
}
