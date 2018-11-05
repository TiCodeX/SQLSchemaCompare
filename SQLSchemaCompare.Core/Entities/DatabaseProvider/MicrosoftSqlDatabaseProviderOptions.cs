namespace SQLCompare.Core.Entities.DatabaseProvider
{
    /// <summary>
    /// Provides the options to connect to a Microsoft SQL Server
    /// </summary>
    public class MicrosoftSqlDatabaseProviderOptions : ADatabaseProviderOptions
    {
        /// <summary>
        /// The default port
        /// </summary>
        public const short DefaultPort = 1433;

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftSqlDatabaseProviderOptions"/> class
        /// </summary>
        public MicrosoftSqlDatabaseProviderOptions()
        {
            this.Port = DefaultPort;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use the Windows integrated authentication
        /// </summary>
        public bool UseWindowsAuthentication { get; set; }
    }
}
