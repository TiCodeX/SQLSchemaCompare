namespace TiCodeX.SQLSchemaCompare.Infrastructure.DatabaseProviders;

/// <summary>
/// Implementation that creates a database provider
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DatabaseProviderFactory"/> class.
/// </remarks>
/// <param name="loggerFactory">The injected logger factory</param>
/// <param name="cipherService">The injected cipher service</param>
public class DatabaseProviderFactory(ILoggerFactory loggerFactory, ICipherService cipherService) : IDatabaseProviderFactory
{
    /// <summary>
    /// The logger factory
    /// </summary>
    private readonly ILoggerFactory loggerFactory = loggerFactory;

    /// <summary>
    /// The cipher service
    /// </summary>
    private readonly ICipherService cipherService = cipherService;

    /// <inheritdoc/>
    public IDatabaseProvider Create(ADatabaseProviderOptions dbpo)
    {
        return dbpo switch
        {
            MicrosoftSqlDatabaseProviderOptions microsoftSqlOptions => new MicrosoftSqlDatabaseProvider(this.loggerFactory, this.cipherService, microsoftSqlOptions),
            MariaDbDatabaseProviderOptions mariaDbOptions => new MariaDbDatabaseProvider(this.loggerFactory, this.cipherService, mariaDbOptions),
            MySqlDatabaseProviderOptions mySqlOptions => new MySqlDatabaseProvider(this.loggerFactory, this.cipherService, mySqlOptions),
            PostgreSqlDatabaseProviderOptions postgreSqlOptions => new PostgreSqlDatabaseProvider(this.loggerFactory, this.cipherService, postgreSqlOptions),
            _ => throw new NotSupportedException("Unknown Database Type"),
        };
    }
}
