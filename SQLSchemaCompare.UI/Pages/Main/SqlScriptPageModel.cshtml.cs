using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiCodeX.SQLSchemaCompare.Core.Entities.Api;
using TiCodeX.SQLSchemaCompare.Core.Enums;
using TiCodeX.SQLSchemaCompare.Core.Interfaces.Services;

namespace TiCodeX.SQLSchemaCompare.UI.Pages.Main
{
    /// <summary>
    /// PageModel of the SqlScript page
    /// </summary>
    public class SqlScriptPageModel : PageModel
    {
        private readonly IProjectService projectService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlScriptPageModel"/> class.
        /// </summary>
        /// <param name="projectService">The injected project service</param>
        public SqlScriptPageModel(IProjectService projectService)
        {
            this.projectService = projectService;
        }

        /// <summary>
        /// Get the full script
        /// </summary>
        /// <param name="direction">The source or target</param>
        /// <returns>A JSON object with the full script</returns>
        public ActionResult OnGetFullScript(CompareDirection direction)
        {
            return new JsonResult(new ApiResponse<string>
            {
                Result = direction == CompareDirection.Source
                    ? this.projectService.Project.Result.SourceFullScript
                    : this.projectService.Project.Result.TargetFullScript,
            });
        }

        /// <summary>
        /// Get the full alter script
        /// </summary>
        /// <returns>A JSON object with the full alter script</returns>
        public ActionResult OnGetFullAlterScript()
        {
            return new JsonResult(new ApiResponse<string>
            {
                Result = this.projectService.Project.Result.FullAlterScript,
            });
        }
    }
}