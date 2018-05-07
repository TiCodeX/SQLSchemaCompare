namespace SQLCompare.Core.Entities.DatabaseProvider
{
    /// <summary>
    /// Provides the options to connect to a Microsoft SQL Server
    /// </summary>
    public class MicrosoftSqlDatabaseProviderOptions : DatabaseProviderOptions
    {
        /// <summary>
        /// Gets or sets the Hostname
        /// </summary>
        public string Hostname { get; set; }

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
    }
}
