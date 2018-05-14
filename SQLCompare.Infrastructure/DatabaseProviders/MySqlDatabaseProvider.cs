using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Interfaces;
using SQLCompare.Infrastructure.EntityFramework;
using System.Collections.Generic;

namespace SQLCompare.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Retrieves various information from a MySQL Server
    /// </summary>
    internal class MySqlDatabaseProvider : IDatabaseProvider
    {
        private readonly MySqlDatabaseProviderOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlDatabaseProvider"/> class.
        /// </summary>
        /// <param name="options">The options to connect to the MySQL Database</param>
        public MySqlDatabaseProvider(MySqlDatabaseProviderOptions options)
        {
            this.options = options;
        }

        /// <inheritdoc />
        public BaseDb GetDatabase()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public List<string> GetDatabaseList()
        {
            using (var context = new MySqlDatabaseContext(this.options))
            {
                return context.Query("SHOW DATABASES");
            }
        }
    }
}
