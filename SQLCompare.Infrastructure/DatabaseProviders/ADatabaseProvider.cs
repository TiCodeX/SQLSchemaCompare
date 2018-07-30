using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Interfaces;
using SQLCompare.Infrastructure.EntityFramework;

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

            var tables = this.GetTables(db, context).ToList();
            var columns = this.GetColumns(db, context).ToList();
            var foreignKeys = this.GetForeignKeys(db, context).ToList();
            var indexes = this.GetIndexes(db, context).ToList();
            var constraints = this.GetConstraints(db, context).ToList();

            foreach (var table in tables)
            {
                table.Columns.AddRange(
                    columns.Where(y => table.Catalog == y.Catalog
                                           && table.Schema == y.Schema
                                           && table.Name == y.TableName));
                table.ForeignKeys.AddRange(
                    foreignKeys.Where(y => table.Catalog == y.TableCatalog
                                           && table.Schema == y.TableSchema
                                           && table.Name == y.TableName));
                table.PrimaryKeys.AddRange(
                    indexes.Where(y => y.IsPrimaryKey == true
                                           && table.Catalog == y.TableCatalog
                                           && table.Schema == y.TableSchema
                                           && table.Name == y.TableName));
                table.Indexes.AddRange(
                    indexes.Where(y => y.IsPrimaryKey == false
                                       && table.Catalog == y.TableCatalog
                                       && table.Schema == y.TableSchema
                                       && table.Name == y.TableName));
                table.Constraints.AddRange(
                    constraints.Where(y => table.Catalog == y.TableCatalog
                                       && table.Schema == y.TableSchema
                                       && table.Name == y.TableName));
            }

            db.Tables.AddRange(tables);
            db.Views.AddRange(this.GetViews(db, context));
            db.Functions.AddRange(this.GetFunctions(db, context));
            db.StoredProcedures.AddRange(this.GetStoredProcedures(db, context));
            db.DataTypes.AddRange(this.GetDataTypes(db, context));

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
        /// Get the table foreign keys
        /// </summary>
        /// <param name="database">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of foreign keys</returns>
        protected abstract IEnumerable<ABaseDbConstraint> GetForeignKeys(TDatabase database, TDatabaseContext context);

        /// <summary>
        /// Get the table constraints
        /// </summary>
        /// <param name="database">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of constraints</returns>
        protected abstract IEnumerable<ABaseDbConstraint> GetConstraints(TDatabase database, TDatabaseContext context);

        /// <summary>
        /// Get the table indexes
        /// </summary>
        /// <param name="database">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of indexes</returns>
        protected abstract IEnumerable<ABaseDbIndex> GetIndexes(TDatabase database, TDatabaseContext context);

        /// <summary>
        /// Get the database views
        /// </summary>
        /// <param name="database">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of views</returns>
        protected abstract IEnumerable<ABaseDbView> GetViews(TDatabase database, TDatabaseContext context);

        /// <summary>
        /// Get the database functions
        /// </summary>
        /// <param name="database">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of functions</returns>
        protected abstract IEnumerable<ABaseDbRoutine> GetFunctions(TDatabase database, TDatabaseContext context);

        /// <summary>
        /// Get the database stored procedures
        /// </summary>
        /// <param name="database">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of stored procedures</returns>
        protected abstract IEnumerable<ABaseDbRoutine> GetStoredProcedures(TDatabase database, TDatabaseContext context);

        /// <summary>
        /// Get the database data types
        /// </summary>
        /// <param name="database">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of data types</returns>
        protected abstract IEnumerable<ABaseDbObject> GetDataTypes(TDatabase database, TDatabaseContext context);
    }
}
