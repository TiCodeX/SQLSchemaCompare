namespace TiCodeX.SQLSchemaCompare.Infrastructure.DatabaseProviders
{
    using System;
    using Microsoft.Extensions.Logging;
    using TiCodeX.SQLSchemaCompare.Core.Entities.DatabaseProvider;
    using TiCodeX.SQLSchemaCompare.Core.Interfaces;
    using TiCodeX.SQLSchemaCompare.Core.Interfaces.Services;

    /// <summary>
    /// Implementation that creates a database provider
    /// </summary>
    public class DatabaseProviderFactory : IDatabaseProviderFactory
    {
        /// <summary>
        /// The logger factory
        /// </summary>
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// The cipher service
        /// </summary>
        private readonly ICipherService cipherService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseProviderFactory"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="cipherService">The injected cipher service</param>
        public DatabaseProviderFactory(ILoggerFactory loggerFactory, ICipherService cipherService)
        {
            this.loggerFactory = loggerFactory;
            this.cipherService = cipherService;
        }

        /// <inheritdoc/>
        public IDatabaseProvider Create(ADatabaseProviderOptions dbpo)
        {
            switch (dbpo)
            {
                case MicrosoftSqlDatabaseProviderOptions microsoftSqlOptions:
                    return new MicrosoftSqlDatabaseProvider(this.loggerFactory, this.cipherService, microsoftSqlOptions);
                case MySqlDatabaseProviderOptions mySqlOptions:
                    return new MySqlDatabaseProvider(this.loggerFactory, this.cipherService, mySqlOptions);
                case PostgreSqlDatabaseProviderOptions postgreSqlOptions:
                    return new PostgreSqlDatabaseProvider(this.loggerFactory, this.cipherService, postgreSqlOptions);
                case MariaDbDatabaseProviderOptions mariaDbOptions:
                    return new MariaDbDatabaseProvider(this.loggerFactory, this.cipherService, mariaDbOptions);
                default:
                    throw new NotSupportedException("Unknown Database Type");
            }
        }
    }
}
