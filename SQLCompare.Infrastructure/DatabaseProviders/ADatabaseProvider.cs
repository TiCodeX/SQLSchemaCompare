using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Interfaces;
using SQLCompare.Infrastructure.EntityFramework;
using SQLCompare.Services;

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
        public abstract ABaseDb GetDatabase(TaskInfo taskInfo);

        /// <summary>
        /// Discover the complete database structure
        /// </summary>
        /// <param name="context">The database context</param>
        /// <param name="taskInfo">The task info for async operations</param>
        /// <returns>The discovered database structure</returns>
        protected TDatabase DiscoverDatabase(TDatabaseContext context, TaskInfo taskInfo)
        {
            this.Logger.LogInformation($"DiscoverDatabase started for database '{context.DatabaseName}'");
            var db = new TDatabase { Name = context.DatabaseName };

            var columns = new List<ABaseDbColumn>();
            var foreignKeys = new List<ABaseDbConstraint>();
            var indexes = new List<ABaseDbIndex>();
            var constraints = new List<ABaseDbConstraint>();

            var exceptions = new List<Exception>();

            try
            {
                taskInfo.Percentage = 8;
                taskInfo.Message = Localization.StatusConnecting;
                context.Database.OpenConnection();
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error connecting to database");
                throw;
            }

            try
            {
                taskInfo.Percentage = 16;
                taskInfo.Message = Localization.StatusRetrievingTables;
                db.Tables.AddRange(this.GetTables(db, context));
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving tables");
                exceptions.Add(ex);
            }

            try
            {
                taskInfo.Percentage = 24;
                columns.AddRange(this.GetColumns(db, context));
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving columns");
                exceptions.Add(ex);
            }

            try
            {
                taskInfo.Percentage = 32;
                taskInfo.Message = Localization.StatusRetrievingForeignKeys;
                foreignKeys.AddRange(this.GetForeignKeys(db, context));
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving foreign keys");
                exceptions.Add(ex);
            }

            try
            {
                taskInfo.Percentage = 40;
                taskInfo.Message = Localization.StatusRetrievingIndexes;
                indexes.AddRange(this.GetIndexes(db, context));
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving indexes");
                exceptions.Add(ex);
            }

            try
            {
                taskInfo.Percentage = 48;
                taskInfo.Message = Localization.StatusRetrievingConstraints;
                constraints.AddRange(this.GetConstraints(db, context));
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving constraints");
                exceptions.Add(ex);
            }

            // Assign the retrieved items to the related tables
            foreach (var table in db.Tables)
            {
                table.Columns.AddRange(
                    columns.Where(y => table.Database == y.Database
                                           && table.Schema == y.Schema
                                           && table.Name == y.TableName));
                table.ForeignKeys.AddRange(
                    foreignKeys.Where(y => table.Database == y.TableDatabase
                                           && table.Schema == y.TableSchema
                                           && table.Name == y.TableName));
                table.PrimaryKeys.AddRange(
                    indexes.Where(y => y.IsPrimaryKey
                                           && table.Database == y.TableDatabase
                                           && table.Schema == y.TableSchema
                                           && table.Name == y.TableName));
                table.Indexes.AddRange(
                    indexes.Where(y => y.IsPrimaryKey == false
                                       && table.Database == y.TableDatabase
                                       && table.Schema == y.TableSchema
                                       && table.Name == y.TableName));
                table.Constraints.AddRange(
                    constraints.Where(y => table.Database == y.TableDatabase
                                       && table.Schema == y.TableSchema
                                       && table.Name == y.TableName));
            }

            try
            {
                taskInfo.Percentage = 56;
                taskInfo.Message = Localization.StatusRetrievingViews;
                db.Views.AddRange(this.GetViews(db, context));
                db.Views.ForEach(x => { x.ViewDefinition = x.ViewDefinition.TrimStart('\r', '\n'); });
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving views");
                exceptions.Add(ex);
            }

            try
            {
                taskInfo.Percentage = 64;
                taskInfo.Message = Localization.StatusRetrievingFunctions;
                db.Functions.AddRange(this.GetFunctions(db, context));
                db.Functions.ForEach(x => { x.Definition = x.Definition.TrimStart('\r', '\n'); });
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving functions");
                exceptions.Add(ex);
            }

            try
            {
                taskInfo.Percentage = 72;
                taskInfo.Message = Localization.StatusRetrievingStoredProcedures;
                db.StoredProcedures.AddRange(this.GetStoredProcedures(db, context));
                db.StoredProcedures.ForEach(x => { x.Definition = x.Definition.TrimStart('\r', '\n'); });
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving stored procedures");
                exceptions.Add(ex);
            }

            try
            {
                taskInfo.Percentage = 80;
                taskInfo.Message = Localization.StatusRetrievingTriggers;
                db.Triggers.AddRange(this.GetTriggers(db, context));
                db.Triggers.ForEach(x => { x.Definition = x.Definition.TrimStart('\r', '\n'); });
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving triggers");
                exceptions.Add(ex);
            }

            try
            {
                taskInfo.Percentage = 88;
                taskInfo.Message = Localization.StatusRetrievingDataTypes;
                db.DataTypes.AddRange(this.GetDataTypes(db, context));
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving data types");
                exceptions.Add(ex);
            }

            try
            {
                taskInfo.Percentage = 96;
                taskInfo.Message = Localization.StatusRetrievingSequences;
                db.Sequences.AddRange(this.GetSequences(db, context));
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving sequences");
                exceptions.Add(ex);
            }

            taskInfo.Percentage = 100;
            taskInfo.Message = Localization.StatusDone;

            if (exceptions.Count > 0)
            {
                taskInfo.Message = Localization.StatusError;
                throw new AggregateException(exceptions);
            }

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
        protected abstract IEnumerable<ABaseDbFunction> GetFunctions(TDatabase database, TDatabaseContext context);

        /// <summary>
        /// Get the database stored procedures
        /// </summary>
        /// <param name="database">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of stored procedures</returns>
        protected abstract IEnumerable<ABaseDbStoredProcedure> GetStoredProcedures(TDatabase database, TDatabaseContext context);

        /// <summary>
        /// Get the database triggers
        /// </summary>
        /// <param name="database">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of triggers</returns>
        protected abstract IEnumerable<ABaseDbTrigger> GetTriggers(TDatabase database, TDatabaseContext context);

        /// <summary>
        /// Get the database data types
        /// </summary>
        /// <param name="database">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of data types</returns>
        protected abstract IEnumerable<ABaseDbDataType> GetDataTypes(TDatabase database, TDatabaseContext context);

        /// <summary>
        /// Get the database sequences
        /// </summary>
        /// <param name="database">The database information</param>
        /// <param name="context">The database context</param>
        /// <returns>The list of sequences</returns>
        protected abstract IEnumerable<ABaseDbSequence> GetSequences(TDatabase database, TDatabaseContext context);
    }
}
