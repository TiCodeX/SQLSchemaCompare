using System.Collections.Generic;
using System.Linq;
using SQLCompare.Core.Entities;
using SQLCompare.Core.Entities.Compare;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Interfaces;
using SQLCompare.Core.Interfaces.Services;

namespace SQLCompare.Services
{
    /// <summary>
    /// Implementation that provides the mechanisms to compare two database instances
    /// </summary>
    public class DatabaseCompareService : IDatabaseCompareService
    {
        private readonly IProjectService projectService;
        private readonly IDatabaseService databaseService;
        private readonly IDatabaseScripterFactory databaseScripterFactory;
        private readonly ITaskService taskService;

        private readonly CompareResult result = new CompareResult();
        private ABaseDb retrievedSourceDatabase;
        private ABaseDb retrievedTargetDatabase;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseCompareService"/> class
        /// </summary>
        /// <param name="projectService">The injected project service</param>
        /// <param name="databaseService">The injected database service</param>
        /// <param name="databaseScripterFactory">The injected database scripter factory</param>
        /// <param name="taskService">The injected task service</param>
        public DatabaseCompareService(
            IProjectService projectService,
            IDatabaseService databaseService,
            IDatabaseScripterFactory databaseScripterFactory,
            ITaskService taskService)
        {
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
                    new TaskInfo("Registering source database"),
                    true,
                    taskInfo =>
                    {
                        this.retrievedSourceDatabase = this.databaseService.GetDatabase(
                            this.projectService.Project.SourceProviderOptions, taskInfo);
                        return true;
                    }),
                new TaskWork(
                    new TaskInfo("Registering target database"),
                    true,
                    taskInfo =>
                    {
                        this.retrievedTargetDatabase = this.databaseService.GetDatabase(
                            this.projectService.Project.TargetProviderOptions, taskInfo);
                        return true;
                    }),
                new TaskWork(
                    new TaskInfo("Mapping databases"),
                    false,
                    taskInfo =>
                    {
                        // TODO: Perform mapping with user config from project
                        taskInfo.Message = "Mapping tables...";
                        foreach (var table in this.retrievedSourceDatabase.Tables)
                        {
                            this.result.Tables.Add(new CompareResultItem<ABaseDbTable>
                            {
                                SourceItem = table,
                                TargetItem = this.retrievedTargetDatabase.Tables.FirstOrDefault(x => x.Schema == table.Schema && x.Name == table.Name)
                            });
                        }

                        foreach (var table in this.retrievedTargetDatabase.Tables.Where(x =>
                            !this.result.Tables.Any(y => y.SourceItem.Schema == x.Schema && y.SourceItem.Name == x.Name)).ToList())
                        {
                            this.result.Tables.Add(new CompareResultItem<ABaseDbTable>
                            {
                                TargetItem = table
                            });
                        }

                        taskInfo.Percentage = 15;

                        taskInfo.Message = "Mapping views...";
                        foreach (var view in this.retrievedSourceDatabase.Views)
                        {
                            this.result.Views.Add(new CompareResultItem<ABaseDbView>
                            {
                                SourceItem = view,
                                TargetItem = this.retrievedTargetDatabase.Views.FirstOrDefault(x => x.Schema == view.Schema && x.Name == view.Name)
                            });
                        }

                        foreach (var view in this.retrievedTargetDatabase.Views.Where(x =>
                            !this.result.Views.Any(y => y.SourceItem.Schema == x.Schema && y.SourceItem.Name == x.Name)).ToList())
                        {
                            this.result.Views.Add(new CompareResultItem<ABaseDbView>
                            {
                                TargetItem = view
                            });
                        }

                        taskInfo.Percentage = 30;

                        taskInfo.Message = "Mapping functions...";
                        foreach (var function in this.retrievedSourceDatabase.Functions)
                        {
                            this.result.Functions.Add(new CompareResultItem<ABaseDbFunction>
                            {
                                SourceItem = function,
                                TargetItem = this.retrievedTargetDatabase.Functions.FirstOrDefault(x => x.Schema == function.Schema && x.Name == function.Name)
                            });
                        }

                        foreach (var function in this.retrievedTargetDatabase.Functions.Where(x =>
                            !this.result.Functions.Any(y => y.SourceItem.Schema == x.Schema && y.SourceItem.Name == x.Name)).ToList())
                        {
                            this.result.Functions.Add(new CompareResultItem<ABaseDbFunction>
                            {
                                TargetItem = function
                            });
                        }

                        taskInfo.Percentage = 45;

                        taskInfo.Message = "Mapping stored procedures...";
                        foreach (var storedProcedure in this.retrievedSourceDatabase.StoredProcedures)
                        {
                            this.result.StoredProcedures.Add(new CompareResultItem<ABaseDbStoredProcedure>
                            {
                                SourceItem = storedProcedure,
                                TargetItem = this.retrievedTargetDatabase.StoredProcedures.FirstOrDefault(x => x.Schema == storedProcedure.Schema && x.Name == storedProcedure.Name)
                            });
                        }

                        foreach (var storedProcedure in this.retrievedTargetDatabase.StoredProcedures.Where(x =>
                            !this.result.StoredProcedures.Any(y => y.SourceItem.Schema == x.Schema && y.SourceItem.Name == x.Name)).ToList())
                        {
                            this.result.StoredProcedures.Add(new CompareResultItem<ABaseDbStoredProcedure>
                            {
                                TargetItem = storedProcedure
                            });
                        }

                        taskInfo.Percentage = 60;

                        taskInfo.Message = "Mapping triggers...";
                        foreach (var trigger in this.retrievedSourceDatabase.Triggers)
                        {
                            this.result.Triggers.Add(new CompareResultItem<ABaseDbTrigger>
                            {
                                SourceItem = trigger,
                                TargetItem = this.retrievedTargetDatabase.Triggers.FirstOrDefault(x => x.Schema == trigger.Schema &&
                                                                                                       x.Name == trigger.Name &&
                                                                                                       x.TableSchema == trigger.TableSchema &&
                                                                                                       x.TableName == trigger.TableName)
                            });
                        }

                        foreach (var trigger in this.retrievedTargetDatabase.Triggers.Where(x =>
                            !this.result.Triggers.Any(y => y.SourceItem.Schema == x.Schema &&
                                y.SourceItem.Name == x.Name &&
                                y.SourceItem.TableSchema == x.TableSchema &&
                                y.SourceItem.TableName == x.TableName)).ToList())
                        {
                            this.result.Triggers.Add(new CompareResultItem<ABaseDbTrigger>
                            {
                                TargetItem = trigger
                            });
                        }

                        taskInfo.Percentage = 75;

                        taskInfo.Message = "Mapping sequences...";
                        foreach (var sequence in this.retrievedSourceDatabase.Sequences)
                        {
                            this.result.Sequences.Add(new CompareResultItem<ABaseDbSequence>
                            {
                                SourceItem = sequence,
                                TargetItem = this.retrievedTargetDatabase.Sequences.FirstOrDefault(x => x.Schema == sequence.Schema && x.Name == sequence.Name)
                            });
                        }

                        foreach (var sequence in this.retrievedTargetDatabase.Sequences.Where(x =>
                            !this.result.Sequences.Any(y => y.SourceItem.Schema == x.Schema && y.SourceItem.Name == x.Name)).ToList())
                        {
                            this.result.Sequences.Add(new CompareResultItem<ABaseDbSequence>
                            {
                                TargetItem = sequence
                            });
                        }

                        taskInfo.Percentage = 90;

                        taskInfo.Message = "Mapping user defined types...";
                        foreach (var type in this.retrievedSourceDatabase.DataTypes.Where(x => x.IsUserDefined))
                        {
                            this.result.DataTypes.Add(new CompareResultItem<ABaseDbDataType>
                            {
                                SourceItem = type,
                                TargetItem = this.retrievedTargetDatabase.DataTypes.FirstOrDefault(x => x.Schema == type.Schema && x.Name == type.Name)
                            });
                        }

                        foreach (var type in this.retrievedTargetDatabase.DataTypes.Where(x => x.IsUserDefined &&
                            !this.result.DataTypes.Any(y => y.SourceItem.Schema == x.Schema && y.SourceItem.Name == x.Name)).ToList())
                        {
                            this.result.DataTypes.Add(new CompareResultItem<ABaseDbDataType>
                            {
                                TargetItem = type
                            });
                        }

                        taskInfo.Message = string.Empty;
                        taskInfo.Percentage = 100;

                        return true;
                    }),
                new TaskWork(
                    new TaskInfo("Comparing databases"),
                    false,
                    taskInfo =>
                    {
                        var totalItems = this.result.Tables.Count +
                                         this.result.Views.Count +
                                         this.result.Functions.Count +
                                         this.result.StoredProcedures.Count +
                                         this.result.Triggers.Count +
                                         this.result.DataTypes.Count;
                        var processedItems = 1;
                        var scripter = this.databaseScripterFactory.Create(
                            this.retrievedSourceDatabase,
                            this.projectService.Project.Options);

                        taskInfo.Message = "Comparing tables...";
                        foreach (var resultTable in this.result.Tables)
                        {
                            if (resultTable.SourceItem != null)
                            {
                                resultTable.SourceItemName = scripter.GenerateObjectName(resultTable.SourceItem);
                                resultTable.SourceCreateScript = scripter.GenerateCreateTableScript(resultTable.SourceItem);
                            }

                            if (resultTable.TargetItem != null)
                            {
                                resultTable.TargetItemName = scripter.GenerateObjectName(resultTable.TargetItem);
                                resultTable.TargetCreateScript = scripter.GenerateCreateTableScript(resultTable.TargetItem, resultTable.SourceItem);
                            }

                            resultTable.Equal = resultTable.SourceCreateScript == resultTable.TargetCreateScript;
                            taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
                        }

                        taskInfo.Message = "Comparing views...";
                        foreach (var resultView in this.result.Views)
                        {
                            if (resultView.SourceItem != null)
                            {
                                resultView.SourceItemName = scripter.GenerateObjectName(resultView.SourceItem);
                                resultView.SourceCreateScript = scripter.GenerateCreateViewScript(resultView.SourceItem);
                            }

                            if (resultView.TargetItem != null)
                            {
                                resultView.TargetItemName = scripter.GenerateObjectName(resultView.TargetItem);
                                resultView.TargetCreateScript = scripter.GenerateCreateViewScript(resultView.TargetItem);
                            }

                            resultView.Equal = resultView.SourceCreateScript == resultView.TargetCreateScript;
                            taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
                        }

                        taskInfo.Message = "Comparing functions...";
                        foreach (var resultFunction in this.result.Functions)
                        {
                            if (resultFunction.SourceItem != null)
                            {
                                resultFunction.SourceItemName = scripter.GenerateObjectName(resultFunction.SourceItem);
                                resultFunction.SourceCreateScript = scripter.GenerateCreateFunctionScript(resultFunction.SourceItem, this.retrievedSourceDatabase.DataTypes);
                            }

                            if (resultFunction.TargetItem != null)
                            {
                                resultFunction.TargetItemName = scripter.GenerateObjectName(resultFunction.TargetItem);
                                resultFunction.TargetCreateScript = scripter.GenerateCreateFunctionScript(resultFunction.TargetItem, this.retrievedTargetDatabase.DataTypes);
                            }

                            resultFunction.Equal = resultFunction.SourceCreateScript == resultFunction.TargetCreateScript;
                            taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
                        }

                        taskInfo.Message = "Comparing stored procedures...";
                        foreach (var resultStoredProcedure in this.result.StoredProcedures)
                        {
                            if (resultStoredProcedure.SourceItem != null)
                            {
                                resultStoredProcedure.SourceItemName = scripter.GenerateObjectName(resultStoredProcedure.SourceItem);
                                resultStoredProcedure.SourceCreateScript = scripter.GenerateCreateStoredProcedureScript(resultStoredProcedure.SourceItem);
                            }

                            if (resultStoredProcedure.TargetItem != null)
                            {
                                resultStoredProcedure.TargetItemName = scripter.GenerateObjectName(resultStoredProcedure.TargetItem);
                                resultStoredProcedure.TargetCreateScript = scripter.GenerateCreateStoredProcedureScript(resultStoredProcedure.TargetItem);
                            }

                            resultStoredProcedure.Equal = resultStoredProcedure.SourceCreateScript == resultStoredProcedure.TargetCreateScript;
                            taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
                        }

                        taskInfo.Message = "Comparing triggers...";
                        foreach (var resultTrigger in this.result.Triggers)
                        {
                            if (resultTrigger.SourceItem != null)
                            {
                                resultTrigger.SourceItemName = scripter.GenerateObjectName(resultTrigger.SourceItem);
                                resultTrigger.SourceCreateScript = scripter.GenerateCreateTriggerScript(resultTrigger.SourceItem);
                            }

                            if (resultTrigger.TargetItem != null)
                            {
                                resultTrigger.TargetItemName = scripter.GenerateObjectName(resultTrigger.TargetItem);
                                resultTrigger.TargetCreateScript = scripter.GenerateCreateTriggerScript(resultTrigger.TargetItem);
                            }

                            resultTrigger.Equal = resultTrigger.SourceCreateScript == resultTrigger.TargetCreateScript;
                            taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
                        }

                        taskInfo.Message = "Comparing sequences...";
                        foreach (var resultSequence in this.result.Sequences)
                        {
                            if (resultSequence.SourceItem != null)
                            {
                                resultSequence.SourceItemName = scripter.GenerateObjectName(resultSequence.SourceItem);
                                resultSequence.SourceCreateScript = scripter.GenerateCreateSequenceScript(resultSequence.SourceItem);
                            }

                            if (resultSequence.TargetItem != null)
                            {
                                resultSequence.TargetItemName = scripter.GenerateObjectName(resultSequence.TargetItem);
                                resultSequence.TargetCreateScript = scripter.GenerateCreateSequenceScript(resultSequence.TargetItem);
                            }

                            resultSequence.Equal = resultSequence.SourceCreateScript == resultSequence.TargetCreateScript;
                            taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
                        }

                        taskInfo.Message = "Comparing User-Defined Data Types...";
                        foreach (var resultType in this.result.DataTypes)
                        {
                            if (resultType.SourceItem != null)
                            {
                                resultType.SourceItemName = scripter.GenerateObjectName(resultType.SourceItem);
                                resultType.SourceCreateScript = scripter.GenerateCreateTypeScript(resultType.SourceItem, this.retrievedSourceDatabase.DataTypes);
                            }

                            if (resultType.TargetItem != null)
                            {
                                resultType.TargetItemName = scripter.GenerateObjectName(resultType.TargetItem);
                                resultType.TargetCreateScript = scripter.GenerateCreateTypeScript(resultType.TargetItem, this.retrievedTargetDatabase.DataTypes);
                            }

                            resultType.Equal = resultType.SourceCreateScript == resultType.TargetCreateScript;
                            taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
                        }

                        this.projectService.Project.Result = this.result;

                        return true;
                    })
            });
        }
    }
}
