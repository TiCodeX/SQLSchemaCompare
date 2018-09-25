using System;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Interfaces;
using SQLCompare.Core.Interfaces.Services;

namespace SQLCompare.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Implementation that creates a database provider
    /// </summary>
    public class DatabaseProviderFactory : IDatabaseProviderFactory
    {
        private readonly ILoggerFactory loggerFactory;
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
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
