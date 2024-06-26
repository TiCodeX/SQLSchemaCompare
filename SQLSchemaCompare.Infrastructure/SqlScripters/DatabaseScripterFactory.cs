﻿namespace TiCodeX.SQLSchemaCompare.Infrastructure.SqlScripters
{
    /// <summary>
    /// Implementation class for the factory that create a Database scripter
    /// </summary>
    public class DatabaseScripterFactory : IDatabaseScripterFactory
    {
        /// <summary>
        /// The logger factory
        /// </summary>
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseScripterFactory"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected loggerfactory</param>
        public DatabaseScripterFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        /// <inheritdoc/>
        public IDatabaseScripter Create(ABaseDb database, ProjectOptions options)
        {
            switch (database)
            {
                case MicrosoftSqlDb _:
                    return new MicrosoftSqlScripter(this.loggerFactory.CreateLogger(nameof(MicrosoftSqlScripter)), options);
                case MySqlDb _:
                    return new MySqlScripter(this.loggerFactory.CreateLogger(nameof(MySqlScripter)), options);
                case PostgreSqlDb _:
                    return new PostgreSqlScripter(this.loggerFactory.CreateLogger(nameof(PostgreSqlScripter)), options);
                default:
                    throw new NotSupportedException("Unknown Database Type");
            }
        }
    }
}
