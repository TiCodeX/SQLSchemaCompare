using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SQLCompare.Core.Interfaces;
using SQLCompare.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SQLCompare.UI.Pages
{
    /// <summary>
    /// PageModel of the Main page
    /// </summary>
    public class Main : PageModel
    {
        private readonly IProjectService projectService;
        private readonly IDatabaseScripterFactory databaseScripterFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Main"/> class.
        /// </summary>
        /// <param name="projectService">The injected project service</param>
        /// <param name="databaseScripterFactory">The injected database scripter factory</param>
        public Main(IProjectService projectService, IDatabaseScripterFactory databaseScripterFactory)
        {
            this.projectService = projectService;
            this.databaseScripterFactory = databaseScripterFactory;
        }

        /// <summary>
        /// Gets the tables that have the same name
        /// </summary>
        public List<string> SameTables { get; } = new List<string>();

        /// <summary>
        /// Gets the tables only on the source
        /// </summary>
        public List<string> OnlySource { get; } = new List<string>();

        /// <summary>
        /// Gets the tables only on the target
        /// </summary>
        public List<string> OnlyTarget { get; } = new List<string>();

        /// <summary>
        /// Get the Main page
        /// </summary>
        public void OnGet()
        {
            this.OnlyTarget.AddRange(this.projectService.Project.RetrievedTargetDatabase.Tables.Select(x => x.Name).OrderBy(x => x));

            foreach (var sourceTable in this.projectService.Project.RetrievedSourceDatabase.Tables.OrderBy(x => x.Name).ToList())
            {
                if (this.OnlyTarget.Contains(sourceTable.Name))
                {
                    this.SameTables.Add(sourceTable.Name);
                    this.OnlyTarget.Remove(sourceTable.Name);
                }
                else
                {
                    this.OnlySource.Add(sourceTable.Name);
                }
            }
        }

        /// <summary>
        /// Get the SQL script of two tables
        /// </summary>
        /// <param name="request">The requested source and target table</param>
        /// <returns>A JSON object with the source and target SQL scripts</returns>
        public ActionResult OnPostSqlScript([FromBody] dynamic request)
        {
            var requestSourceTable = request.sourceTable.ToString();
            var requestTargetTable = request.targetTable.ToString();

            var sourceTable = this.projectService.Project.RetrievedSourceDatabase.Tables.FirstOrDefault(x =>
                string.Equals(x.Name, requestSourceTable, StringComparison.Ordinal));

            var targetTable = this.projectService.Project.RetrievedTargetDatabase.Tables.FirstOrDefault(x =>
                string.Equals(x.Name, requestTargetTable, StringComparison.Ordinal));

            return new JsonResult(new
            {
                SourceSql = sourceTable != null ? this.databaseScripterFactory.Create(
                    this.projectService.Project.RetrievedSourceDatabase, this.projectService.Project.Options).ScriptCreateTable(sourceTable) : string.Empty,
                TargetSql = targetTable != null ? this.databaseScripterFactory.Create(
                    this.projectService.Project.RetrievedTargetDatabase, this.projectService.Project.Options).ScriptCreateTable(targetTable) : string.Empty,
            });
        }
    }
}