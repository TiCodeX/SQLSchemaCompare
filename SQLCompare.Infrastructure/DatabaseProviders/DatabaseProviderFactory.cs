using System;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Interfaces;

namespace SQLCompare.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Implementation that creates a database provider
    /// </summary>
    public class DatabaseProviderFactory : IDatabaseProviderFactory
    {
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseProviderFactory"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        public DatabaseProviderFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        /// <inheritdoc/>
        public IDatabaseProvider Create(ADatabaseProviderOptions dbpo)
        {
            switch (dbpo)
            {
                case MicrosoftSqlDatabaseProviderOptions microsoftSqlOptions:
                    return new MicrosoftSqlDatabaseProvider(this.loggerFactory, microsoftSqlOptions);
                case MySqlDatabaseProviderOptions mySqlOptions:
                    return new MySqlDatabaseProvider(this.loggerFactory, mySqlOptions);
                case PostgreSqlDatabaseProviderOptions postgreSqlOptions:
                    return new PostgreSqlDatabaseProvider(this.loggerFactory, postgreSqlOptions);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
