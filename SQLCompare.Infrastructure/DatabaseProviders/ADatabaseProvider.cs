using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Interfaces;
using SQLCompare.Infrastructure.EntityFramework;
using System.Collections.Generic;
using System.Linq;

namespace SQLCompare.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Retrieves common information from a Server
    /// </summary>
    /// <typeparam name="TDatabaseProviderOptions">Concrete type of the database provider options</typeparam>
    /// <typeparam name="TDatabaseContext">Concrete type of the database context</typeparam>
    /// <typeparam name="TDatabase">Concrete type of the database</typeparam>
    public abstract class ADatabaseProvider<TDatabaseProviderOptions, TDatabaseContext, TDatabase> : IDatabaseProvider
        where TDatabaseProviderOptions : ADatabaseProviderOptions
        where TDatabaseContext : ADatabaseContext<TDatabaseProviderOptions>
        where TDatabase : ABaseDb, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ADatabaseProvider{TDatabaseProviderOptions, TDatabaseContext, TDatabase}"/> class.
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
            var db = new TDatabase { Name = context.Database.GetDbConnection().Database };

            var tables = this.GetTables(db, context);
            var columns = this.GetColumns(db, context);
            var primaryKeys = this.GetPrimaryKeys(db, context);
            var foreignKeys = this.GetForeignKeys(db, context);
            var views = this.GetViews(db, context);
            var functions = this.GetFunctions(db, context);
            var storeProcedures = this.GetStoreProcedures(db, context);

            foreach (var table in tables)
            {
                table.Columns.AddRange(
                    columns.Where(y => string.Equals(table.Catalog, y.Catalog, System.StringComparison.Ordinal)
                                           && string.Equals(table.Schema, y.Schema, System.StringComparison.Ordinal)
                                           && string.Equals(table.Name, y.TableName, System.StringComparison.Ordinal)));
                table.ForeignKeys.AddRange(
                    foreignKeys.Where(y => string.Equals(table.Catalog, y.TableCatalog, System.StringComparison.Ordinal)
                                           && string.Equals(table.Schema, y.TableSchema, System.StringComparison.Ordinal)
                                           && string.Equals(table.Name, y.TableName, System.StringComparison.Ordinal)));
                table.PrimaryKeys.AddRange(
                    primaryKeys.Where(y => string.Equals(table.Catalog, y.TableCatalog, System.StringComparison.Ordinal)
                                           && string.Equals(table.Schema, y.TableSchema, System.StringComparison.Ordinal)
                                           && string.Equals(table.Name, y.TableName, System.StringComparison.Ordinal)));
            }

            db.Tables.AddRange(tables);
            db.Views.AddRange(views);
            db.Functions.AddRange(functions);
            db.StoreProcedures.AddRange(storeProcedures);

            return db;
        }

        /// <summary>
        /// Get the database tables
        /// </summary>
        /// <param name="database">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of tables</returns>
        protected abstract IEnumerable<ABaseDbTable> GetTables(TDatabase database, TDatabaseContext context);

        /// <summary>
        /// Get the table columns
        /// </summary>
        /// <param name="database">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of columns</returns>
        protected abstract IEnumerable<ABaseDbColumn> GetColumns(TDatabase database, TDatabaseContext context);

        /// <summary>
        /// Get the table primary keys
        /// </summary>
        /// <param name="database">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of primary keys</returns>
        protected abstract IEnumerable<ABaseDbConstraint> GetPrimaryKeys(TDatabase database, TDatabaseContext context);

        /// <summary>
        /// Get the table foreign keys
        /// </summary>
        /// <param name="database">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of foreign keys</returns>
        protected abstract IEnumerable<ABaseDbConstraint> GetForeignKeys(TDatabase database, TDatabaseContext context);

        /// <summary>
        /// Get the database views
        /// </summary>
        /// <param name="db">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of views</returns>
        protected abstract IEnumerable<ABaseDbView> GetViews(TDatabase db, TDatabaseContext context);

        /// <summary>
        /// Get the database functions
        /// </summary>
        /// <param name="db">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of functions</returns>
        protected abstract IEnumerable<ABaseDbRoutine> GetFunctions(TDatabase db, TDatabaseContext context);

        /// <summary>
        /// Get the database store procedure
        /// </summary>
        /// <param name="db">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of store procedures</returns>
        protected abstract IEnumerable<ABaseDbRoutine> GetStoreProcedures(TDatabase db, TDatabaseContext context);
    }
}
