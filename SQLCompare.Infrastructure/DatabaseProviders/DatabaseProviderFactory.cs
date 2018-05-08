using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Interfaces;
using System;

namespace SQLCompare.Infrastructure.DatabaseProviders
{
    /// <inheritdoc/>
    public class DatabaseProviderFactory : IDatabaseProviderFactory
    {
        /// <inheritdoc/>
        public IDatabaseProvider Create(DatabaseProviderOptions dbpo)
        {
            if (dbpo is MicrosoftSqlDatabaseProviderOptions microsoftSqlOptions)
            {
                return new MicrosoftSqlDatabaseProvider(microsoftSqlOptions);
            }

            if (dbpo is MySqlDatabaseProviderOptions mySqlOptions)
            {
                return new MySqlDatabaseProvider(mySqlOptions);
            }

            if (dbpo is PostgreSqlDatabaseProviderOptions postgreSqlOptions)
            {
                return new PostgreSqlDatabaseProvider(postgreSqlOptions);
            }

            throw new NotImplementedException();
        }
    }
}
