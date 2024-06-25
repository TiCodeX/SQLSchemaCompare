namespace TiCodeX.SQLSchemaCompare.UI
{
    /// <summary>
    /// Global configuration of the application
    /// </summary>
    internal class AppGlobals : IAppGlobals
    {
        /// <inheritdoc/>
        public string CompanyName => "TiCodeX";

        /// <inheritdoc/>
        public string ProductName => "SQL Schema Compare";

        /// <inheritdoc/>
        public bool IsDevelopment
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        /// <inheritdoc/>
        public string AuthorizationHeaderName => "CustomAuthToken";

        /// <inheritdoc/>
        public string AppSettingsFullFilename => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".SQLSchemaCompare",
            "SQLSchemaCompare.conf");

        /// <inheritdoc/>
        public string LoggerLayout =>
            "${longdate}|${event-properties:item=EventId_Id}|${uppercase:${level}}|${logger}|${message} ${exception:format=tostring}";

        /// <inheritdoc/>
        public string LoggerFile => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".SQLSchemaCompare",
            "log",
            @"SQLSchemaCompare.${shortdate}-service.log");

        /// <inheritdoc/>
        public int LoggerMaxArchiveFiles => 9;

        /// <inheritdoc/>
        public string ElectronAuthAppId => "queieimiugrepqueieimiucrap";

        /// <inheritdoc/>
        public string ProductCode => "SQLCMP";

        /// <inheritdoc/>
        public string AppVersion { get; set; }
    }
}
