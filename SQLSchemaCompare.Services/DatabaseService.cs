namespace TiCodeX.SQLSchemaCompare.Services;

/// <summary>
/// Implementation that provides the mechanisms to read information from a database
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DatabaseService"/> class.
/// </remarks>
/// <param name="dbProviderFactory">The injected database provider factory</param>
public class DatabaseService(IDatabaseProviderFactory dbProviderFactory) : IDatabaseService
{
    /// <summary>
    /// The db provider factory
    /// </summary>
    private readonly IDatabaseProviderFactory dbProviderFactory = dbProviderFactory;

    /// <inheritdoc />
    public List<string> ListDatabases(ADatabaseProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // Remove the database since we want to retrieve all of them
        options.Database = string.Empty;
        var provider = this.dbProviderFactory.Create(options);
        return [.. provider.GetDatabaseList().OrderBy(x => x)];
    }

    /// <inheritdoc />
    public ABaseDb GetDatabase(ADatabaseProviderOptions options, TaskInfo taskInfo)
    {
        var provider = this.dbProviderFactory.Create(options);
        return provider.GetDatabase(taskInfo);
    }
}
