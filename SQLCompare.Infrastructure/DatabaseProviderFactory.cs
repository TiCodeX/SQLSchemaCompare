using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Interfaces;
using System;

namespace SQLCompare.Infrastructure
{
    /// <inheritdoc/>
    public class DatabaseProviderFactory : IDatabaseProviderFactory
    {
        /// <inheritdoc/>
        public IDatabaseProvider Create(DatabaseProviderOptions dbpo)
        {
            if (dbpo is MicrosoftSqlDatabaseProviderOptions microsoftSqlDatabaseProviderOptions)
            {
                return new MicrosoftSqlDatabaseProvider(microsoftSqlDatabaseProviderOptions);
            }

            throw new NotImplementedException();
        }
    }
}
