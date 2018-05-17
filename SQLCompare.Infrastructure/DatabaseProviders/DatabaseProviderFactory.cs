using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Interfaces;
using System;

namespace SQLCompare.Infrastructure.DatabaseProviders
{
    /// <inheritdoc/>
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
        public IDatabaseProvider Create(DatabaseProviderOptions dbpo)
        {
            if (dbpo is MicrosoftSqlDatabaseProviderOptions microsoftSqlOptions)
            {
                return new MicrosoftSqlDatabaseProvider(this.loggerFactory, microsoftSqlOptions);
            }

            if (dbpo is MySqlDatabaseProviderOptions mySqlOptions)
            {
                return new MySqlDatabaseProvider(this.loggerFactory, mySqlOptions);
            }

            if (dbpo is PostgreSqlDatabaseProviderOptions postgreSqlOptions)
            {
                return new PostgreSqlDatabaseProvider(this.loggerFactory, postgreSqlOptions);
            }

            throw new NotImplementedException();
        }
    }
}
