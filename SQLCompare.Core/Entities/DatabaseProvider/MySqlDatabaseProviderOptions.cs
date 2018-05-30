namespace SQLCompare.Core.Entities.DatabaseProvider
{
    /// <summary>
    /// Provides the options to connect to a MySQL Server
    /// </summary>
    public class MySqlDatabaseProviderOptions : ADatabaseProviderOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to use SSL for the connection
        /// </summary>
        public bool UseSSL { get; set; }
    }
}
