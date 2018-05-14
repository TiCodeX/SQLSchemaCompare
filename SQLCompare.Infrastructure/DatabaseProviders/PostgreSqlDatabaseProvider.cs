using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Interfaces;
using SQLCompare.Infrastructure.EntityFramework;
using System;
using System.Collections.Generic;

namespace SQLCompare.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Retrieves various information from a PostgreSQL Server
    /// </summary>
    internal class PostgreSqlDatabaseProvider : IDatabaseProvider
    {
        private readonly PostgreSqlDatabaseProviderOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlDatabaseProvider"/> class.
        /// </summary>
        /// <param name="options">The options to connect to the PostgreSQL Database</param>
        public PostgreSqlDatabaseProvider(PostgreSqlDatabaseProviderOptions options)
        {
            this.options = options;
        }

        /// <inheritdoc />
        public BaseDb GetDatabase()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public List<string> GetDatabaseList()
        {
            using (var context = new PostgreSqlDatabaseContext(this.options))
            {
                return context.Query("SELECT datname FROM pg_database WHERE datistemplate = FALSE");
            }
        }
    }
}
