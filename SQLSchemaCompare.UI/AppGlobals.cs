namespace TiCodeX.SQLSchemaCompare.UI;

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
    public string AppSettingsFullFilename => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".SQLSchemaCompare",
        "SQLSchemaCompare.conf");

    /// <inheritdoc/>
    public string LoggerLayout =>
        "${longdate}|${event-properties:EventId}|${uppercase:${level}}|${logger}|${message} ${exception:format=tostring}";

    /// <inheritdoc/>
    public string LoggerFile => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".SQLSchemaCompare",
        "log",
        @"SQLSchemaCompare.${shortdate}-service.log");

    /// <inheritdoc/>
    public int LoggerMaxArchiveFiles => 9;

    /// <inheritdoc/>
    public string AppVersion { get; set; }
}
