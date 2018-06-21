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
    /// <typeparam name="TTable">Concrete type of the database table</typeparam>
    /// <typeparam name="TColumn">Concrete type of the database column</typeparam>
    /// <typeparam name="TPrimaryKey">Concrete type of the database primary key</typeparam>
    /// <typeparam name="TForeignKey">Concrete type of the database foreign key</typeparam>
    /// <typeparam name="TView">Concrete type of the database view</typeparam>
    public abstract class ADatabaseProvider<TDatabaseProviderOptions, TDatabaseContext, TDatabase, TTable, TColumn, TPrimaryKey, TForeignKey, TView> : IDatabaseProvider
        where TDatabaseProviderOptions : ADatabaseProviderOptions
        where TDatabaseContext : ADatabaseContext<TDatabaseProviderOptions>
        where TDatabase : ABaseDb, new()
        where TTable : ABaseDbTable, new()
        where TColumn : ABaseDbColumn, new()
        where TPrimaryKey : ABaseDbConstraint, new()
        where TForeignKey : ABaseDbConstraint, new()
        where TView : ABaseDbView, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ADatabaseProvider{TDatabaseProviderOptions, TDatabaseContext, TDatabase, TTable, TColumn, TPrimaryKey, TForeignKey, TView}"/> class.
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

            tables.ForEach(x =>
                    {
                        x.Columns.AddRange(
                            columns.Where(y => string.Equals(x.TableCatalog, y.TableCatalog, System.StringComparison.Ordinal)
                                                   && string.Equals(x.TableSchema, y.TableSchema, System.StringComparison.Ordinal)
                                                   && string.Equals(x.Name, y.TableName, System.StringComparison.Ordinal)));
                        x.ForeignKeys.AddRange(
                            foreignKeys.Where(y => string.Equals(x.TableCatalog, y.TableCatalog, System.StringComparison.Ordinal)
                                                   && string.Equals(x.TableSchema, y.TableSchema, System.StringComparison.Ordinal)
                                                   && string.Equals(x.Name, y.TableName, System.StringComparison.Ordinal)));
                        x.PrimaryKeys.AddRange(
                            primaryKeys.Where(y => string.Equals(x.TableCatalog, y.TableCatalog, System.StringComparison.Ordinal)
                                                   && string.Equals(x.TableSchema, y.TableSchema, System.StringComparison.Ordinal)
                                                   && string.Equals(x.Name, y.TableName, System.StringComparison.Ordinal)));
                    });

            db.Tables.AddRange(tables);
            db.Views.AddRange(views);

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
        /// <param name="database">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of columns</returns>
        protected abstract List<TColumn> GetColumns(TDatabase database, TDatabaseContext context);

        /// <summary>
        /// Get the table primary keys
        /// </summary>
        /// <param name="database">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of primary keys</returns>
        protected abstract List<TPrimaryKey> GetPrimaryKeys(TDatabase database, TDatabaseContext context);

        /// <summary>
        /// Get the table foreign keys
        /// </summary>
        /// <param name="database">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of foreign keys</returns>
        protected abstract List<TForeignKey> GetForeignKeys(TDatabase database, TDatabaseContext context);

        /// <summary>
        /// Get the database views
        /// </summary>
        /// <param name="db">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of views</returns>
        protected abstract List<TView> GetViews(TDatabase db, TDatabaseContext context);
    }
}
