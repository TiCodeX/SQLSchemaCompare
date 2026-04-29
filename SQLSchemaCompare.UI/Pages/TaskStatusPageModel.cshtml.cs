namespace TiCodeX.SQLSchemaCompare.UI.Pages;

/// <summary>
/// PageModel of the TaskStatus page
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TaskStatusPageModel"/> class.
/// </remarks>
/// <param name="taskService">The injected task service</param>
public class TaskStatusPageModel(ITaskService taskService) : PageModel
{
    /// <summary>
    /// Gets or sets the current list of TaskInfo
    /// </summary>
    public IReadOnlyCollection<TaskInfo> TaskInfos { get; set; }

    /// <summary>
    /// Get the TaskStatus page for the current Task
    /// </summary>
    public void OnGet()
    {
        this.TaskInfos = taskService.CurrentTaskInfos;
    }

    /// <summary>
    /// Aborts the current running task
    /// </summary>
    /// <returns>The resulting json</returns>
    public IActionResult OnGetAbortTask()
    {
        taskService.Abort();
        return new JsonResult(new ApiResponse());
    }
}
