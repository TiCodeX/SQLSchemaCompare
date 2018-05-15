using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Infrastructure.EntityFramework;
using System.Collections.Generic;

namespace SQLCompare.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Retrieves various information from a MySQL Server
    /// </summary>
    internal class MySqlDatabaseProvider : GenericDatabaseProvider<MySqlDb, MySqlTable, MySqlColumn, MySqlDatabaseProviderOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlDatabaseProvider"/> class.
        /// </summary>
        /// <param name="options">The options to connect to the MySQL Database</param>
        public MySqlDatabaseProvider(MySqlDatabaseProviderOptions options)
            : base(options)
        {
        }

        /// <inheritdoc />
        public override BaseDb GetDatabase()
        {
            using (var context = new MySqlDatabaseContext(this.Options))
            {
                return GetCommonDatabase(context);
            }
        }

        /// <inheritdoc />
        public override List<string> GetDatabaseList()
        {
            using (var context = new MySqlDatabaseContext(this.Options))
            {
                return context.Query("SHOW DATABASES");
            }
        }
    }
}
