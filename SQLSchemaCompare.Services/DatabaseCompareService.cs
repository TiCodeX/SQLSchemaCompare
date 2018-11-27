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
        private readonly ITaskService taskService;

        private readonly List<CompareResultItem<ABaseDbTable>> tables = new List<CompareResultItem<ABaseDbTable>>();
        private readonly List<CompareResultItem<ABaseDbIndex>> indexes = new List<CompareResultItem<ABaseDbIndex>>();
        private readonly List<CompareResultItem<ABaseDbConstraint>> constraints = new List<CompareResultItem<ABaseDbConstraint>>();
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

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "TODO")]
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
            taskInfo.Percentage = 10;

            taskInfo.Message = Localization.StatusMappingIndexes;
            foreach (var index in this.retrievedSourceDatabase.Indexes)
            {
                this.indexes.Add(new CompareResultItem<ABaseDbIndex>
                {
                    SourceItem = index,
                    TargetItem = this.retrievedTargetDatabase.Indexes.FirstOrDefault(x => x.TableSchema == index.TableSchema && x.TableName == index.TableName &&
                                                                                          x.Schema == index.Schema && x.Name == index.Name)
                });
            }

            foreach (var index in this.retrievedTargetDatabase.Indexes.Where(x =>
                !this.indexes.Any(y => y.SourceItem.TableSchema == x.TableSchema && y.SourceItem.TableName == x.TableName &&
                                       y.SourceItem.Schema == x.Schema && y.SourceItem.Name == x.Name)).ToList())
            {
                this.indexes.Add(new CompareResultItem<ABaseDbIndex>
                {
                    TargetItem = index
                });
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();
            taskInfo.Percentage = 20;

            taskInfo.Message = Localization.StatusMappingConstraints;
            foreach (var constraint in this.retrievedSourceDatabase.Constraints)
            {
                this.constraints.Add(new CompareResultItem<ABaseDbConstraint>
                {
                    SourceItem = constraint,
                    TargetItem = this.retrievedTargetDatabase.Constraints.FirstOrDefault(x => x.TableSchema == constraint.TableSchema && x.TableName == constraint.TableName &&
                                                                                              x.Schema == constraint.Schema && x.Name == constraint.Name)
                });
            }

            foreach (var constraint in this.retrievedTargetDatabase.Constraints.Where(x =>
                !this.constraints.Any(y => y.SourceItem.TableSchema == x.TableSchema && y.SourceItem.TableName == x.TableName &&
                                           y.SourceItem.Schema == x.Schema && y.SourceItem.Name == x.Name)).ToList())
            {
                this.constraints.Add(new CompareResultItem<ABaseDbConstraint>
                {
                    TargetItem = constraint
                });
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();
            taskInfo.Percentage = 30;

            taskInfo.Message = Localization.StatusMappingForeignKeys;
            foreach (var foreignKey in this.retrievedSourceDatabase.ForeignKeys)
            {
                this.foreignKeys.Add(new CompareResultItem<ABaseDbForeignKey>
                {
                    SourceItem = foreignKey,
                    TargetItem = this.retrievedTargetDatabase.ForeignKeys.FirstOrDefault(x => x.TableSchema == foreignKey.TableSchema && x.TableName == foreignKey.TableName &&
                                                                                              x.Schema == foreignKey.Schema && x.Name == foreignKey.Name)
                });
            }

            foreach (var foreignKey in this.retrievedTargetDatabase.ForeignKeys.Where(x =>
                !this.foreignKeys.Any(y => y.SourceItem.TableSchema == x.TableSchema && y.SourceItem.TableName == x.TableName &&
                                           y.SourceItem.Schema == x.Schema && y.SourceItem.Name == x.Name)).ToList())
            {
                this.foreignKeys.Add(new CompareResultItem<ABaseDbForeignKey>
                {
                    TargetItem = foreignKey
                });
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();
            taskInfo.Percentage = 40;

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
            taskInfo.Percentage = 50;

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
            taskInfo.Percentage = 70;

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
            taskInfo.Percentage = 80;

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

            taskInfo.CancellationToken.ThrowIfCancellationRequested();
            taskInfo.Percentage = 90;

            taskInfo.Message = Localization.StatusMappingTriggers;
            foreach (var trigger in this.retrievedSourceDatabase.Triggers)
            {
                this.triggers.Add(new CompareResultItem<ABaseDbTrigger>
                {
                    SourceItem = trigger,
                    TargetItem = this.retrievedTargetDatabase.Triggers.FirstOrDefault(x => x.TableSchema == trigger.TableSchema && x.TableName == trigger.TableName &&
                                                                                           x.Schema == trigger.Schema && x.Name == trigger.Name)
                });
            }

            foreach (var trigger in this.retrievedTargetDatabase.Triggers.Where(x =>
                !this.triggers.Any(y => y.SourceItem.TableSchema == x.TableSchema && y.SourceItem.TableName == x.TableName &&
                                        y.SourceItem.Schema == x.Schema && y.SourceItem.Name == x.Name)).ToList())
            {
                this.triggers.Add(new CompareResultItem<ABaseDbTrigger>
                {
                    TargetItem = trigger
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
                             this.indexes.Count +
                             this.constraints.Count +
                             this.foreignKeys.Count +
                             this.triggers.Count +
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

                if (!resultTable.Equal)
                {
                    resultTable.Scripts.AlterScript = scripter.GenerateAlterTableScript(resultTable.SourceItem, resultTable.TargetItem);
                }

                taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            taskInfo.Message = Localization.StatusComparingIndexes;
            foreach (var resultIndex in this.indexes)
            {
                if (resultIndex.SourceItem != null)
                {
                    resultIndex.SourceItemName = scripter.GenerateObjectName(resultIndex.SourceItem);
                    resultIndex.Scripts.SourceCreateScript = scripter.GenerateCreateIndexScript(resultIndex.SourceItem);
                }

                if (resultIndex.TargetItem != null)
                {
                    resultIndex.TargetItemName = scripter.GenerateObjectName(resultIndex.TargetItem);
                    resultIndex.Scripts.TargetCreateScript = scripter.GenerateCreateIndexScript(resultIndex.TargetItem);
                }

                taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            taskInfo.Message = Localization.StatusComparingConstraints;
            foreach (var resultConstraint in this.constraints)
            {
                if (resultConstraint.SourceItem != null)
                {
                    resultConstraint.SourceItemName = scripter.GenerateObjectName(resultConstraint.SourceItem);
                    resultConstraint.Scripts.SourceCreateScript = scripter.GenerateCreateConstraintScript(resultConstraint.SourceItem);
                }

                if (resultConstraint.TargetItem != null)
                {
                    resultConstraint.TargetItemName = scripter.GenerateObjectName(resultConstraint.TargetItem);
                    resultConstraint.Scripts.TargetCreateScript = scripter.GenerateCreateConstraintScript(resultConstraint.TargetItem);
                }

                taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            taskInfo.Message = Localization.StatusComparingForeignKeys;
            foreach (var resultForeignKey in this.foreignKeys)
            {
                if (resultForeignKey.SourceItem != null)
                {
                    resultForeignKey.SourceItemName = scripter.GenerateObjectName(resultForeignKey.SourceItem);
                    resultForeignKey.Scripts.SourceCreateScript = scripter.GenerateCreateForeignKeyScript(resultForeignKey.SourceItem);
                }

                if (resultForeignKey.TargetItem != null)
                {
                    resultForeignKey.TargetItemName = scripter.GenerateObjectName(resultForeignKey.TargetItem);
                    resultForeignKey.Scripts.TargetCreateScript = scripter.GenerateCreateForeignKeyScript(resultForeignKey.TargetItem);
                }

                taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
            }

            taskInfo.CancellationToken.ThrowIfCancellationRequested();

            taskInfo.Message = Localization.StatusComparingTriggers;
            foreach (var resultTrigger in this.triggers)
            {
                if (resultTrigger.SourceItem != null)
                {
                    resultTrigger.SourceItemName = scripter.GenerateObjectName(resultTrigger.SourceItem);
                    resultTrigger.Scripts.SourceCreateScript = scripter.GenerateCreateTriggerScript(resultTrigger.SourceItem);
                }

                if (resultTrigger.TargetItem != null)
                {
                    resultTrigger.TargetItemName = scripter.GenerateObjectName(resultTrigger.TargetItem);
                    resultTrigger.Scripts.TargetCreateScript = scripter.GenerateCreateTriggerScript(resultTrigger.TargetItem);
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

                if (!resultType.Equal)
                {
                    resultType.Scripts.AlterScript = scripter.GenerateAlterTypeScript(resultType.SourceItem, this.retrievedSourceDatabase.DataTypes, resultType.TargetItem, this.retrievedTargetDatabase.DataTypes);
                }

                taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
            }

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
            result.DifferentItems.AddRange(this.tables.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            result.DifferentItems.AddRange(this.views.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            result.DifferentItems.AddRange(this.functions.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            result.DifferentItems.AddRange(this.storedProcedures.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            result.DifferentItems.AddRange(this.sequences.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            result.DifferentItems.AddRange(this.dataTypes.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal).OrderBy(x => x.SourceItemName));
            this.logger.LogDebug($"Different items => {result.DifferentItems.Count}");

            result.OnlySourceItems.AddRange(this.tables.Where(x => x.SourceItem != null && x.TargetItem == null).OrderBy(x => x.SourceItemName));
            result.OnlySourceItems.AddRange(this.views.Where(x => x.SourceItem != null && x.TargetItem == null).OrderBy(x => x.SourceItemName));
            result.OnlySourceItems.AddRange(this.functions.Where(x => x.SourceItem != null && x.TargetItem == null).OrderBy(x => x.SourceItemName));
            result.OnlySourceItems.AddRange(this.storedProcedures.Where(x => x.SourceItem != null && x.TargetItem == null).OrderBy(x => x.SourceItemName));
            result.OnlySourceItems.AddRange(this.sequences.Where(x => x.SourceItem != null && x.TargetItem == null).OrderBy(x => x.SourceItemName));
            result.OnlySourceItems.AddRange(this.dataTypes.Where(x => x.SourceItem != null && x.TargetItem == null).OrderBy(x => x.SourceItemName));
            this.logger.LogDebug($"Only Source items => {result.OnlySourceItems.Count}");

            result.OnlyTargetItems.AddRange(this.tables.Where(x => x.TargetItem != null && x.SourceItem == null).OrderBy(x => x.TargetItemName));
            result.OnlyTargetItems.AddRange(this.views.Where(x => x.TargetItem != null && x.SourceItem == null).OrderBy(x => x.TargetItemName));
            result.OnlyTargetItems.AddRange(this.functions.Where(x => x.TargetItem != null && x.SourceItem == null).OrderBy(x => x.TargetItemName));
            result.OnlyTargetItems.AddRange(this.storedProcedures.Where(x => x.TargetItem != null && x.SourceItem == null).OrderBy(x => x.TargetItemName));
            result.OnlyTargetItems.AddRange(this.sequences.Where(x => x.TargetItem != null && x.SourceItem == null).OrderBy(x => x.TargetItemName));
            result.OnlyTargetItems.AddRange(this.dataTypes.Where(x => x.TargetItem != null && x.SourceItem == null).OrderBy(x => x.TargetItemName));
            this.logger.LogDebug($"Only Target items => {result.OnlyTargetItems.Count}");

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
            differentItems.AddRange(this.tables.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal));
            differentItems.AddRange(this.views.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal));
            differentItems.AddRange(this.functions.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal));
            differentItems.AddRange(this.storedProcedures.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal));
            differentItems.AddRange(this.sequences.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal));
            differentItems.AddRange(this.dataTypes.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal));
            differentItems.AddRange(this.indexes.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal));
            differentItems.AddRange(this.constraints.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal));
            differentItems.AddRange(this.foreignKeys.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal));
            differentItems.AddRange(this.triggers.Where(x => x.SourceItem != null && x.TargetItem != null && !x.Equal));

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

            onlySourceDb.Tables.AddRange(this.tables.Where(x => x.SourceItem != null && x.TargetItem == null).Select(x => x.SourceItem));
            onlySourceDb.Indexes.AddRange(this.indexes.Where(x => x.SourceItem != null && x.TargetItem == null).Select(x => x.SourceItem));
            onlySourceDb.Constraints.AddRange(this.constraints.Where(x => x.SourceItem != null && x.TargetItem == null).Select(x => x.SourceItem));
            onlySourceDb.ForeignKeys.AddRange(this.foreignKeys.Where(x => x.SourceItem != null && x.TargetItem == null).Select(x => x.SourceItem));
            onlySourceDb.Triggers.AddRange(this.triggers.Where(x => x.SourceItem != null && x.TargetItem == null).Select(x => x.SourceItem));
            onlySourceDb.Views.AddRange(this.views.Where(x => x.SourceItem != null && x.TargetItem == null).Select(x => x.SourceItem));
            onlySourceDb.Functions.AddRange(this.functions.Where(x => x.SourceItem != null && x.TargetItem == null).Select(x => x.SourceItem));
            onlySourceDb.StoredProcedures.AddRange(this.storedProcedures.Where(x => x.SourceItem != null && x.TargetItem == null).Select(x => x.SourceItem));
            onlySourceDb.DataTypes.AddRange(this.retrievedSourceDatabase.DataTypes.Where(x => x.IsUserDefined == false));
            onlySourceDb.DataTypes.AddRange(this.dataTypes.Where(x => x.SourceItem != null && x.TargetItem == null).Select(x => x.SourceItem));
            onlySourceDb.Sequences.AddRange(this.sequences.Where(x => x.SourceItem != null && x.TargetItem == null).Select(x => x.SourceItem));

            onlyTargetDb.Tables.AddRange(this.tables.Where(x => x.SourceItem == null && x.TargetItem != null).Select(x => x.TargetItem));
            onlyTargetDb.Indexes.AddRange(this.indexes.Where(x => x.SourceItem == null && x.TargetItem != null).Select(x => x.TargetItem));
            onlyTargetDb.Constraints.AddRange(this.constraints.Where(x => x.SourceItem == null && x.TargetItem != null).Select(x => x.TargetItem));
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
