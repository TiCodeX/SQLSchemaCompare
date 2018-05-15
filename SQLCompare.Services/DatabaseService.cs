using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Interfaces;
using SQLCompare.Core.Interfaces.Services;
using System.Collections.Generic;

namespace SQLCompare.Services
{
    /// <inheritdoc />
    public class DatabaseService : IDatabaseService
    {
        private readonly IDatabaseProviderFactory dbProviderFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseService"/> class.
        /// </summary>
        /// <param name="dbProviderFactory">The injected database provider factory</param>
        public DatabaseService(IDatabaseProviderFactory dbProviderFactory)
        {
            this.dbProviderFactory = dbProviderFactory;
        }

        /// <inheritdoc />
        public List<string> ListDatabases(DatabaseProviderOptions options)
        {
            var provider = this.dbProviderFactory.Create(options);
            return provider.GetDatabaseList();
        }

        /// <inheritdoc />
        public BaseDb GetDatabase(DatabaseProviderOptions options)
        {
            var provider = this.dbProviderFactory.Create(options);
            return provider.GetDatabase();
        }
    }
}
