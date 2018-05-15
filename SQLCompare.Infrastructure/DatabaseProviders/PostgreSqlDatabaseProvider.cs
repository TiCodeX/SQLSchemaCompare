using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Infrastructure.EntityFramework;
using System.Collections.Generic;

namespace SQLCompare.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Retrieves various information from a PostgreSQL Server
    /// </summary>
    internal class PostgreSqlDatabaseProvider : GenericDatabaseProvider<PostgreSqlDb, PostgreSqlTable, PostgreSqlColumn, PostgreSqlDatabaseProviderOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlDatabaseProvider"/> class.
        /// </summary>
        /// <param name="options">The options to connect to the PostgreSQL Database</param>
        public PostgreSqlDatabaseProvider(PostgreSqlDatabaseProviderOptions options)
            : base(options)
        {
        }

        /// <inheritdoc />
        public override BaseDb GetDatabase()
        {
            using (var context = new PostgreSqlDatabaseContext(this.Options))
            {
                return GetCommonDatabase(context);
            }
        }

        /// <inheritdoc />
        public override List<string> GetDatabaseList()
        {
            using (var context = new PostgreSqlDatabaseContext(this.Options))
            {
                return context.Query("SELECT datname FROM pg_database WHERE datistemplate = FALSE");
            }
        }
    }
}
