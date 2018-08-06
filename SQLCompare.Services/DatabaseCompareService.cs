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
                        taskInfo.Message = "Connecting to source server";
                        for (short i = 1; i < 3; i++)
                        {
                            taskInfo.Percentage = (short)(i * 10);
                        }

                        taskInfo.Message = "Reading source tables";
                        this.retrievedSourceDatabase = this.databaseService.GetDatabase(
                            this.projectService.Project.SourceProviderOptions);

                        taskInfo.Message = string.Empty;
                        taskInfo.Percentage = 100;

                        return true;
                    }),
                new TaskWork(
                    new TaskInfo("Registering target database"),
                    true,
                    taskInfo =>
                    {
                        taskInfo.Message = "Connecting to target server";
                        for (short i = 1; i < 3; i++)
                        {
                            taskInfo.Percentage = (short)(i * 10);
                        }

                        taskInfo.Message = "Reading target tables";
                        this.retrievedTargetDatabase = this.databaseService.GetDatabase(
                            this.projectService.Project.TargetProviderOptions);

                        taskInfo.Message = string.Empty;
                        taskInfo.Percentage = 100;

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
                                ItemTypeLabel = Localization.LabelTable,
                                SourceItem = table,
                                TargetItem = this.retrievedTargetDatabase.Tables.FirstOrDefault(x => x.Name == table.Name)
                            });
                        }

                        foreach (var table in this.retrievedTargetDatabase.Tables.Where(x => this.result.Tables.All(y => y.SourceItem.Name != x.Name)).ToList())
                        {
                            this.result.Tables.Add(new CompareResultItem<ABaseDbTable>
                            {
                                ItemTypeLabel = Localization.LabelTable,
                                TargetItem = table
                            });
                        }

                        taskInfo.Percentage = 25;

                        taskInfo.Message = "Mapping views...";
                        foreach (var view in this.retrievedSourceDatabase.Views)
                        {
                            this.result.Views.Add(new CompareResultItem<ABaseDbView>
                            {
                                ItemTypeLabel = Localization.LabelView,
                                SourceItem = view,
                                TargetItem = this.retrievedTargetDatabase.Views.FirstOrDefault(x => x.Name == view.Name)
                            });
                        }

                        foreach (var view in this.retrievedTargetDatabase.Views.Where(x => this.result.Views.All(y => y.SourceItem.Name != x.Name)).ToList())
                        {
                            this.result.Views.Add(new CompareResultItem<ABaseDbView>
                            {
                                ItemTypeLabel = Localization.LabelView,
                                TargetItem = view
                            });
                        }

                        taskInfo.Percentage = 50;

                        taskInfo.Message = "Mapping functions...";
                        foreach (var function in this.retrievedSourceDatabase.Functions)
                        {
                            this.result.Functions.Add(new CompareResultItem<ABaseDbRoutine>
                            {
                                ItemTypeLabel = Localization.LabelFunction,
                                SourceItem = function,
                                TargetItem = this.retrievedTargetDatabase.Functions.FirstOrDefault(x => x.Name == function.Name)
                            });
                        }

                        foreach (var function in this.retrievedTargetDatabase.Functions.Where(x => this.result.Functions.All(y => y.SourceItem.Name != x.Name)).ToList())
                        {
                            this.result.Functions.Add(new CompareResultItem<ABaseDbRoutine>
                            {
                                ItemTypeLabel = Localization.LabelFunction,
                                TargetItem = function
                            });
                        }

                        taskInfo.Percentage = 75;

                        taskInfo.Message = "Mapping stored procedures...";
                        foreach (var storedProcedure in this.retrievedSourceDatabase.StoredProcedures)
                        {
                            this.result.StoredProcedures.Add(new CompareResultItem<ABaseDbRoutine>
                            {
                                ItemTypeLabel = Localization.LabelStoredProcedure,
                                SourceItem = storedProcedure,
                                TargetItem = this.retrievedTargetDatabase.StoredProcedures.FirstOrDefault(x => x.Name == storedProcedure.Name)
                            });
                        }

                        foreach (var function in this.retrievedTargetDatabase.StoredProcedures.Where(x => this.result.StoredProcedures.All(y => y.SourceItem.Name != x.Name)).ToList())
                        {
                            this.result.StoredProcedures.Add(new CompareResultItem<ABaseDbRoutine>
                            {
                                ItemTypeLabel = Localization.LabelStoredProcedure,
                                TargetItem = function
                            });
                        }

                        taskInfo.Percentage = 95;

                        taskInfo.Message = "Mapping sequences...";
                        foreach (var sequence in this.retrievedSourceDatabase.Sequences)
                        {
                            this.result.Sequences.Add(new CompareResultItem<ABaseDbSequence>
                            {
                                ItemTypeLabel = Localization.LabelSequence,
                                SourceItem = sequence,
                                TargetItem = this.retrievedTargetDatabase.Sequences.FirstOrDefault(x => x.Name == sequence.Name)
                            });
                        }

                        foreach (var sequence in this.retrievedTargetDatabase.Sequences.Where(x => this.result.Sequences.All(y => y.SourceItem.Name != x.Name)).ToList())
                        {
                            this.result.Sequences.Add(new CompareResultItem<ABaseDbSequence>
                            {
                                ItemTypeLabel = Localization.LabelSequence,
                                TargetItem = sequence
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
                                         this.result.StoredProcedures.Count;
                        var processedItems = 1;
                        var scripter = this.databaseScripterFactory.Create(
                            this.retrievedSourceDatabase,
                            this.projectService.Project.Options);

                        taskInfo.Message = "Comparing tables...";
                        foreach (var resultTable in this.result.Tables)
                        {
                            if (resultTable.SourceItem != null)
                            {
                                resultTable.SourceCreateScript = scripter.GenerateCreateTableScript(resultTable.SourceItem);
                            }

                            if (resultTable.TargetItem != null)
                            {
                                resultTable.TargetCreateScript = scripter.GenerateCreateTableScript(resultTable.TargetItem, resultTable.SourceItem);
                            }

                            resultTable.Equal = resultTable.SourceCreateScript == resultTable.TargetCreateScript;

                            taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
                        }

                        taskInfo.Message = "Comparing views...";
                        foreach (var resultView in this.result.Views)
                        {
                            resultView.SourceCreateScript = resultView.SourceItem?.ViewDefinition ?? string.Empty;
                            resultView.TargetCreateScript = resultView.TargetItem?.ViewDefinition ?? string.Empty;
                            resultView.Equal = resultView.SourceCreateScript == resultView.TargetCreateScript;
                            taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
                        }

                        taskInfo.Message = "Comparing functions...";
                        foreach (var resultFunction in this.result.Functions)
                        {
                            resultFunction.SourceCreateScript = resultFunction.SourceItem?.RoutineDefinition ?? string.Empty;
                            resultFunction.TargetCreateScript = resultFunction.TargetItem?.RoutineDefinition ?? string.Empty;
                            resultFunction.Equal = resultFunction.SourceCreateScript == resultFunction.TargetCreateScript;
                            taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
                        }

                        taskInfo.Message = "Comparing stored procedures...";
                        foreach (var resultStoredProcedure in this.result.StoredProcedures)
                        {
                            resultStoredProcedure.SourceCreateScript = resultStoredProcedure.SourceItem?.RoutineDefinition ?? string.Empty;
                            resultStoredProcedure.TargetCreateScript = resultStoredProcedure.TargetItem?.RoutineDefinition ?? string.Empty;
                            resultStoredProcedure.Equal = resultStoredProcedure.SourceCreateScript == resultStoredProcedure.TargetCreateScript;
                            taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
                        }

                        taskInfo.Message = "Comparing sequences...";
                        foreach (var resultSequence in this.result.Sequences)
                        {
                            if (resultSequence.SourceItem != null)
                            {
                                resultSequence.SourceCreateScript = scripter.GenerateCreateSequenceScript(resultSequence.SourceItem);
                            }

                            if (resultSequence.TargetItem != null)
                            {
                                resultSequence.TargetCreateScript = scripter.GenerateCreateSequenceScript(resultSequence.TargetItem);
                            }

                            resultSequence.Equal = resultSequence.SourceCreateScript == resultSequence.TargetCreateScript;
                            taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
                        }

                        this.projectService.Project.Result = this.result;

                        return true;
                    })
            });
        }
    }
}
