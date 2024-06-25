namespace TiCodeX.SQLSchemaCompare.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Retrieves common information from a Server
    /// </summary>
    /// <typeparam name="TDatabaseProviderOptions">Concrete type of the database provider options</typeparam>
    /// <typeparam name="TDatabaseContext">Concrete type of the database context</typeparam>
    /// <typeparam name="TDatabase">Concrete type of the database</typeparam>
    [SuppressMessage("Major Code Smell", "S2436:Types and methods should not have too many generic parameters", Justification = "Necessary")]
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
        /// <param name="cipherService">The injected cipher service</param>
        /// <param name="options">The options to connect to the Database</param>
        protected ADatabaseProvider(ILoggerFactory loggerFactory, ILogger logger, ICipherService cipherService, TDatabaseProviderOptions options)
        {
            this.Options = options;
            this.LoggerFactory = loggerFactory;
            this.Logger = logger;
            this.CipherService = cipherService;
        }

        /// <summary>
        /// Gets or sets the current server version
        /// </summary>
        public Version CurrentServerVersion { get; set; } = new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);

        /// <summary>
        /// Gets the injected logger
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the injected logger factory
        /// </summary>
        protected ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Gets the cipher service
        /// </summary>
        protected ICipherService CipherService { get; }

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
        [SuppressMessage("Major Code Smell", "S138:Functions should not have too many lines of code", Justification = "TODO")]
        [SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "TODO")]
        protected TDatabase DiscoverDatabase(TDatabaseContext context, TaskInfo taskInfo)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (taskInfo == null)
            {
                throw new ArgumentNullException(nameof(taskInfo));
            }

            this.Logger.LogInformation($"DiscoverDatabase started for database '{context.DatabaseName}' on server '{context.Hostname}'");
            var db = new TDatabase { Name = context.DatabaseName };

            var columns = new List<ABaseDbColumn>();

            var exceptions = new List<Exception>();

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                taskInfo.Percentage = 4;
                taskInfo.Message = Localization.StatusConnecting;
                context.Database.OpenConnection();
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error connecting to database");
                throw;
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                taskInfo.Percentage = 8;
                var version = this.GetServerVersion(context);
                this.Logger.LogInformation($"Server '{context.Hostname}' version: {version}");
                if (Version.TryParse(version, out var result))
                {
                    this.CurrentServerVersion = db.ServerVersion = result;
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving server version");
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                taskInfo.Percentage = 12;
                taskInfo.Message = Localization.StatusRetrievingSchemas;
                db.Schemas.AddRange(this.GetSchemas(context));
                db.Schemas.ForEach(x => x.Database = db);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving schemas");
                exceptions.Add(ex);
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                taskInfo.Percentage = 16;
                taskInfo.Message = Localization.StatusRetrievingTables;
                db.Tables.AddRange(this.GetTables(context));
                db.Tables.ForEach(x => x.Database = db);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving tables");
                exceptions.Add(ex);
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                taskInfo.Percentage = 24;
                columns.AddRange(this.GetColumns(context));
                columns.ForEach(x => x.Database = db);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving columns");
                exceptions.Add(ex);
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                taskInfo.Percentage = 28;
                taskInfo.Message = Localization.StatusRetrievingPrimaryKeys;

                foreach (var primaryKeyGroup in this.GetPrimaryKeys(context).GroupBy(x => new { x.TableSchema, x.TableName, x.Name }))
                {
                    var primaryKey = primaryKeyGroup.First();
                    primaryKey.Database = db;
                    primaryKey.ColumnNames.AddRange(primaryKeyGroup.OrderBy(x => x.OrdinalPosition).Select(x => x.ColumnName));
                    primaryKey.ColumnDescending.AddRange(primaryKeyGroup.OrderBy(x => x.OrdinalPosition).Select(x => x.IsDescending));
                    db.PrimaryKeys.Add(primaryKey);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving primary keys");
                exceptions.Add(ex);
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                taskInfo.Percentage = 32;
                taskInfo.Message = Localization.StatusRetrievingForeignKeys;

                foreach (var foreignKeyGroup in this.GetForeignKeys(context).GroupBy(x => new { x.TableSchema, x.TableName, x.Name }))
                {
                    var foreignKey = foreignKeyGroup.First();
                    foreignKey.Database = db;
                    foreignKey.ColumnNames.AddRange(foreignKeyGroup.OrderBy(x => x.OrdinalPosition).Select(x => x.ColumnName));
                    foreignKey.ReferencedColumnNames.AddRange(foreignKeyGroup.OrderBy(x => x.OrdinalPosition).Select(x => x.ReferencedColumnName));
                    db.ForeignKeys.Add(foreignKey);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving foreign keys");
                exceptions.Add(ex);
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                taskInfo.Percentage = 40;
                taskInfo.Message = Localization.StatusRetrievingIndexes;

                foreach (var indexGroup in this.GetIndexes(context).GroupBy(x => new { x.TableSchema, x.TableName, x.Name }))
                {
                    var index = indexGroup.First();
                    index.Database = db;
                    index.ColumnNames.AddRange(indexGroup.OrderBy(x => x.OrdinalPosition).Select(x => x.ColumnName));
                    index.ColumnDescending.AddRange(indexGroup.OrderBy(x => x.OrdinalPosition).Select(x => x.IsDescending));
                    db.Indexes.Add(index);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving indexes");
                exceptions.Add(ex);
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                taskInfo.Percentage = 48;
                taskInfo.Message = Localization.StatusRetrievingConstraints;
                foreach (var constraintGroup in this.GetConstraints(context).GroupBy(x => new { x.TableSchema, x.TableName, x.Name }))
                {
                    var constraint = constraintGroup.First();
                    constraint.Database = db;
                    constraint.ColumnNames.AddRange(constraintGroup.OrderBy(x => x.OrdinalPosition).Select(x => x.ColumnName));
                    db.Constraints.Add(constraint);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving constraints");
                exceptions.Add(ex);
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                taskInfo.Percentage = 56;
                taskInfo.Message = Localization.StatusRetrievingTriggers;
                db.Triggers.AddRange(this.GetTriggers(context).Where(_ => !string.IsNullOrWhiteSpace(_.Definition)));
                db.Triggers.ForEach(x =>
                {
                    x.Definition = x.Definition.TrimStart('\r', '\n');
                    x.Database = db;
                });
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving triggers");
                exceptions.Add(ex);
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            AssignRetrievedItemsToRelatedTables(taskInfo, db, columns);

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                taskInfo.Percentage = 64;
                taskInfo.Message = Localization.StatusRetrievingViews;
                db.Views.AddRange(this.GetViews(context).Where(_ => !string.IsNullOrWhiteSpace(_.ViewDefinition)));
                db.Views.ForEach(x =>
                {
                    x.ViewDefinition = x.ViewDefinition.TrimStart('\r', '\n');
                    x.Database = db;
                });
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving views");
                exceptions.Add(ex);
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            AssignRetrievedItemsToRelatedViews(taskInfo, db);

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                taskInfo.Percentage = 72;
                taskInfo.Message = Localization.StatusRetrievingFunctions;
                db.Functions.AddRange(this.GetFunctions(context).Where(_ => !string.IsNullOrWhiteSpace(_.Definition)));
                db.Functions.ForEach(x =>
                {
                    x.Definition = x.Definition.TrimStart('\r', '\n');
                    x.Database = db;
                });
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving functions");
                exceptions.Add(ex);
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                taskInfo.Percentage = 80;
                taskInfo.Message = Localization.StatusRetrievingStoredProcedures;
                db.StoredProcedures.AddRange(this.GetStoredProcedures(context).Where(_ => !string.IsNullOrWhiteSpace(_.Definition)));
                db.StoredProcedures.ForEach(x =>
                {
                    x.Definition = x.Definition.TrimStart('\r', '\n');
                    x.Database = db;
                });
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving stored procedures");
                exceptions.Add(ex);
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                taskInfo.Percentage = 88;
                taskInfo.Message = Localization.StatusRetrievingDataTypes;
                db.DataTypes.AddRange(this.GetDataTypes(context));
                db.DataTypes.ForEach(x => x.Database = db);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error retrieving data types");
                exceptions.Add(ex);
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                taskInfo.Percentage = 96;
                taskInfo.Message = Localization.StatusRetrievingSequences;
                db.Sequences.AddRange(this.GetSequences(context));
                db.Sequences.ForEach(x => x.Database = db);
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
        /// Get the SQL server version
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>The version string</returns>
        protected abstract string GetServerVersion(TDatabaseContext context);

        /// <summary>
        /// Get the database schemas
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>The list of schemas</returns>
        protected abstract IEnumerable<ABaseDbSchema> GetSchemas(TDatabaseContext context);

        /// <summary>
        /// Get the database tables
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>The list of tables</returns>
        protected abstract IEnumerable<ABaseDbTable> GetTables(TDatabaseContext context);

        /// <summary>
        /// Get the table columns
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>The list of columns</returns>
        protected abstract IEnumerable<ABaseDbColumn> GetColumns(TDatabaseContext context);

        /// <summary>
        /// Get the table primary keys
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>The list of primary keys</returns>
        protected abstract IEnumerable<ABaseDbPrimaryKey> GetPrimaryKeys(TDatabaseContext context);

        /// <summary>
        /// Get the table foreign keys
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>The list of foreign keys</returns>
        protected abstract IEnumerable<ABaseDbForeignKey> GetForeignKeys(TDatabaseContext context);

        /// <summary>
        /// Get the table constraints
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>The list of constraints</returns>
        protected abstract IEnumerable<ABaseDbConstraint> GetConstraints(TDatabaseContext context);

        /// <summary>
        /// Get the table indexes
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>The list of indexes</returns>
        protected abstract IEnumerable<ABaseDbIndex> GetIndexes(TDatabaseContext context);

        /// <summary>
        /// Get the database views
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>The list of views</returns>
        protected abstract IEnumerable<ABaseDbView> GetViews(TDatabaseContext context);

        /// <summary>
        /// Get the database functions
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>The list of functions</returns>
        protected abstract IEnumerable<ABaseDbFunction> GetFunctions(TDatabaseContext context);

        /// <summary>
        /// Get the database stored procedures
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>The list of stored procedures</returns>
        protected abstract IEnumerable<ABaseDbStoredProcedure> GetStoredProcedures(TDatabaseContext context);

        /// <summary>
        /// Get the database triggers
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>The list of triggers</returns>
        protected abstract IEnumerable<ABaseDbTrigger> GetTriggers(TDatabaseContext context);

        /// <summary>
        /// Get the database data types
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>The list of data types</returns>
        protected abstract IEnumerable<ABaseDbDataType> GetDataTypes(TDatabaseContext context);

        /// <summary>
        /// Get the database sequences
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>The list of sequences</returns>
        protected abstract IEnumerable<ABaseDbSequence> GetSequences(TDatabaseContext context);

        /// <summary>
        /// Assign the retrieved items to the related tables
        /// </summary>
        /// <param name="taskInfo">The task info</param>
        /// <param name="db">The database</param>
        /// <param name="columns">The columns</param>
        private static void AssignRetrievedItemsToRelatedTables(TaskInfo taskInfo, TDatabase db, IReadOnlyCollection<ABaseDbColumn> columns)
        {
            foreach (var table in db.Tables)
            {
                taskInfo.CancellationToken.ThrowIfCancellationRequested();

                table.Columns.AddRange(columns.Where(y => table.Schema == y.Schema && table.Name == y.TableName));
                table.ForeignKeys.AddRange(db.ForeignKeys.Where(y => table.Schema == y.TableSchema && table.Name == y.TableName));
                table.ReferencingForeignKeys.AddRange(db.ForeignKeys.Where(y => table.Schema == y.ReferencedTableSchema && table.Name == y.ReferencedTableName));
                table.PrimaryKeys.AddRange(db.PrimaryKeys.Where(y => table.Schema == y.TableSchema && table.Name == y.TableName));
                table.Indexes.AddRange(db.Indexes.Where(y => table.Schema == y.TableSchema && table.Name == y.TableName));
                table.Constraints.AddRange(db.Constraints.Where(y => table.Schema == y.TableSchema && table.Name == y.TableName));
                table.Triggers.AddRange(db.Triggers.Where(y => table.Schema == y.TableSchema && table.Name == y.TableName));
            }
        }

        /// <summary>
        /// Assign the retrieved items to the related views
        /// </summary>
        /// <param name="taskInfo">The task info</param>
        /// <param name="db">The database</param>
        private static void AssignRetrievedItemsToRelatedViews(TaskInfo taskInfo, TDatabase db)
        {
            foreach (var view in db.Views)
            {
                taskInfo.CancellationToken.ThrowIfCancellationRequested();

                view.Indexes.AddRange(db.Indexes.Where(y => view.Schema == y.TableSchema && view.Name == y.TableName));
            }
        }
    }
}
