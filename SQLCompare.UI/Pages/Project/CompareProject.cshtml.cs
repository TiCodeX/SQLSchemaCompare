using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Enums;
using SQLCompare.Core.Interfaces.Services;
using SQLCompare.UI.Models.Project;
using System;

namespace SQLCompare.UI.Pages.Project
{
    /// <summary>
    /// PageModel of the CompareProject page
    /// </summary>
    public class CompareProject : PageModel
    {
        private readonly IProjectService projectService;
        private readonly IDatabaseService databaseService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareProject"/> class.
        /// </summary>
        /// <param name="projectService">The injected project service</param>
        /// <param name="databaseService">The injected database service</param>
        public CompareProject(IProjectService projectService, IDatabaseService databaseService)
        {
            this.projectService = projectService;
            this.databaseService = databaseService;
        }

        /// <summary>
        /// Get the CompareProject page for a new Project
        /// </summary>
        public void OnGet()
        {
            this.projectService.NewProject();
        }

        /// <summary>
        /// Get the CompareProject page
        /// </summary>
        /// <param name="projectFile">The project file to load</param>
        public void OnPostLoad([FromBody] string projectFile)
        {
            this.projectService.LoadProject(projectFile);
        }

        /// <summary>
        /// Perform the database comparison
        /// </summary>
        /// <param name="options">The project options</param>
        public void OnPostCompare([FromBody] CompareProjectOptions options)
        {
            this.projectService.Project.SourceProviderOptions = this.GetDatabaseProviderOptions(
                options.SourceDatabaseType,
                options.SourceHostname,
                options.SourceUsername,
                options.SourcePassword);

            this.projectService.Project.TargetProviderOptions = this.GetDatabaseProviderOptions(
                options.TargetDatabaseType,
                options.TargetHostname,
                options.TargetUsername,
                options.TargetPassword);

            // Start showing modal dialog with progress bar
            this.projectService.Project.RetrievedSourceDatabase = this.databaseService.GetDatabase(this.projectService.Project.SourceProviderOptions);

            this.projectService.Project.RetrievedTargetDatabase = this.databaseService.GetDatabase(this.projectService.Project.TargetProviderOptions);

            // Perform database compare
            // Close modal dialog
            // Show main page with result
        }

        /// <summary>
        /// Connect to the server to retrieve the available databases
        /// </summary>
        /// <param name="sourceOptions">The database provider options</param>
        /// <returns>The list of database names in JSON</returns>
        public ActionResult OnPostLoadSourceDatabaseList([FromBody] CompareProjectOptions sourceOptions)
        {
            var dbProviderOptions = this.GetDatabaseProviderOptions(
                sourceOptions.SourceDatabaseType,
                sourceOptions.SourceHostname,
                sourceOptions.SourceUsername,
                sourceOptions.SourcePassword);
            return new JsonResult(this.databaseService.ListDatabases(dbProviderOptions));
        }

        /// <summary>
        /// Connect to the server to retrieve the available databases
        /// </summary>
        /// <param name="targetOptions">The database provider options</param>
        /// <returns>The list of database names in JSON</returns>
        public ActionResult OnPostLoadTargetDatabaseList([FromBody] CompareProjectOptions targetOptions)
        {
            var dbProviderOptions = this.GetDatabaseProviderOptions(
                targetOptions.TargetDatabaseType,
                targetOptions.TargetHostname,
                targetOptions.TargetUsername,
                targetOptions.TargetPassword);
            return new JsonResult(this.databaseService.ListDatabases(dbProviderOptions));
        }

        // TODO: move somewhere else and add missing parameters
        private DatabaseProviderOptions GetDatabaseProviderOptions(DatabaseType type, string hostname, string username, string password)
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
                    };
                case DatabaseType.MySql:
                    return new MySqlDatabaseProviderOptions
                    {
                        Hostname = hostname,
                        Username = username,
                        Password = password,
                        UseSSL = false,
                    };
                case DatabaseType.PostgreSql:
                    return new PostgreSqlDatabaseProviderOptions
                    {
                        Hostname = hostname,
                        Username = username,
                        Password = password,
                    };
                default:
                    throw new ArgumentException("Unknown Database type");
            }
        }
    }
}