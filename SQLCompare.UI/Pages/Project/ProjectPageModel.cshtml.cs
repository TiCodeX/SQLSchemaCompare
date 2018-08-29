using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Api;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Entities.Project;
using SQLCompare.Core.Enums;
using SQLCompare.Core.Interfaces.Services;
using SQLCompare.UI.Enums;
using SQLCompare.UI.Models.Project;

namespace SQLCompare.UI.Pages.Project
{
    /// <summary>
    /// PageModel of the Project page
    /// </summary>
    public class ProjectPageModel : PageModel
    {
        private readonly ILogger logger;
        private readonly IAppSettingsService appSettingsService;
        private readonly IProjectService projectService;
        private readonly IDatabaseService databaseService;
        private readonly IDatabaseCompareService databaseCompareService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectPageModel"/> class.
        /// </summary>
        /// <param name="logger">The injected logger</param>
        /// <param name="appSettingsService">The injected app settings service</param>
        /// <param name="projectService">The injected project service</param>
        /// <param name="databaseService">The injected database service</param>
        /// <param name="databaseCompareService">The injected database compare service</param>
        public ProjectPageModel(
            ILogger<ProjectPageModel> logger,
            IAppSettingsService appSettingsService,
            IProjectService projectService,
            IDatabaseService databaseService,
            IDatabaseCompareService databaseCompareService)
        {
            this.logger = logger;
            this.appSettingsService = appSettingsService;
            this.projectService = projectService;
            this.databaseService = databaseService;
            this.databaseCompareService = databaseCompareService;
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

            return new JsonResult(new ApiResponse());
        }

        /// <summary>
        /// Close the project
        /// </summary>
        /// <returns>TODO: boh</returns>
        public ActionResult OnGetCloseProject()
        {
            this.projectService.CloseProject();

            return new JsonResult(new ApiResponse());
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
            this.projectService.Project.Options = options.ProjectOptions;

            return new JsonResult(new ApiResponse());
        }

        /// <summary>
        /// Perform the database comparison
        /// </summary>
        /// <returns>TODO: boh</returns>
        public ActionResult OnGetStartCompare()
        {
            this.databaseCompareService.StartCompare();

            return new JsonResult(new ApiResponse());
        }

        /// <summary>
        /// Connect to the server to retrieve the available databases
        /// </summary>
        /// <param name="options">The database provider options</param>
        /// <returns>The list of database names in JSON</returns>
        public ActionResult OnPostLoadDatabaseList([FromBody] CompareProjectOptions options)
        {
            return new JsonResult(new ApiResponse<List<string>>
            {
                Result = this.databaseService.ListDatabases(this.GetDatabaseProviderOptions(options, options.Direction))
            });
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
                    this.logger.LogError($"Unknown Database type: {type}");
                    throw new ArgumentException("Unknown Database type");
            }
        }
    }
}