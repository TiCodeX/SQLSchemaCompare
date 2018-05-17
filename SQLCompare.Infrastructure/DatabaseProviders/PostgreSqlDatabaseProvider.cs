using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Infrastructure.EntityFramework;
using System.Collections.Generic;

namespace SQLCompare.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Retrieves various information from a PostgreSQL Server
    /// </summary>
    internal class PostgreSqlDatabaseProvider : GenericDatabaseProvider<PostgreSqlDatabaseProviderOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlDatabaseProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="options">The options to connect to the PostgreSQL Database</param>
        public PostgreSqlDatabaseProvider(ILoggerFactory loggerFactory, PostgreSqlDatabaseProviderOptions options)
            : base(loggerFactory, loggerFactory.CreateLogger("PostgreSqlDatabaseProvider"), options)
        {
        }

        /// <inheritdoc />
        public override BaseDb GetDatabase()
        {
            using (var context = new PostgreSqlDatabaseContext(this.LoggerFactory, this.Options))
            {
                return null;
            }
        }

        /// <inheritdoc />
        public override List<string> GetDatabaseList()
        {
            using (var context = new PostgreSqlDatabaseContext(this.LoggerFactory, this.Options))
            {
                return context.Query("SELECT datname FROM pg_database WHERE datistemplate = FALSE");
            }
        }
    }
}
