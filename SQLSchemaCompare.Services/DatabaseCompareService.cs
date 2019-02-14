using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Logging;
using TiCodeX.SQLSchemaCompare.Core.Entities;
using TiCodeX.SQLSchemaCompare.Core.Entities.Compare;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database.MicrosoftSql;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database.MySql;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database.PostgreSql;
using TiCodeX.SQLSchemaCompare.Core.Enums;
using TiCodeX.SQLSchemaCompare.Core.Interfaces;
using TiCodeX.SQLSchemaCompare.Core.Interfaces.Services;

namespace TiCodeX.SQLSchemaCompare.Services
{
    /// <summary>
    /// Implementation that provides the mechanisms to compare two database instances
    /// </summary>
    public class DatabaseCompareService : IDatabaseCompareService
    {
        private readonly ILogger logger;
        private readonly IProjectService projectService;
        private readonly IDatabaseService databaseService;
        private readonly IDatabaseScripterFactory databaseScripterFactory;
        private readonly IDatabaseMapper databaseMapper;
        private readonly IDatabaseFilter databaseFilter;
        private readonly ITaskService taskService;

        private readonly List<CompareResultItem<ABaseDbSchema>> schemas = new List<CompareResultItem<ABaseDbSchema>>();
        private readonly List<CompareResultItem<ABaseDbTable>> tables = new List<CompareResultItem<ABaseDbTable>>();
        private readonly List<CompareResultItem<ABaseDbIndex>> indexes = new List<CompareResultItem<ABaseDbIndex>>();
        private readonly List<CompareResultItem<ABaseDbConstraint>> constraints = new List<CompareResultItem<ABaseDbConstraint>>();
        private readonly List<CompareResultItem<ABaseDbPrimaryKey>> primaryKeys = new List<CompareResultItem<ABaseDbPrimaryKey>>();
        private readonly List<CompareResultItem<ABaseDbForeignKey>> foreignKeys = new List<CompareResultItem<ABaseDbForeignKey>>();
        private readonly List<CompareResultItem<ABaseDbTrigger>> triggers = new List<CompareResultItem<ABaseDbTrigger>>();
        private readonly List<CompareResultItem<ABaseDbView>> views = new List<CompareResultItem<ABaseDbView>>();
        private readonly List<CompareResultItem<ABaseDbFunction>> functions = new List<CompareResultItem<ABaseDbFunction>>();
        private readonly List<CompareResultItem<ABaseDbStoredProcedure>> storedProcedures = new List<CompareResultItem<ABaseDbStoredProcedure>>();
        private readonly List<CompareResultItem<ABaseDbSequence>> sequences = new List<CompareResultItem<ABaseDbSequence>>();
        private readonly List<CompareResultItem<ABaseDbDataType>> dataTypes = new List<CompareResultItem<ABaseDbDataType>>();

        private ABaseDb retrievedSourceDatabase;
        private ABaseDb retrievedTargetDatabase;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseCompareService"/> class
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="projectService">The injected project service</param>
        /// <param name="databaseService">The injected database service</param>
        /// <param name="databaseScripterFactory">The injected database scripter factory</param>
        /// <param name="databaseMapper">The injected database mapper</param>
        /// <param name="databaseFilter">The injected database filter</param>
        /// <param name="taskService">The injected task service</param>
        public DatabaseCompareService(
            ILoggerFactory loggerFactory,
            IProjectService projectService,
            IDatabaseService databaseService,
            IDatabaseScripterFactory databaseScripterFactory,
            IDatabaseMapper databaseMapper,
            IDatabaseFilter databaseFilter,
            ITaskService taskService)
        {
            this.logger = loggerFactory.CreateLogger(nameof(DatabaseCompareService));
            this.projectService = projectService;
            this.databaseService = databaseService;
            this.databaseScripterFactory = databaseScripterFactory;
            this.databaseMapper = databaseMapper;
            this.databaseFilter = databaseFilter;
            this.taskService = taskService;
        }

        /// <inheritdoc />
        public void StartCompare()
        {
            this.taskService.ExecuteTasks(new List<TaskWork>
            {
                new TaskWork(
                    new TaskInfo(Localization.LabelRetrieveSourceDatabase),
                    true,
                    taskInfo =>
                    {
                        this.retrievedSourceDatabase = this.databaseService.GetDatabase(
                            this.projectService.Project.SourceProviderOptions, taskInfo);
                        this.retrievedSourceDatabase.Direction = CompareDirection.Source;
                        this.databaseFilter.PerformFilter(this.retrievedSourceDatabase, this.projectService.Project.Options.Filtering);
                        return true;
                    }),
                new TaskWork(
                    new TaskInfo(Localization.LabelRetrieveTargetDatabase),
                    true,
                    taskInfo =>
                    {
                        this.retrievedTargetDatabase = this.databaseService.GetDatabase(
                            this.projectService.Project.TargetProviderOptions, taskInfo);
                        this.retrievedTargetDatabase.Direction = CompareDirection.Target;
                        this.databaseFilter.PerformFilter(this.retrievedTargetDatabase, this.projectService.Project.Options.Filtering);
                        return true;
                    }),
                new TaskWork(
                    new TaskInfo(Localization.LabelMappingDatabaseObjects),
                    false,
                    taskInfo =>
                    {
                        this.databaseMapper.PerformMapping(this.retrievedSourceDatabase, this.retrievedTargetDatabase, null, taskInfo);
                        return true;
                    }),
                new TaskWork(
                    new TaskInfo(Localization.LabelDatabaseComparison),
                    false,
                    this.ExecuteDatabaseComparison)
            });
        }

        private static void SetCompareResultList<T>(ICollection<CompareResultItem<T>> resultList, IEnumerable<T> source, IEnumerable<T> target, IDatabaseScripter scripter)
            where T : ABaseDbObject
        {
            foreach (var item in source)
            {
                resultList.Add(new CompareResultItem<T>
                {
                    SourceItem = item,
                    TargetItem = item.MappedDbObject as T,
                    SourceItemName = scripter.GenerateObjectName(item),
                    TargetItemName = item.MappedDbObject != null ? scripter.GenerateObjectName(item.MappedDbObject) : string.Empty,
                    Scripts = new CompareResultItemScripts
                    {
                        SourceCreateScript = item.CreateScript,
                        TargetCreateScript = item.MappedDbObject != null ? item.MappedDbObject.CreateScript : string.Empty,
                        AlterScript = item.AlterScript
                    }
                });
            }

            foreach (var item in target.Where(x => x.MappedDbObject == null).ToList())
            {
                resultList.Add(new CompareResultItem<T>
                {
                    TargetItem = item,
                    TargetItemName = scripter.GenerateObjectName(item),
                    Scripts = new CompareResultItemScripts
                    {
                        TargetCreateScript = item.CreateScript,
                        AlterScript = item.AlterScript
                    }
                });
            }
        }

        private static void CompareTable(ABaseDbTable table, IDatabaseScripter scripter)
        {
            // Linearize the 2 databases for mapping
            var dbObjects = new List<ABaseDbObject>();
            dbObjects.AddRange(table.Columns);
            dbObjects.AddRange(table.Indexes);
            dbObjects.AddRange(table.Constraints);
            dbObjects.AddRange(table.PrimaryKeys);
            dbObjects.AddRange(table.ForeignKeys);
            dbObjects.AddRange(table.Triggers);

            foreach (var item in dbObjects)
            {
                item.CreateScript = scripter.GenerateCreateScript(item, true);
                if (item.MappedDbObject != null)
                {
                    item.MappedDbObject.CreateScript = scripter.GenerateCreateScript(item.MappedDbObject, true);
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "TODO")]
        private bool ExecuteDatabaseComparison(TaskInfo taskInfo)
        {
            var processedItems = 1;
            var scripter = this.databaseScripterFactory.Create(
                this.retrievedSourceDatabase,
                this.projectService.Project.Options);

            // Linearize the 2 databases into a single list of of all source items and the target items not present in source
            var maps = new List<ObjectMap>
            {
                new ObjectMap { ObjectTitle = Localization.StatusComparingSchemas, DbObjects = this.retrievedSourceDatabase.Schemas.Concat(this.retrievedTargetDatabase.Schemas.Where(x => x.MappedDbObject == null)) },
                new ObjectMap { ObjectTitle = Localization.StatusComparingTables, DbObjects = this.retrievedSourceDatabase.Tables.Concat(this.retrievedTargetDatabase.Tables.Where(x => x.MappedDbObject == null)) },
                new ObjectMap { ObjectTitle = Localization.StatusComparingViews, DbObjects = this.retrievedSourceDatabase.Views.Concat(this.retrievedTargetDatabase.Views.Where(x => x.MappedDbObject == null)) },
                new ObjectMap { ObjectTitle = Localization.StatusComparingFunctions, DbObjects = this.retrievedSourceDatabase.Functions.Concat(this.retrievedTargetDatabase.Functions.Where(x => x.MappedDbObject == null)) },
                new ObjectMap { ObjectTitle = Localization.StatusComparingStoredProcedures, DbObjects = this.retrievedSourceDatabase.StoredProcedures.Concat(this.retrievedTargetDatabase.StoredProcedures.Where(x => x.MappedDbObject == null)) },
                new ObjectMap { ObjectTitle = Localization.StatusComparingDataTypes, DbObjects = this.retrievedSourceDatabase.DataTypes.Where(x => x.IsUserDefined).Concat(this.retrievedTargetDatabase.DataTypes.Where(x => x.MappedDbObject == null && x.IsUserDefined)) },
                new ObjectMap { ObjectTitle = Localization.StatusComparingSequences, DbObjects = this.retrievedSourceDatabase.Sequences.Concat(this.retrievedTargetDatabase.Sequences.Where(x => x.MappedDbObject == null)) },
            };

            var totalItems = maps.SelectMany(x => x.DbObjects).Count();
            if (totalItems == 0)
            {
                throw new DataException(Localization.ErrorEmptyDatabases);
            }

            foreach (var m in maps)
            {
                taskInfo.Message = m.ObjectTitle;
                foreach (var item in m.DbObjects)
                {
                    if (item is ABaseDbTable table)
                    {
                        CompareTable(table, scripter);
                    }

                    item.CreateScript = scripter.GenerateCreateScript(item, true);
                    if (item.MappedDbObject != null)
                    {
                        item.MappedDbObject.CreateScript = scripter.GenerateCreateScript(item.MappedDbObject, true);

                        if (item.MappedDbObject is ABaseDbTable mappedTable)
                        {
                            CompareTable(mappedTable, scripter);
                        }
                    }

                    item.AlterScript = scripter.GenerateAlterScript(item, true);

                    taskInfo.Percentage = (short)(100 * processedItems++ / totalItems);
                }
            }

            SetCompareResultList(this.schemas, this.retrievedSourceDatabase.Schemas, this.retrievedTargetDatabase.Schemas, scripter);
            SetCompareResultList(this.tables, this.retrievedSourceDatabase.Tables, this.retrievedTargetDatabase.Tables, scripter);
            SetCompareResultList(this.indexes, this.retrievedSourceDatabase.Indexes, this.retrievedTargetDatabase.Indexes, scripter);
            SetCompareResultList(this.constraints, this.retrievedSourceDatabase.Constraints, this.retrievedTargetDatabase.Constraints, scripter);
            SetCompareResultList(this.primaryKeys, this.retrievedSourceDatabase.PrimaryKeys, this.retrievedTargetDatabase.PrimaryKeys, scripter);
            SetCompareResultList(this.foreignKeys, this.retrievedSourceDatabase.ForeignKeys, this.retrievedTargetDatabase.ForeignKeys, scripter);
            SetCompareResultList(this.views, this.retrievedSourceDatabase.Views, this.retrievedTargetDatabase.Views, scripter);
            SetCompareResultList(this.functions, this.retrievedSourceDatabase.Functions, this.retrievedTargetDatabase.Functions, scripter);
            SetCompareResultList(this.storedProcedures, this.retrievedSourceDatabase.StoredProcedures, this.retrievedTargetDatabase.StoredProcedures, scripter);
            SetCompareResultList(this.sequences, this.retrievedSourceDatabase.Sequences, this.retrievedTargetDatabase.Sequences, scripter);
            SetCompareResultList(this.dataTypes, this.retrievedSourceDatabase.DataTypes.Where(x => x.IsUserDefined), this.retrievedTargetDatabase.DataTypes.Where(x => x.IsUserDefined), scripter);
            SetCompareResultList(this.triggers, this.retrievedSourceDatabase.Triggers, this.retrievedTargetDatabase.Triggers, scripter);

            var result = new CompareResult();

            this.FillCompareResultItems(result);

            result.SourceFullScript = scripter.GenerateFullCreateScript(this.retrievedSourceDatabase);
            result.TargetFullScript = scripter.GenerateFullCreateScript(this.retrievedTargetDatabase);
            result.FullAlterScript = this.GenerateFullAlterScript(scripter);

            this.projectService.Project.Result = result;

            return true;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "TODO")]
        private void FillCompareResultItems(CompareResult result)
        {
            result.DifferentItems.AddRange(this.schemas.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            result.DifferentItems.AddRange(this.tables.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            result.DifferentItems.AddRange(this.views.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            result.DifferentItems.AddRange(this.functions.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            result.DifferentItems.AddRange(this.storedProcedures.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            result.DifferentItems.AddRange(this.sequences.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            result.DifferentItems.AddRange(this.dataTypes.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            this.logger.LogDebug($"Different items => {result.DifferentItems.Count}");

            result.OnlySourceItems.AddRange(this.schemas.Where(x => x.SourceItem != null && x.TargetItem == null).OrderBy(x => x.SourceItemName));
            result.OnlySourceItems.AddRange(this.tables.Where(x => x.SourceItem != null && x.TargetItem == null).OrderBy(x => x.SourceItemName));
            result.OnlySourceItems.AddRange(this.views.Where(x => x.SourceItem != null && x.TargetItem == null).OrderBy(x => x.SourceItemName));
            result.OnlySourceItems.AddRange(this.functions.Where(x => x.SourceItem != null && x.TargetItem == null).OrderBy(x => x.SourceItemName));
            result.OnlySourceItems.AddRange(this.storedProcedures.Where(x => x.SourceItem != null && x.TargetItem == null).OrderBy(x => x.SourceItemName));
            result.OnlySourceItems.AddRange(this.sequences.Where(x => x.SourceItem != null && x.TargetItem == null).OrderBy(x => x.SourceItemName));
            result.OnlySourceItems.AddRange(this.dataTypes.Where(x => x.SourceItem != null && x.TargetItem == null).OrderBy(x => x.SourceItemName));
            this.logger.LogDebug($"Only Source items => {result.OnlySourceItems.Count}");

            result.OnlyTargetItems.AddRange(this.schemas.Where(x => x.TargetItem != null && x.SourceItem == null).OrderBy(x => x.TargetItemName));
            result.OnlyTargetItems.AddRange(this.tables.Where(x => x.TargetItem != null && x.SourceItem == null).OrderBy(x => x.TargetItemName));
            result.OnlyTargetItems.AddRange(this.views.Where(x => x.TargetItem != null && x.SourceItem == null).OrderBy(x => x.TargetItemName));
            result.OnlyTargetItems.AddRange(this.functions.Where(x => x.TargetItem != null && x.SourceItem == null).OrderBy(x => x.TargetItemName));
            result.OnlyTargetItems.AddRange(this.storedProcedures.Where(x => x.TargetItem != null && x.SourceItem == null).OrderBy(x => x.TargetItemName));
            result.OnlyTargetItems.AddRange(this.sequences.Where(x => x.TargetItem != null && x.SourceItem == null).OrderBy(x => x.TargetItemName));
            result.OnlyTargetItems.AddRange(this.dataTypes.Where(x => x.TargetItem != null && x.SourceItem == null).OrderBy(x => x.TargetItemName));
            this.logger.LogDebug($"Only Target items => {result.OnlyTargetItems.Count}");

            result.SameItems.AddRange(this.schemas.Where(x => x.SourceItem != null && x.TargetItem != null && x.Equal).OrderBy(x => x.SourceItemName));
            result.SameItems.AddRange(this.tables.Where(x => x.SourceItem != null && x.TargetItem != null && x.Equal).OrderBy(x => x.SourceItemName));
            result.SameItems.AddRange(this.views.Where(x => x.SourceItem != null && x.TargetItem != null && x.Equal).OrderBy(x => x.SourceItemName));
            result.SameItems.AddRange(this.functions.Where(x => x.SourceItem != null && x.TargetItem != null && x.Equal).OrderBy(x => x.SourceItemName));
            result.SameItems.AddRange(this.storedProcedures.Where(x => x.SourceItem != null && x.TargetItem != null && x.Equal).OrderBy(x => x.SourceItemName));
            result.SameItems.AddRange(this.sequences.Where(x => x.SourceItem != null && x.TargetItem != null && x.Equal).OrderBy(x => x.SourceItemName));
            result.SameItems.AddRange(this.dataTypes.Where(x => x.SourceItem != null && x.TargetItem != null && x.Equal).OrderBy(x => x.SourceItemName));
            this.logger.LogDebug($"Same items => {result.SameItems.Count}");
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "TODO")]
        private string GenerateFullAlterScript(IDatabaseScripter scripter)
        {
            // Add items related to tables directly in the different items list only for the alter script generation
            var differentItems = new List<ABaseCompareResultItem>();
            differentItems.AddRange(this.schemas.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal));
            differentItems.AddRange(this.tables.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal));
            differentItems.AddRange(this.views.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal));
            differentItems.AddRange(this.functions.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal));
            differentItems.AddRange(this.storedProcedures.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal));
            differentItems.AddRange(this.sequences.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal));
            differentItems.AddRange(this.dataTypes.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal));
            differentItems.AddRange(this.indexes.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal));
            differentItems.AddRange(this.constraints.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal));
            differentItems.AddRange(this.primaryKeys.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal));
            differentItems.AddRange(this.foreignKeys.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal));
            differentItems.AddRange(this.triggers.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal));

            ABaseDb onlySourceDb;
            ABaseDb onlyTargetDb;
            switch (this.retrievedSourceDatabase)
            {
                case MicrosoftSqlDb _:
                    onlySourceDb = new MicrosoftSqlDb();
                    onlyTargetDb = new MicrosoftSqlDb();
                    break;

                case MySqlDb _:
                    onlySourceDb = new MySqlDb();
                    onlyTargetDb = new MySqlDb();
                    break;

                case PostgreSqlDb _:
                    onlySourceDb = new PostgreSqlDb();
                    onlyTargetDb = new PostgreSqlDb();
                    break;

                default:
                    throw new NotImplementedException("Unknown Database Type");
            }

            onlySourceDb.Schemas.AddRange(this.schemas.Where(x => x.SourceItem != null && x.TargetItem == null).Select(x => x.SourceItem));
            onlySourceDb.Tables.AddRange(this.tables.Where(x => x.SourceItem != null && x.TargetItem == null).Select(x => x.SourceItem));
            onlySourceDb.Indexes.AddRange(this.indexes.Where(x => x.SourceItem != null && x.TargetItem == null).Select(x => x.SourceItem));
            onlySourceDb.Constraints.AddRange(this.constraints.Where(x => x.SourceItem != null && x.TargetItem == null).Select(x => x.SourceItem));
            onlySourceDb.PrimaryKeys.AddRange(this.primaryKeys.Where(x => x.SourceItem != null && x.TargetItem == null).Select(x => x.SourceItem));
            onlySourceDb.ForeignKeys.AddRange(this.foreignKeys.Where(x => x.SourceItem != null && x.TargetItem == null).Select(x => x.SourceItem));
            onlySourceDb.Triggers.AddRange(this.triggers.Where(x => x.SourceItem != null && x.TargetItem == null).Select(x => x.SourceItem));
            onlySourceDb.Views.AddRange(this.views.Where(x => x.SourceItem != null && x.TargetItem == null).Select(x => x.SourceItem));
            onlySourceDb.Functions.AddRange(this.functions.Where(x => x.SourceItem != null && x.TargetItem == null).Select(x => x.SourceItem));
            onlySourceDb.StoredProcedures.AddRange(this.storedProcedures.Where(x => x.SourceItem != null && x.TargetItem == null).Select(x => x.SourceItem));
            onlySourceDb.DataTypes.AddRange(this.retrievedSourceDatabase.DataTypes.Where(x => x.IsUserDefined == false));
            onlySourceDb.DataTypes.AddRange(this.dataTypes.Where(x => x.SourceItem != null && x.TargetItem == null).Select(x => x.SourceItem));
            onlySourceDb.Sequences.AddRange(this.sequences.Where(x => x.SourceItem != null && x.TargetItem == null).Select(x => x.SourceItem));

            onlyTargetDb.Schemas.AddRange(this.schemas.Where(x => x.SourceItem == null && x.TargetItem != null).Select(x => x.TargetItem));
            onlyTargetDb.Tables.AddRange(this.tables.Where(x => x.SourceItem == null && x.TargetItem != null).Select(x => x.TargetItem));
            onlyTargetDb.Indexes.AddRange(this.indexes.Where(x => x.SourceItem == null && x.TargetItem != null).Select(x => x.TargetItem));
            onlyTargetDb.Constraints.AddRange(this.constraints.Where(x => x.SourceItem == null && x.TargetItem != null).Select(x => x.TargetItem));
            onlyTargetDb.PrimaryKeys.AddRange(this.primaryKeys.Where(x => x.SourceItem == null && x.TargetItem != null).Select(x => x.TargetItem));
            onlyTargetDb.ForeignKeys.AddRange(this.foreignKeys.Where(x => x.SourceItem == null && x.TargetItem != null).Select(x => x.TargetItem));
            onlyTargetDb.Triggers.AddRange(this.triggers.Where(x => x.SourceItem == null && x.TargetItem != null).Select(x => x.TargetItem));
            onlyTargetDb.Views.AddRange(this.views.Where(x => x.SourceItem == null && x.TargetItem != null).Select(x => x.TargetItem));
            onlyTargetDb.Functions.AddRange(this.functions.Where(x => x.SourceItem == null && x.TargetItem != null).Select(x => x.TargetItem));
            onlyTargetDb.StoredProcedures.AddRange(this.storedProcedures.Where(x => x.SourceItem == null && x.TargetItem != null).Select(x => x.TargetItem));
            onlyTargetDb.DataTypes.AddRange(this.retrievedTargetDatabase.DataTypes.Where(x => x.IsUserDefined == false));
            onlyTargetDb.DataTypes.AddRange(this.dataTypes.Where(x => x.SourceItem == null && x.TargetItem != null).Select(x => x.TargetItem));
            onlyTargetDb.Sequences.AddRange(this.sequences.Where(x => x.SourceItem == null && x.TargetItem != null).Select(x => x.TargetItem));

            // Unsupported alter functionality: remove from different items and add the items to the onlySource/onlyTarget
            // so that they will be dropped at the beginning of the script and recreated at the end

            // Indexes
            var i = differentItems.OfType<CompareResultItem<ABaseDbIndex>>().Where(x => !x.SourceItem.AlterScriptSupported).ToList();
            onlySourceDb.Indexes.AddRange(i.Select(x => x.SourceItem));
            onlyTargetDb.Indexes.AddRange(i.Select(x => x.TargetItem));
            i.ForEach(x => differentItems.Remove(x));

            // Constraints
            var c = differentItems.OfType<CompareResultItem<ABaseDbConstraint>>().Where(x => !x.SourceItem.AlterScriptSupported).ToList();
            onlySourceDb.Constraints.AddRange(c.Select(x => x.SourceItem));
            onlyTargetDb.Constraints.AddRange(c.Select(x => x.TargetItem));
            c.ForEach(x => differentItems.Remove(x));

            // Primary Keys
            var pk = differentItems.OfType<CompareResultItem<ABaseDbPrimaryKey>>().Where(x => !x.SourceItem.AlterScriptSupported).ToList();
            onlySourceDb.PrimaryKeys.AddRange(pk.Select(x => x.SourceItem));
            onlyTargetDb.PrimaryKeys.AddRange(pk.Select(x => x.TargetItem));
            pk.ForEach(x => differentItems.Remove(x));

            // Foreign Keys
            var fk = differentItems.OfType<CompareResultItem<ABaseDbForeignKey>>().Where(x => !x.SourceItem.AlterScriptSupported).ToList();
            onlySourceDb.ForeignKeys.AddRange(fk.Select(x => x.SourceItem));
            onlyTargetDb.ForeignKeys.AddRange(fk.Select(x => x.TargetItem));
            fk.ForEach(x => differentItems.Remove(x));

            // Triggers
            var t = differentItems.OfType<CompareResultItem<ABaseDbTrigger>>().Where(x => !x.SourceItem.AlterScriptSupported).ToList();
            onlySourceDb.Triggers.AddRange(t.Select(x => x.SourceItem));
            onlyTargetDb.Triggers.AddRange(t.Select(x => x.TargetItem));
            t.ForEach(x => differentItems.Remove(x));

            // StoredProcedures
            var s = differentItems.OfType<CompareResultItem<ABaseDbStoredProcedure>>().Where(x => !x.SourceItem.AlterScriptSupported).ToList();
            onlySourceDb.StoredProcedures.AddRange(s.Select(x => x.SourceItem));
            onlyTargetDb.StoredProcedures.AddRange(s.Select(x => x.TargetItem));
            s.ForEach(x => differentItems.Remove(x));

            // Functions
            var f = differentItems.OfType<CompareResultItem<ABaseDbFunction>>().Where(x => !x.SourceItem.AlterScriptSupported).ToList();
            onlySourceDb.Functions.AddRange(f.Select(x => x.SourceItem));
            onlyTargetDb.Functions.AddRange(f.Select(x => x.TargetItem));
            f.ForEach(x => differentItems.Remove(x));

            // Views
            var v = differentItems.OfType<CompareResultItem<ABaseDbView>>().Where(x => !x.SourceItem.AlterScriptSupported).ToList();
            onlySourceDb.Views.AddRange(v.Select(x => x.SourceItem));
            onlyTargetDb.Views.AddRange(v.Select(x => x.TargetItem));
            v.ForEach(x => differentItems.Remove(x));

            return scripter.GenerateFullAlterScript(differentItems, onlySourceDb, onlyTargetDb);
        }
    }
}
