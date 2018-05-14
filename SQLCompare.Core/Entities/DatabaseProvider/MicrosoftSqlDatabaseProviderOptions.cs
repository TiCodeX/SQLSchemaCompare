namespace SQLCompare.Core.Entities.DatabaseProvider
{
    /// <summary>
    /// Provides the options to connect to a Microsoft SQL Server
    /// </summary>
    public class MicrosoftSqlDatabaseProviderOptions : DatabaseProviderOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to use the Windows integrated authentication
        /// </summary>
        public bool UseWindowsAuthentication { get; set; }
    }
}
