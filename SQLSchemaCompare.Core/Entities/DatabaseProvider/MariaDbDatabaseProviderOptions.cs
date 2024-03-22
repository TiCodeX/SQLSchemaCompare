namespace TiCodeX.SQLSchemaCompare.Core.Entities.DatabaseProvider
{
    /// <summary>
    /// Provides the options to connect to a MariaDB Server
    /// </summary>
    public class MariaDbDatabaseProviderOptions : ADatabaseProviderOptions
    {
        /// <summary>
        /// The default port
        /// </summary>
        public static readonly ushort DefaultPort = 3306;

        /// <summary>
        /// Initializes a new instance of the <see cref="MariaDbDatabaseProviderOptions"/> class
        /// </summary>
        public MariaDbDatabaseProviderOptions()
        {
            this.Port = DefaultPort;
        }
    }
}
