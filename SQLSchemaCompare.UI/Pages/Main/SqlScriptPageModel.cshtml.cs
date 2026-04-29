namespace TiCodeX.SQLSchemaCompare.UI.Pages.Main;

/// <summary>
/// PageModel of the SqlScript page
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SqlScriptPageModel"/> class.
/// </remarks>
/// <param name="projectService">The injected project service</param>
public class SqlScriptPageModel(IProjectService projectService) : PageModel
{
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
                ? projectService.Project.Result.SourceFullScript
                : projectService.Project.Result.TargetFullScript,
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
            Result = projectService.Project.Result.FullAlterScript,
        });
    }
}
