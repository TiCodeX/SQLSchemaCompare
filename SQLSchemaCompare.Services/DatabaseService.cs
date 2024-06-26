﻿namespace TiCodeX.SQLSchemaCompare.Services
{
    /// <summary>
    /// Implementation that provides the mechanisms to read information from a database
    /// </summary>
    public class DatabaseService : IDatabaseService
    {
        /// <summary>
        /// The db provider factory
        /// </summary>
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
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Remove the database since we want to retrieve all of them
            options.Database = string.Empty;
            var provider = this.dbProviderFactory.Create(options);
            return provider.GetDatabaseList().OrderBy(x => x).ToList();
        }

        /// <inheritdoc />
        public ABaseDb GetDatabase(ADatabaseProviderOptions options, TaskInfo taskInfo)
        {
            var provider = this.dbProviderFactory.Create(options);
            return provider.GetDatabase(taskInfo);
        }
    }
}
