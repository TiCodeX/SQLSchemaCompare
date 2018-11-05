using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SQLCompare.Core.Entities;
using SQLCompare.Core.Entities.Api;
using SQLCompare.Core.Interfaces.Services;

namespace SQLCompare.UI.Pages
{
    /// <summary>
    /// PageModel of the TaskStatus page
    /// </summary>
    public class TaskStatusPageModel : PageModel
    {
        private readonly ITaskService taskService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskStatusPageModel"/> class.
        /// </summary>
        /// <param name="taskService">The injected task service</param>
        public TaskStatusPageModel(ITaskService taskService)
        {
            this.taskService = taskService;
        }

        /// <summary>
        /// Gets or sets the current list of TaskInfo
        /// </summary>
        public IReadOnlyCollection<TaskInfo> TaskInfos { get; set; }

        /// <summary>
        /// Get the TaskStatus page for the current Task
        /// </summary>
        public void OnGet()
        {
            this.TaskInfos = this.taskService.CurrentTaskInfos;
        }

        /// <summary>
        /// Aborts the current running task
        /// </summary>
        /// <returns>The resulting json</returns>
        public IActionResult OnGetAbortTask()
        {
            this.taskService.Abort();
            return new JsonResult(new ApiResponse());
        }
    }
}