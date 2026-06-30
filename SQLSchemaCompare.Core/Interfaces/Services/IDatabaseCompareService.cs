namespace TiCodeX.SQLSchemaCompare.Core.Interfaces.Services;

/// <summary>
/// Defines a class that provides the mechanisms to compare two database instances
/// </summary>
public interface IDatabaseCompareService
{
    /// <summary>
    /// Compares two databases
    /// </summary>
    /// <param name="waitBeforeRetrieveTargetDatabase">Indicates whether to wait before retrieving the target database</param>
    void StartCompare(bool waitBeforeRetrieveTargetDatabase);
}
