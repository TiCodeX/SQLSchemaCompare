namespace SQLCompare.Core.Entities.DatabaseProvider
{
    /// <summary>
    /// Provides the options to connect to a PostgreSQL Server
    /// </summary>
    public class PostgreSqlDatabaseProviderOptions : ADatabaseProviderOptions
    {
        /// <summary>
        /// The default port
        /// </summary>
        public const short DefaultPort = 5432;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlDatabaseProviderOptions"/> class
        /// </summary>
        public PostgreSqlDatabaseProviderOptions()
        {
            this.Port = DefaultPort;
        }
    }
}
