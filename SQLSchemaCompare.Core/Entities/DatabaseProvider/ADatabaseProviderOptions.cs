namespace TiCodeX.SQLSchemaCompare.Core.Entities.DatabaseProvider
{
    /// <summary>
    /// Provides generic options for Database Provider Class.
    /// </summary>
    public abstract class ADatabaseProviderOptions
    {
        /// <summary>
        /// Gets or sets the Hostname
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Gets or sets the port
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// Gets or sets the database name
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to save the password in the config
        /// </summary>
        public bool SavePassword { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use SSL for the connection
        /// </summary>
        public bool UseSSL { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore the server certificate
        /// </summary>
        public bool IgnoreServerCertificate { get; set; }

        /// <summary>
        /// Specify whether to serialize the Password property
        /// </summary>
        /// <returns>True if the Password property should be serialized</returns>
        public bool ShouldSerializePassword()
        {
            return this.SavePassword;
        }
    }
}
