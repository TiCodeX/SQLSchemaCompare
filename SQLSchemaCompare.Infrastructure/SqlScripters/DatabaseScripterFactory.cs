namespace TiCodeX.SQLSchemaCompare.Infrastructure.SqlScripters
{
    /// <summary>
    /// Implementation class for the factory that create a Database scripter
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="DatabaseScripterFactory"/> class.
    /// </remarks>
    /// <param name="loggerFactory">The injected loggerfactory</param>
    public class DatabaseScripterFactory(ILoggerFactory loggerFactory) : IDatabaseScripterFactory
    {
        /// <summary>
        /// The logger factory
        /// </summary>
        private readonly ILoggerFactory loggerFactory = loggerFactory;

        /// <inheritdoc/>
        public IDatabaseScripter Create(ABaseDb database, ProjectOptions options)
        {
            return database switch
            {
                MicrosoftSqlDb => new MicrosoftSqlScripter(this.loggerFactory.CreateLogger(nameof(MicrosoftSqlScripter)), options),
                MySqlDb => new MySqlScripter(this.loggerFactory.CreateLogger(nameof(MySqlScripter)), options),
                PostgreSqlDb => new PostgreSqlScripter(this.loggerFactory.CreateLogger(nameof(PostgreSqlScripter)), options),
                _ => throw new NotSupportedException("Unknown Database Type"),
            };
        }
    }
}
