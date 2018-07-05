using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Interfaces;
using SQLCompare.Core.Interfaces.Services;
using System.Collections.Generic;

namespace SQLCompare.Services
{
    /// <summary>
    /// Implementation that provides the mechanisms to read information from a database
    /// </summary>
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
        public List<string> ListDatabases(ADatabaseProviderOptions options)
        {
            // Remove the database since we want to retrieve all of them
            options.Database = string.Empty;
            var provider = this.dbProviderFactory.Create(options);
            return provider.GetDatabaseList();
        }

        /// <inheritdoc />
        public ABaseDb GetDatabase(ADatabaseProviderOptions options)
        {
            var provider = this.dbProviderFactory.Create(options);
            return provider.GetDatabase();
        }
    }
}
