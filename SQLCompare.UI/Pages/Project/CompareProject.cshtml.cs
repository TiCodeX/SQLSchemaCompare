using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Enums;
using SQLCompare.Core.Interfaces.Services;
using SQLCompare.UI.Enums;
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
        /// Gets the current project
        /// </summary>
        public Core.Entities.Project.CompareProject Project { get; private set; }

        /// <summary>
        /// Get the CompareProject page for a new Project
        /// </summary>
        public void OnGetNew()
        {
            this.projectService.NewProject();
            this.Project = this.projectService.Project;
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

            // Start showing modal dialog with progress bar
            this.projectService.Project.RetrievedSourceDatabase = this.databaseService.GetDatabase(this.projectService.Project.SourceProviderOptions);

            this.projectService.Project.RetrievedTargetDatabase = this.databaseService.GetDatabase(this.projectService.Project.TargetProviderOptions);

            // Perform database compare
            // Close modal dialog
            // Show main page with result
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