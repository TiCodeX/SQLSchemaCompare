namespace SQLCompare.Core.Interfaces
{
    /// <summary>
    /// Defines a class that provides global constants
    /// </summary>
    public interface IAppGlobals
    {
        /// <summary>
        /// Gets the company name
        /// </summary>
        string CompanyName { get; }

        /// <summary>
        /// Gets the product name
        /// </summary>
        string ProductName { get; }

        /// <summary>
        /// Gets a value indicating whether the solution configuration is in Debug
        /// </summary>
        bool IsDevelopment { get; }

        /// <summary>
        /// Gets the initial port for the range of the WebServer
        /// </summary>
        int StartPortRange { get; }

        /// <summary>
        /// Gets the final port for the range of the WebServer
        /// </summary>
        int EndPortRange { get; }

        /// <summary>
        /// Gets the header attribute name for the authentication
        /// </summary>
        string AuthorizationHeaderName { get; }

        /// <summary>
        /// Gets the full filename of the application settings file
        /// </summary>
        string AppSettingsFullFilename { get; }

        /// <summary>
        /// Gets the layout for the logger
        /// </summary>
        string LoggerLayout { get; }

        /// <summary>
        /// Gets the path to save the log file
        /// </summary>
        string LoggerFile { get; }

        /// <summary>
        /// Gets the logger maximum archive files
        /// </summary>
        int LoggerMaxArchiveFiles { get; }
    }
}
