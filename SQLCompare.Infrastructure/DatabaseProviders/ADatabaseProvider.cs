using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Interfaces;
using SQLCompare.Infrastructure.EntityFramework;
using System.Collections.Generic;

namespace SQLCompare.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Retrieves common information from a Server
    /// </summary>
    /// <typeparam name="TDatabaseProviderOptions">Concrete type of the database provider options</typeparam>
    /// <typeparam name="TDatabaseContext">Concrete type of the database context</typeparam>
    /// <typeparam name="TDatabase">Concrete type of the database</typeparam>
    /// <typeparam name="TTable">Concrete type of the database table</typeparam>
    /// <typeparam name="TColumn">Concrete type of the database column</typeparam>
    public abstract class ADatabaseProvider<TDatabaseProviderOptions, TDatabaseContext, TDatabase, TTable, TColumn> : IDatabaseProvider
        where TDatabaseProviderOptions : ADatabaseProviderOptions
        where TDatabaseContext : ADatabaseContext<TDatabaseProviderOptions>
        where TDatabase : ABaseDb, new()
        where TTable : ABaseDbTable, new()
        where TColumn : ABaseDbColumn, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ADatabaseProvider{TDatabaseProviderOptions, TDatabaseContext, TDatabase, TTable, TColumn}"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory used when using DBContext</param>
        /// <param name="logger">The logger created in the concrete class</param>
        /// <param name="options">The options to connect to the Database</param>
        protected ADatabaseProvider(ILoggerFactory loggerFactory, ILogger logger, TDatabaseProviderOptions options)
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
        public abstract List<string> GetDatabaseList();

        /// <inheritdoc/>
        public abstract ABaseDb GetDatabase();

        /// <summary>
        /// Discover the complete database structure
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>The discovered database structure</returns>
        protected TDatabase DiscoverDatabase(TDatabaseContext context)
        {
            TDatabase db = new TDatabase() { Name = this.Options.Database };

            var tables = this.GetTables(db, context);

            foreach (var table in tables)
            {
                table.Columns.AddRange(this.GetColumns(table, context));
            }

            db.Tables.AddRange(tables);
            return db;
        }

        /// <summary>
        /// Get the database tables
        /// </summary>
        /// <param name="database">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of tables</returns>
        protected abstract List<TTable> GetTables(TDatabase database, TDatabaseContext context);

        /// <summary>
        /// Get the table columns
        /// </summary>
        /// <param name="table">The database table</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of columns</returns>
        protected abstract List<TColumn> GetColumns(TTable table, TDatabaseContext context);
    }
}
