using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SQLCompare.Core.Entities;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Enums;
using SQLCompare.Core.Interfaces.Services;
using SQLCompare.UI.Enums;
using SQLCompare.UI.Models.Project;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SQLCompare.UI.Pages.Project
{
    /// <summary>
    /// PageModel of the CompareProject page
    /// </summary>
    public class CompareProject : PageModel
    {
        private readonly IAppSettingsService appSettingsService;
        private readonly IProjectService projectService;
        private readonly IDatabaseService databaseService;
        private readonly ITaskService taskService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareProject"/> class.
        /// </summary>
        /// <param name="appSettingsService">The injected app settings service</param>
        /// <param name="projectService">The injected project service</param>
        /// <param name="databaseService">The injected database service</param>
        /// <param name="taskService">The injected task service</param>
        public CompareProject(
            IAppSettingsService appSettingsService,
            IProjectService projectService,
            IDatabaseService databaseService,
            ITaskService taskService)
        {
            this.appSettingsService = appSettingsService;
            this.projectService = projectService;
            this.databaseService = databaseService;
            this.taskService = taskService;
        }

        /// <summary>
        /// Gets the current project
        /// </summary>
        public Core.Entities.Project.CompareProject Project { get; private set; }

        /// <summary>
        /// Get the CompareProject page for the current Project
        /// </summary>
        public void OnGet()
        {
            this.Project = this.projectService.Project;
        }

        /// <summary>
        /// Get the CompareProject page for a new Project
        /// </summary>
        public void OnGetNewProject()
        {
            this.projectService.NewProject();
            this.Project = this.projectService.Project;
        }

        /// <summary>
        /// Get the CompareProject page
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
        /// <param name="options">The project options</param>
        /// <returns>TODO: boh</returns>
        public ActionResult OnPostSave([FromBody] CompareProjectOptions options)
        {
            this.projectService.Project.SourceProviderOptions = this.GetDatabaseProviderOptions(
                options.SourceDatabaseType,
                options.SourceHostname,
                options.SourceUsername,
                options.SourcePassword,
                options.SourceDatabase);

            this.projectService.Project.TargetProviderOptions = this.GetDatabaseProviderOptions(
                options.TargetDatabaseType,
                options.TargetHostname,
                options.TargetUsername,
                options.TargetPassword,
                options.TargetDatabase);

            this.projectService.SaveProject(options.SaveProjectFilename);

            var appSettings = this.appSettingsService.GetAppSettings();
            appSettings.RecentProjects.Remove(options.SaveProjectFilename);
            appSettings.RecentProjects.Add(options.SaveProjectFilename);
            this.appSettingsService.SaveAppSettings();

            return new JsonResult(null);
        }

        /// <summary>
        /// Perform the database comparison
        /// </summary>
        /// <param name="options">The project options</param>
        /// <returns>TODO: boh</returns>
        public ActionResult OnPostCompare([FromBody] CompareProjectOptions options)
        {
            this.projectService.Project.SourceProviderOptions = this.GetDatabaseProviderOptions(
                options.SourceDatabaseType,
                options.SourceHostname,
                options.SourceUsername,
                options.SourcePassword,
                options.SourceDatabase);

            this.projectService.Project.TargetProviderOptions = this.GetDatabaseProviderOptions(
                options.TargetDatabaseType,
                options.TargetHostname,
                options.TargetUsername,
                options.TargetPassword,
                options.TargetDatabase);

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
                            Thread.Sleep(TimeSpan.FromSeconds(1));
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
                            Thread.Sleep(TimeSpan.FromMilliseconds(500));
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
                        for (short i = 1; i <= 10; i++)
                        {
                            Thread.Sleep(TimeSpan.FromMilliseconds(100));
                            taskInfo.Percentage = (short)(i * 10);
                        }
                        return true;
                    }),
                new TaskWork(
                    new TaskInfo("Comparing databases"),
                    false,
                    taskInfo =>
                    {
                        for (short i = 1; i <= 10; i++)
                        {
                            Thread.Sleep(TimeSpan.FromMilliseconds(200));
                            taskInfo.Percentage = (short)(i * 10);
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
            ADatabaseProviderOptions dbProviderOptions;
            if (options.Direction == CompareDirection.Source)
            {
                dbProviderOptions = this.GetDatabaseProviderOptions(
                    options.SourceDatabaseType,
                    options.SourceHostname,
                    options.SourceUsername,
                    options.SourcePassword);
            }
            else
            {
                dbProviderOptions = this.GetDatabaseProviderOptions(
                    options.TargetDatabaseType,
                    options.TargetHostname,
                    options.TargetUsername,
                    options.TargetPassword);
            }

            return new JsonResult(this.databaseService.ListDatabases(dbProviderOptions));
        }

        // TODO: move somewhere else and add missing parameters
        private ADatabaseProviderOptions GetDatabaseProviderOptions(DatabaseType type, string hostname, string username, string password, string database = "")
        {
            switch (type)
            {
                case DatabaseType.MicrosoftSql:
                    return new MicrosoftSqlDatabaseProviderOptions
                    {
                        Hostname = hostname,
                        Username = username,
                        Password = password,
                        UseWindowsAuthentication = true,
                        Database = database,
                    };
                case DatabaseType.MySql:
                    return new MySqlDatabaseProviderOptions
                    {
                        Hostname = hostname,
                        Username = username,
                        Password = password,
                        UseSSL = false,
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