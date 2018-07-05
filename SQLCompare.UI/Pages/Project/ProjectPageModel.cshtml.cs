using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SQLCompare.Core.Entities;
using SQLCompare.Core.Entities.Compare;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Entities.Project;
using SQLCompare.Core.Enums;
using SQLCompare.Core.Interfaces;
using SQLCompare.Core.Interfaces.Services;
using SQLCompare.UI.Enums;
using SQLCompare.UI.Models.Project;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SQLCompare.UI.Pages.Project
{
    /// <summary>
    /// PageModel of the Project page
    /// </summary>
    public class ProjectPageModel : PageModel
    {
        private readonly IAppSettingsService appSettingsService;
        private readonly IProjectService projectService;
        private readonly IDatabaseService databaseService;
        private readonly IDatabaseCompareService databaseCompareService;
        private readonly IDatabaseScripterFactory databaseScripterFactory;
        private readonly ITaskService taskService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectPageModel"/> class.
        /// </summary>
        /// <param name="appSettingsService">The injected app settings service</param>
        /// <param name="projectService">The injected project service</param>
        /// <param name="databaseService">The injected database service</param>
        /// <param name="databaseCompareService">The injected database compare service</param>
        /// <param name="databaseScripterFactory">The injected database scripter factory</param>
        /// <param name="taskService">The injected task service</param>
        public ProjectPageModel(
            IAppSettingsService appSettingsService,
            IProjectService projectService,
            IDatabaseService databaseService,
            IDatabaseCompareService databaseCompareService,
            IDatabaseScripterFactory databaseScripterFactory,
            ITaskService taskService)
        {
            this.appSettingsService = appSettingsService;
            this.projectService = projectService;
            this.databaseService = databaseService;
            this.databaseCompareService = databaseCompareService;
            this.databaseScripterFactory = databaseScripterFactory;
            this.taskService = taskService;
        }

        /// <summary>
        /// Gets the current project
        /// </summary>
        public CompareProject Project { get; private set; }

        /// <summary>
        /// Get the Project page for the current Project
        /// </summary>
        public void OnGet()
        {
            this.Project = this.projectService.Project;
        }

        /// <summary>
        /// Get the Project page for a new Project
        /// </summary>
        public void OnGetNewProject()
        {
            this.projectService.NewProject();
            this.Project = this.projectService.Project;
        }

        /// <summary>
        /// Get the Project page
        /// </summary>
        /// <param name="projectFile">The project file to load</param>
        public void OnPostLoadProject([FromBody] string projectFile)
        {
            this.projectService.LoadProject(projectFile);

            var appSettings = this.appSettingsService.GetAppSettings();
            appSettings.RecentProjects.Remove(projectFile);
            appSettings.RecentProjects.Add(projectFile);
            this.appSettingsService.SaveAppSettings();

            this.Project = this.projectService.Project;
        }

        /// <summary>
        /// Save the project
        /// </summary>
        /// <param name="filename">The filename</param>
        /// <returns>TODO: boh</returns>
        public ActionResult OnPostSaveProject([FromBody] string filename)
        {
            this.projectService.SaveProject(filename);

            var appSettings = this.appSettingsService.GetAppSettings();
            appSettings.RecentProjects.Remove(filename);
            appSettings.RecentProjects.Add(filename);
            this.appSettingsService.SaveAppSettings();

            return new JsonResult(null);
        }

        /// <summary>
        /// Close the project
        /// </summary>
        /// <returns>TODO: boh</returns>
        public ActionResult OnGetCloseProject()
        {
            this.projectService.CloseProject();

            return new JsonResult(null);
        }

        /// <summary>
        /// Edit the project with the new values from the UI
        /// </summary>
        /// <param name="options">The project options</param>
        /// <returns>TODO: boh</returns>
        public ActionResult OnPostEditProject([FromBody] CompareProjectOptions options)
        {
            this.projectService.Project.SourceProviderOptions = this.GetDatabaseProviderOptions(options, CompareDirection.Source);
            this.projectService.Project.TargetProviderOptions = this.GetDatabaseProviderOptions(options, CompareDirection.Target);

            return new JsonResult(null);
        }

        /// <summary>
        /// Perform the database comparison
        /// </summary>
        /// <returns>TODO: boh</returns>
        public ActionResult OnGetStartCompare()
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
                        this.projectService.Project.RetrievedSourceDatabase = this.databaseService.GetDatabase(
                            this.projectService.Project.SourceProviderOptions);
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
                        this.projectService.Project.RetrievedTargetDatabase = this.databaseService.GetDatabase(
                            this.projectService.Project.TargetProviderOptions);
                        taskInfo.Percentage = 100;

                        return true;
                    }),
                new TaskWork(
                    new TaskInfo("Mapping databases"),
                    false,
                    taskInfo =>
                    {
                        // TODO: Perform mapping with user config from project
                        var result = new CompareResult();

                        foreach (var table in this.projectService.Project.RetrievedSourceDatabase.Tables)
                        {
                            result.Tables.Add(new CompareResultItem<ABaseDbTable>
                            {
                                SourceItem = table,
                                TargetItem = this.projectService.Project.RetrievedTargetDatabase.Tables.FirstOrDefault(x => x.Name == table.Name)
                            });
                        }

                        foreach (var table in this.projectService.Project.RetrievedTargetDatabase.Tables.Where(x => result.Tables.All(y => y.SourceItem.Name != x.Name)).ToList())
                        {
                            result.Tables.Add(new CompareResultItem<ABaseDbTable>
                            {
                                TargetItem = table
                            });
                        }

                        taskInfo.Percentage = 50;

                        foreach (var view in this.projectService.Project.RetrievedSourceDatabase.Views)
                        {
                            result.Views.Add(new CompareResultItem<ABaseDbView>
                            {
                                SourceItem = view,
                                TargetItem = this.projectService.Project.RetrievedTargetDatabase.Views.FirstOrDefault(x => x.Name == view.Name)
                            });
                        }

                        foreach (var view in this.projectService.Project.RetrievedTargetDatabase.Views.Where(x => result.Views.All(y => y.SourceItem.Name != x.Name)).ToList())
                        {
                            result.Views.Add(new CompareResultItem<ABaseDbView>
                            {
                                TargetItem = view
                            });
                        }

                        this.projectService.Project.Result = result;

                        taskInfo.Percentage = 100;

                        return true;
                    }),
                new TaskWork(
                    new TaskInfo("Comparing databases"),
                    false,
                    taskInfo =>
                    {
                        taskInfo.Message = "Comparing tables...";
                        var totalItems = this.projectService.Project.Result.Tables.Count + this.projectService.Project.Result.Views.Count;
                        var processedItems = 1;
                        foreach (var resultTable in this.projectService.Project.Result.Tables)
                        {
                            resultTable.Equal = this.databaseCompareService.CompareTable(resultTable.SourceItem, resultTable.TargetItem);

                            if (resultTable.SourceItem != null)
                            {
                                resultTable.SourceCreateScript = this.databaseScripterFactory.Create(
                                        this.projectService.Project.RetrievedSourceDatabase,
                                        this.projectService.Project.Options)
                                    .GenerateCreateTableScript(resultTable.SourceItem);
                            }

                            if (resultTable.TargetItem != null)
                            {
                                resultTable.TargetCreateScript = this.databaseScripterFactory.Create(
                                        this.projectService.Project.RetrievedTargetDatabase,
                                        this.projectService.Project.Options)
                                    .GenerateCreateTableScript(resultTable.TargetItem);
                            }

                            // Workaround: compare the generated sql script
                            resultTable.Equal = resultTable.SourceCreateScript == resultTable.TargetCreateScript;

                            taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
                        }

                        taskInfo.Message = "Comparing views...";
                        foreach (var resultView in this.projectService.Project.Result.Views)
                        {
                            resultView.Equal = this.databaseCompareService.CompareView(resultView.SourceItem, resultView.TargetItem);

                            if (resultView.SourceItem != null)
                            {
                                resultView.SourceCreateScript = resultView.SourceItem.ViewDefinition;
                            }

                            if (resultView.TargetItem != null)
                            {
                                resultView.TargetCreateScript = resultView.TargetItem.ViewDefinition;
                            }

                            taskInfo.Percentage = (short)((double)processedItems++ / totalItems * 100);
                        }

                        return true;
                    })
            });

            return new JsonResult(null);
        }

        /// <summary>
        /// Connect to the server to retrieve the available databases
        /// </summary>
        /// <param name="options">The database provider options</param>
        /// <returns>The list of database names in JSON</returns>
        public ActionResult OnPostLoadDatabaseList([FromBody] CompareProjectOptions options)
        {
            return new JsonResult(this.databaseService.ListDatabases(this.GetDatabaseProviderOptions(options, options.Direction)));
        }

        // TODO: move somewhere else and add missing parameters
        private ADatabaseProviderOptions GetDatabaseProviderOptions(CompareProjectOptions options, CompareDirection direction)
        {
            var type = direction == CompareDirection.Source ? options.SourceDatabaseType : options.TargetDatabaseType;
            var hostname = direction == CompareDirection.Source ? options.SourceHostname : options.TargetHostname;
            var username = direction == CompareDirection.Source ? options.SourceUsername : options.TargetUsername;
            var password = direction == CompareDirection.Source ? options.SourcePassword : options.TargetPassword;
            var useWindowsAuthentication = direction == CompareDirection.Source ? options.SourceUseWindowsAuthentication : options.TargetUseWindowsAuthentication;
            var useSSL = direction == CompareDirection.Source ? options.SourceUseSSL : options.TargetUseSSL;
            var database = direction == CompareDirection.Source ? options.SourceDatabase : options.TargetDatabase;

            switch (type)
            {
                case DatabaseType.MicrosoftSql:
                    return new MicrosoftSqlDatabaseProviderOptions
                    {
                        Hostname = hostname,
                        Username = username,
                        Password = password,
                        UseWindowsAuthentication = useWindowsAuthentication,
                        UseSSL = useSSL,
                        Database = database,
                    };
                case DatabaseType.MySql:
                    return new MySqlDatabaseProviderOptions
                    {
                        Hostname = hostname,
                        Username = username,
                        Password = password,
                        UseSSL = useSSL,
                        Database = database,
                    };
                case DatabaseType.PostgreSql:
                    return new PostgreSqlDatabaseProviderOptions
                    {
                        Hostname = hostname,
                        Username = username,
                        Password = password,
                        Database = database,
                    };
                default:
                    throw new ArgumentException("Unknown Database type");
            }
        }
    }
}