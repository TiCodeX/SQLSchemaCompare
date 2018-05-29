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
        private readonly IDatabaseService databaseService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareProject"/> class.
        /// </summary>
        /// <param name="databaseService">The injected database service</param>
        public CompareProject(IDatabaseService databaseService)
        {
            this.databaseService = databaseService;
        }

        /// <summary>
        /// Get the CompareProject page
        /// </summary>
        /// <param name="projectFile">The project file to load</param>
        public void OnPost([FromBody] string projectFile)
        {
            this.ViewData["File"] = projectFile;
        }

        /// <summary>
        /// Connect to the server to retrieve the available databases
        /// </summary>
        /// <param name="sourceDatabaseProviderOption">The database provider options</param>
        /// <returns>The list of database names in JSON</returns>
        public ActionResult OnPostLoadSourceDatabaseList([FromBody] SourceDatabaseProviderOptions sourceDatabaseProviderOption)
        {
            var dbProviderOptions = this.GetDatabaseProviderOptions(
                sourceDatabaseProviderOption.SourceDatabaseType,
                sourceDatabaseProviderOption.SourceHostname,
                sourceDatabaseProviderOption.SourceUsername,
                sourceDatabaseProviderOption.SourcePassword);
            return new JsonResult(this.databaseService.ListDatabases(dbProviderOptions));
        }

        /// <summary>
        /// Connect to the server to retrieve the available databases
        /// </summary>
        /// <param name="targetDatabaseProviderOption">The database provider options</param>
        /// <returns>The list of database names in JSON</returns>
        public ActionResult OnPostLoadTargetDatabaseList([FromBody] TargetDatabaseProviderOptions targetDatabaseProviderOption)
        {
            var dbProviderOptions = this.GetDatabaseProviderOptions(
                targetDatabaseProviderOption.TargetDatabaseType,
                targetDatabaseProviderOption.TargetHostname,
                targetDatabaseProviderOption.TargetUsername,
                targetDatabaseProviderOption.TargetPassword);
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