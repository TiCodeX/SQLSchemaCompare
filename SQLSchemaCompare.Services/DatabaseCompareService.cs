using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities;
using SQLCompare.Core.Entities.Compare;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Database.MicrosoftSql;
using SQLCompare.Core.Entities.Database.MySql;
using SQLCompare.Core.Entities.Database.PostgreSql;
using SQLCompare.Core.Interfaces;
using SQLCompare.Core.Interfaces.Services;

namespace SQLCompare.Services
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
        private readonly ITaskService taskService;

        private readonly List<CompareResultItem<ABaseDbTable>> tables = new List<CompareResultItem<ABaseDbTable>>();
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
        /// <param name="taskService">The injected task service</param>
        public DatabaseCompareService(
            ILoggerFactory loggerFactory,
            IProjectService projectService,
            IDatabaseService databaseService,
            IDatabaseScripterFactory databaseScripterFactory,
            ITaskService taskService)
        {
            this.logger = loggerFactory.CreateLogger(nameof(DatabaseCompareService));
            this.projectService = projectService;
            this.databaseService = databaseService;
            this.databaseScripterFactory = databaseScripterFactory;
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
                        return true;
                    }),
                new TaskWork(
                    new TaskInfo(Localization.LabelRetrieveTargetDatabase),
                    true,
                    taskInfo =>
                    {
                        this.retrievedTargetDatabase = this.databaseService.GetDatabase(
                            this.projectService.Project.TargetProviderOptions, taskInfo);
                        return true;
                    }),
                new TaskWork(
                    new TaskInfo(Localization.LabelMappingDatabaseObjects),
                    false,
                    this.ExecuteMappingDatabaseObjects),
                new TaskWork(
                    new TaskInfo(Localization.LabelDatabaseComparison),
                    false,
                    this.ExecuteDatabaseComparison)
            });
        }

        private bool ExecuteMappingDatabaseObjects(TaskInfo taskInfo)
        {
            // TODO: Perform mapping with user config from project
            taskInfo.Message = Localization.StatusMappingTables;
            foreach (var table in this.retrievedSourceDatabase.Tables)
            {
                this.tables.Add(new CompareResultItem<ABaseDbTable>
                {
                    SourceItem = table,
                    TargetItem = this.retrievedTargetDatabase.Tables.FirstOrDefault(x => x.Schema == table.Schema && x.Name == table.Name)
                });
            }

            foreach (var table in this.retrievedTargetDatabase.Tables.Where(x =>
                !this.tables.Any(y => y.SourceItem.Schema == x.Schema && y.SourceItem.Name == x.Name)).ToList())
            {
                this.tables.Add(new CompareResultItem<ABaseDbTable>
                {
                    TargetItem = table
                });
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();
            taskInfo.Percentage = 20;

            taskInfo.Message = Localization.StatusMappingViews;
            foreach (var view in this.retrievedSourceDatabase.Views)
            {
                this.views.Add(new CompareResultItem<ABaseDbView>
                {
                    SourceItem = view,
                    TargetItem = this.retrievedTargetDatabase.Views.FirstOrDefault(x => x.Schema == view.Schema && x.Name == view.Name)
                });
            }

            foreach (var view in this.retrievedTargetDatabase.Views.Where(x =>
                !this.views.Any(y => y.SourceItem.Schema == x.Schema && y.SourceItem.Name == x.Name)).ToList())
            {
                this.views.Add(new CompareResultItem<ABaseDbView>
                {
                    TargetItem = view
                });
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();
            taskInfo.Percentage = 40;

            taskInfo.Message = Localization.StatusMappingFunctions;
            foreach (var function in this.retrievedSourceDatabase.Functions)
            {
                this.functions.Add(new CompareResultItem<ABaseDbFunction>
                {
                    SourceItem = function,
                    TargetItem = this.retrievedTargetDatabase.Functions.FirstOrDefault(x => x.Schema == function.Schema && x.Name == function.Name)
                });
            }

            foreach (var function in this.retrievedTargetDatabase.Functions.Where(x =>
                !this.functions.Any(y => y.SourceItem.Schema == x.Schema && y.SourceItem.Name == x.Name)).ToList())
            {
                this.functions.Add(new CompareResultItem<ABaseDbFunction>
                {
                    TargetItem = function
                });
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();
            taskInfo.Percentage = 60;

            taskInfo.Message = Localization.StatusMappingStoredProcedures;
            foreach (var storedProcedure in this.retrievedSourceDatabase.StoredProcedures)
            {
                this.storedProcedures.Add(new CompareResultItem<ABaseDbStoredProcedure>
                {
                    SourceItem = storedProcedure,
                    TargetItem = this.retrievedTargetDatabase.StoredProcedures.FirstOrDefault(x => x.Schema == storedProcedure.Schema && x.Name == storedProcedure.Name)
                });
            }

            foreach (var storedProcedure in this.retrievedTargetDatabase.StoredProcedures.Where(x =>
                !this.storedProcedures.Any(y => y.SourceItem.Schema == x.Schema && y.SourceItem.Name == x.Name)).ToList())
            {
                this.storedProcedures.Add(new CompareResultItem<ABaseDbStoredProcedure>
                {
                    TargetItem = storedProcedure
                });
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();
            taskInfo.Percentage = 80;

            taskInfo.Message = Localization.StatusMappingSequences;
            foreach (var sequence in this.retrievedSourceDatabase.Sequences)
            {
                this.sequences.Add(new CompareResultItem<ABaseDbSequence>
                {
                    SourceItem = sequence,
                    TargetItem = this.retrievedTargetDatabase.Sequences.FirstOrDefault(x => x.Schema == sequence.Schema && x.Name == sequence.Name)
                });
            }

            foreach (var sequence in this.retrievedTargetDatabase.Sequences.Where(x =>
                !this.sequences.Any(y => y.SourceItem.Schema == x.Schema && y.SourceItem.Name == x.Name)).ToList())
            {
                this.sequences.Add(new CompareResultItem<ABaseDbSequence>
                {
                    TargetItem = sequence
                });
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();
            taskInfo.Percentage = 90;

            taskInfo.Message = Localization.StatusMappingDataTypes;
            foreach (var type in this.retrievedSourceDatabase.DataTypes.Where(x => x.IsUserDefined))
            {
                this.dataTypes.Add(new CompareResultItem<ABaseDbDataType>
                {
                    SourceItem = type,
                    TargetItem = this.retrievedTargetDatabase.DataTypes.FirstOrDefault(x => x.Schema == type.Schema && x.Name == type.Name)
                });
            }

            foreach (var type in this.retrievedTargetDatabase.DataTypes.Where(x => x.IsUserDefined &&
                !this.dataTypes.Any(y => y.SourceItem.Schema == x.Schema && y.SourceItem.Name == x.Name)).ToList())
            {
                this.dataTypes.Add(new CompareResultItem<ABaseDbDataType>
                {
                    TargetItem = type
                });
            }

            taskInfo.Message = string.Empty;
            taskInfo.Percentage = 100;

            return true;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "TODO")]
        private bool ExecuteDatabaseComparison(TaskInfo taskInfo)
        {
            var totalItems = this.tables.Count +
                             this.views.Count +
                             this.functions.Count +
                             this.storedProcedures.Count +
                             this.dataTypes.Count;
            if (totalItems == 0)
            {
                throw new DataException(Localization.ErrorEmptyDatabases);
            }

            var processedItems = 1;
            var scripter = this.databaseScripterFactory.Create(
                this.retrievedSourceDatabase,
                this.projectService.Project.Options);

            taskInfo.Message = Localization.StatusComparingTables;
            foreach (var resultTable in this.tables)
            {
                if (resultTable.SourceItem != null)
                {
                    resultTable.SourceItemName = scripter.GenerateObjectName(resultTable.SourceItem);
                    resultTable.Scripts.SourceCreateScript = scripter.GenerateCreateTableScript(resultTable.SourceItem);
                }

                if (resultTable.TargetItem != null)
                {
                    resultTable.TargetItemName = scripter.GenerateObjectName(resultTable.TargetItem);
                    resultTable.Scripts.TargetCreateScript = scripter.GenerateCreateTableScript(resultTable.TargetItem, resultTable.SourceItem);
                }

                resultTable.Equal = resultTable.Scripts.SourceCreateScript == resultTable.Scripts.TargetCreateScript;

                if (!resultTable.Equal)
                {
                    resultTable.Scripts.AlterScript = scripter.GenerateAlterTableScript(resultTable.SourceItem, resultTable.TargetItem);
                }

                taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            taskInfo.Message = Localization.StatusComparingViews;
            foreach (var resultView in this.views)
            {
                if (resultView.SourceItem != null)
                {
                    resultView.SourceItemName = scripter.GenerateObjectName(resultView.SourceItem);
                    resultView.Scripts.SourceCreateScript = scripter.GenerateCreateViewScript(resultView.SourceItem);
                }

                if (resultView.TargetItem != null)
                {
                    resultView.TargetItemName = scripter.GenerateObjectName(resultView.TargetItem);
                    resultView.Scripts.TargetCreateScript = scripter.GenerateCreateViewScript(resultView.TargetItem);
                }

                resultView.Equal = resultView.Scripts.SourceCreateScript == resultView.Scripts.TargetCreateScript;

                if (!resultView.Equal)
                {
                    resultView.Scripts.AlterScript = scripter.GenerateAlterViewScript(resultView.SourceItem, resultView.TargetItem);
                }

                taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            taskInfo.Message = Localization.StatusComparingFunctions;
            foreach (var resultFunction in this.functions)
            {
                if (resultFunction.SourceItem != null)
                {
                    resultFunction.SourceItemName = scripter.GenerateObjectName(resultFunction.SourceItem);
                    resultFunction.Scripts.SourceCreateScript = scripter.GenerateCreateFunctionScript(resultFunction.SourceItem, this.retrievedSourceDatabase.DataTypes);
                }

                if (resultFunction.TargetItem != null)
                {
                    resultFunction.TargetItemName = scripter.GenerateObjectName(resultFunction.TargetItem);
                    resultFunction.Scripts.TargetCreateScript = scripter.GenerateCreateFunctionScript(resultFunction.TargetItem, this.retrievedTargetDatabase.DataTypes);
                }

                resultFunction.Equal = resultFunction.Scripts.SourceCreateScript == resultFunction.Scripts.TargetCreateScript;

                if (!resultFunction.Equal)
                {
                    resultFunction.Scripts.AlterScript = scripter.GenerateAlterFunctionScript(resultFunction.SourceItem, this.retrievedSourceDatabase.DataTypes, resultFunction.TargetItem, this.retrievedTargetDatabase.DataTypes);
                }

                taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            taskInfo.Message = Localization.StatusComparingStoredProcedures;
            foreach (var resultStoredProcedure in this.storedProcedures)
            {
                if (resultStoredProcedure.SourceItem != null)
                {
                    resultStoredProcedure.SourceItemName = scripter.GenerateObjectName(resultStoredProcedure.SourceItem);
                    resultStoredProcedure.Scripts.SourceCreateScript = scripter.GenerateCreateStoredProcedureScript(resultStoredProcedure.SourceItem);
                }

                if (resultStoredProcedure.TargetItem != null)
                {
                    resultStoredProcedure.TargetItemName = scripter.GenerateObjectName(resultStoredProcedure.TargetItem);
                    resultStoredProcedure.Scripts.TargetCreateScript = scripter.GenerateCreateStoredProcedureScript(resultStoredProcedure.TargetItem);
                }

                resultStoredProcedure.Equal = resultStoredProcedure.Scripts.SourceCreateScript == resultStoredProcedure.Scripts.TargetCreateScript;

                if (!resultStoredProcedure.Equal)
                {
                    resultStoredProcedure.Scripts.AlterScript = scripter.GenerateAlterStoredProcedureScript(resultStoredProcedure.SourceItem, resultStoredProcedure.TargetItem);
                }

                taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            taskInfo.Message = Localization.StatusComparingSequences;
            foreach (var resultSequence in this.sequences)
            {
                if (resultSequence.SourceItem != null)
                {
                    resultSequence.SourceItemName = scripter.GenerateObjectName(resultSequence.SourceItem);
                    resultSequence.Scripts.SourceCreateScript = scripter.GenerateCreateSequenceScript(resultSequence.SourceItem);
                }

                if (resultSequence.TargetItem != null)
                {
                    resultSequence.TargetItemName = scripter.GenerateObjectName(resultSequence.TargetItem);
                    resultSequence.Scripts.TargetCreateScript = scripter.GenerateCreateSequenceScript(resultSequence.TargetItem);
                }

                resultSequence.Equal = resultSequence.Scripts.SourceCreateScript == resultSequence.Scripts.TargetCreateScript;

                if (!resultSequence.Equal)
                {
                    resultSequence.Scripts.AlterScript = scripter.GenerateAlterSequenceScript(resultSequence.SourceItem, resultSequence.TargetItem);
                }

                taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            taskInfo.Message = Localization.StatusComparingDataTypes;
            foreach (var resultType in this.dataTypes)
            {
                if (resultType.SourceItem != null)
                {
                    resultType.SourceItemName = scripter.GenerateObjectName(resultType.SourceItem);
                    resultType.Scripts.SourceCreateScript = scripter.GenerateCreateTypeScript(resultType.SourceItem, this.retrievedSourceDatabase.DataTypes);
                }

                if (resultType.TargetItem != null)
                {
                    resultType.TargetItemName = scripter.GenerateObjectName(resultType.TargetItem);
                    resultType.Scripts.TargetCreateScript = scripter.GenerateCreateTypeScript(resultType.TargetItem, this.retrievedTargetDatabase.DataTypes);
                }

                resultType.Equal = resultType.Scripts.SourceCreateScript == resultType.Scripts.TargetCreateScript;

                if (!resultType.Equal)
                {
                    resultType.Scripts.AlterScript = scripter.GenerateAlterTypeScript(resultType.SourceItem, this.retrievedSourceDatabase.DataTypes, resultType.TargetItem, this.retrievedTargetDatabase.DataTypes);
                }

                taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
            }

            var result = new CompareResult
            {
                SourceFullScript = scripter.GenerateFullCreateScript(this.retrievedSourceDatabase),
                TargetFullScript = scripter.GenerateFullCreateScript(this.retrievedTargetDatabase)
            };

            result.DifferentItems.AddRange(this.tables.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            result.DifferentItems.AddRange(this.views.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            result.DifferentItems.AddRange(this.functions.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            result.DifferentItems.AddRange(this.storedProcedures.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            result.DifferentItems.AddRange(this.sequences.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            result.DifferentItems.AddRange(this.dataTypes.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            this.logger.LogDebug($"Different items => {result.DifferentItems.Count}");

            var onlySourceTables = this.tables.Where(x => x.SourceItem != null && x.TargetItem == null).ToList();
            result.OnlySourceItems.AddRange(onlySourceTables.OrderBy(x => x.SourceItemName));
            var onlySourceViews = this.views.Where(x => x.SourceItem != null && x.TargetItem == null).ToList();
            result.OnlySourceItems.AddRange(onlySourceViews.OrderBy(x => x.SourceItemName));
            var onlySourceFunctions = this.functions.Where(x => x.SourceItem != null && x.TargetItem == null).ToList();
            result.OnlySourceItems.AddRange(onlySourceFunctions.OrderBy(x => x.SourceItemName));
            var onlySourceStoredProcedures = this.storedProcedures.Where(x => x.SourceItem != null && x.TargetItem == null).ToList();
            result.OnlySourceItems.AddRange(onlySourceStoredProcedures.OrderBy(x => x.SourceItemName));
            var onlySourceSequences = this.sequences.Where(x => x.SourceItem != null && x.TargetItem == null).ToList();
            result.OnlySourceItems.AddRange(onlySourceSequences.OrderBy(x => x.SourceItemName));
            var onlySourceDataTypes = this.dataTypes.Where(x => x.SourceItem != null && x.TargetItem == null).ToList();
            result.OnlySourceItems.AddRange(onlySourceDataTypes.OrderBy(x => x.SourceItemName));
            this.logger.LogDebug($"Only Source items => {result.OnlySourceItems.Count}");

            var onlyTargetTables = this.tables.Where(x => x.TargetItem != null && x.SourceItem == null).ToList();
            result.OnlyTargetItems.AddRange(onlyTargetTables.OrderBy(x => x.TargetItemName));
            var onlyTargetViews = this.views.Where(x => x.TargetItem != null && x.SourceItem == null).ToList();
            result.OnlyTargetItems.AddRange(onlyTargetViews.OrderBy(x => x.TargetItemName));
            var onlyTargetFunctions = this.functions.Where(x => x.TargetItem != null && x.SourceItem == null).ToList();
            result.OnlyTargetItems.AddRange(onlyTargetFunctions.OrderBy(x => x.TargetItemName));
            var onlyTargetStoredProcedures = this.storedProcedures.Where(x => x.TargetItem != null && x.SourceItem == null).ToList();
            result.OnlyTargetItems.AddRange(onlyTargetStoredProcedures.OrderBy(x => x.TargetItemName));
            var onlyTargetSequences = this.sequences.Where(x => x.TargetItem != null && x.SourceItem == null).ToList();
            result.OnlyTargetItems.AddRange(onlyTargetSequences.OrderBy(x => x.TargetItemName));
            var onlyTargetDataTypes = this.dataTypes.Where(x => x.TargetItem != null && x.SourceItem == null).ToList();
            result.OnlyTargetItems.AddRange(onlyTargetDataTypes.OrderBy(x => x.TargetItemName));
            this.logger.LogDebug($"Only Target items => {result.OnlyTargetItems.Count}");

            result.SameItems.AddRange(this.tables.Where(x => x.SourceItem != null && x.TargetItem != null && x.Equal).OrderBy(x => x.SourceItemName));
            result.SameItems.AddRange(this.views.Where(x => x.SourceItem != null && x.TargetItem != null && x.Equal).OrderBy(x => x.SourceItemName));
            result.SameItems.AddRange(this.functions.Where(x => x.SourceItem != null && x.TargetItem != null && x.Equal).OrderBy(x => x.SourceItemName));
            result.SameItems.AddRange(this.storedProcedures.Where(x => x.SourceItem != null && x.TargetItem != null && x.Equal).OrderBy(x => x.SourceItemName));
            result.SameItems.AddRange(this.sequences.Where(x => x.SourceItem != null && x.TargetItem != null && x.Equal).OrderBy(x => x.SourceItemName));
            result.SameItems.AddRange(this.dataTypes.Where(x => x.SourceItem != null && x.TargetItem != null && x.Equal).OrderBy(x => x.SourceItemName));
            this.logger.LogDebug($"Same items => {result.SameItems.Count}");

            ABaseDb onlySourceDb = null;
            ABaseDb onlyTargetDb = null;
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

            onlySourceDb.Tables.AddRange(onlySourceTables.Select(x => x.SourceItem));
            onlySourceDb.Views.AddRange(onlySourceViews.Select(x => x.SourceItem));
            onlySourceDb.Functions.AddRange(onlySourceFunctions.Select(x => x.SourceItem));
            onlySourceDb.StoredProcedures.AddRange(onlySourceStoredProcedures.Select(x => x.SourceItem));
            onlySourceDb.DataTypes.AddRange(this.retrievedSourceDatabase.DataTypes.Where(x => x.IsUserDefined == false));
            onlySourceDb.DataTypes.AddRange(onlySourceDataTypes.Select(x => x.SourceItem));
            onlySourceDb.Sequences.AddRange(onlySourceSequences.Select(x => x.SourceItem));

            onlyTargetDb.Tables.AddRange(onlyTargetTables.Select(x => x.TargetItem));
            onlyTargetDb.Views.AddRange(onlyTargetViews.Select(x => x.TargetItem));
            onlyTargetDb.Functions.AddRange(onlyTargetFunctions.Select(x => x.TargetItem));
            onlyTargetDb.StoredProcedures.AddRange(onlyTargetStoredProcedures.Select(x => x.TargetItem));
            onlySourceDb.DataTypes.AddRange(this.retrievedTargetDatabase.DataTypes.Where(x => x.IsUserDefined == false));
            onlyTargetDb.DataTypes.AddRange(onlyTargetDataTypes.Select(x => x.TargetItem));
            onlyTargetDb.Sequences.AddRange(onlyTargetSequences.Select(x => x.TargetItem));

            result.FullAlterScript = scripter.GenerateFullAlterScript(result.DifferentItems, onlySourceDb, onlyTargetDb);

            this.projectService.Project.Result = result;

            return true;
        }
    }
}
