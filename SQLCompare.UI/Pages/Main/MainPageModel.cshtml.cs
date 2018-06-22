using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SQLCompare.Core.Entities.Compare;
using SQLCompare.Core.Interfaces.Services;
using System;
using System.Linq;

namespace SQLCompare.UI.Pages.Main
{
    /// <summary>
    /// PageModel of the Main page
    /// </summary>
    public class MainPageModel : PageModel
    {
        private readonly IProjectService projectService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPageModel"/> class.
        /// </summary>
        /// <param name="projectService">The injected project service</param>
        public MainPageModel(IProjectService projectService)
        {
            this.projectService = projectService;
        }

        /// <summary>
        /// Gets or sets the compare result of the project
        /// </summary>
        public CompareResult CompareResult { get; set; }

        /// <summary>
        /// Get the Main page
        /// </summary>
        public void OnGet()
        {
            this.CompareResult = this.projectService.Project.Result;
        }

        /// <summary>
        /// Get the create script of two items
        /// </summary>
        /// <param name="id">The requested compare result item</param>
        /// <returns>A JSON object with the source and target create scripts</returns>
        public ActionResult OnGetCreateScript(Guid id)
        {
            // TODO: get a generic result item (table, view, role,...)
            var resultItem = this.projectService.Project.Result.Tables.FirstOrDefault(x => x.Id == id);

            return new JsonResult(new
            {
                SourceSql = resultItem?.SourceCreateScript,
                TargetSql = resultItem?.TargetCreateScript
            });
        }
    }
}