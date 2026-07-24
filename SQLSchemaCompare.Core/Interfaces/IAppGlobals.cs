namespace TiCodeX.SQLSchemaCompare.Core.Interfaces;

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

    /// <summary>
    /// Gets or sets the application version
    /// </summary>
    string AppVersion { get; set; }
}
