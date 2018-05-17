using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Infrastructure.EntityFramework;
using System.Collections.Generic;

namespace SQLCompare.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Retrieves various information from a MySQL Server
    /// </summary>
    internal class MySqlDatabaseProvider : GenericDatabaseProvider<MySqlDatabaseProviderOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlDatabaseProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="options">The options to connect to the MySQL Database</param>
        public MySqlDatabaseProvider(ILoggerFactory loggerFactory, MySqlDatabaseProviderOptions options)
            : base(loggerFactory, loggerFactory.CreateLogger("MySqlDatabaseProvider"), options)
        {
        }

        /// <inheritdoc />
        public override BaseDb GetDatabase()
        {
            using (var context = new MySqlDatabaseContext(this.LoggerFactory, this.Options))
            {
                return null;
            }
        }

        /// <inheritdoc />
        public override List<string> GetDatabaseList()
        {
            using (var context = new MySqlDatabaseContext(this.LoggerFactory, this.Options))
            {
                return context.Query("SHOW DATABASES");
            }
        }
    }
}
