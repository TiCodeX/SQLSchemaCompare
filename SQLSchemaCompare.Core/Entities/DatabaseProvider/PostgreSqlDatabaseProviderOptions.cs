namespace TiCodeX.SQLSchemaCompare.Core.Entities.DatabaseProvider
{
    /// <summary>
    /// Provides the options to connect to a PostgreSQL Server
    /// </summary>
    public class PostgreSqlDatabaseProviderOptions : ADatabaseProviderOptions
    {
        /// <summary>
        /// The default port
        /// </summary>
        public static readonly ushort DefaultPort = 5432;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlDatabaseProviderOptions"/> class
        /// </summary>
        public PostgreSqlDatabaseProviderOptions()
        {
            this.Port = DefaultPort;
        }
    }
}
