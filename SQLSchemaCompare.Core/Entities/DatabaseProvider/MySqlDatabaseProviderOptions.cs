namespace TiCodeX.SQLSchemaCompare.Core.Entities.DatabaseProvider
{
    /// <summary>
    /// Provides the options to connect to a MySQL Server
    /// </summary>
    public class MySqlDatabaseProviderOptions : ADatabaseProviderOptions
    {
        /// <summary>
        /// The default port
        /// </summary>
        public static readonly ushort DefaultPort = 3306;

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlDatabaseProviderOptions"/> class
        /// </summary>
        public MySqlDatabaseProviderOptions()
        {
            this.Port = DefaultPort;
        }
    }
}
