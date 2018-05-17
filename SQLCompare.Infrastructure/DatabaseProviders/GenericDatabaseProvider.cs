using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Interfaces;
using System.Collections.Generic;

namespace SQLCompare.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Retrieves common information from a Server
    /// </summary>
    /// <typeparam name="TDatabaseProviderOptions">Concrete type of the database provider options</typeparam>
    public abstract class GenericDatabaseProvider<TDatabaseProviderOptions> : IDatabaseProvider
        where TDatabaseProviderOptions : DatabaseProviderOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericDatabaseProvider{TDatabaseProviderOptions}"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory used when using DBContext</param>
        /// <param name="logger">The logger created in the concrete class</param>
        /// <param name="options">The options to connect to the Database</param>
        protected GenericDatabaseProvider(ILoggerFactory loggerFactory, ILogger logger, TDatabaseProviderOptions options)
        {
            this.Options = options;
            this.LoggerFactory = loggerFactory;
            this.Logger = logger;
        }

        /// <summary>
        /// Gets the injected logger
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the injected logger factory
        /// </summary>
        protected ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Gets the options to connect to the Database
        /// </summary>
        protected TDatabaseProviderOptions Options { get; }

        /// <inheritdoc/>
        public abstract BaseDb GetDatabase();

        /// <inheritdoc/>
        public abstract List<string> GetDatabaseList();
    }
}
