using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SQLCompare.UI.Pages.Project
{
    /// <summary>
    /// PageModel of the CompareProject page
    /// </summary>
    public class CompareProject : PageModel
    {
        /// <summary>
        /// Get the CompareProject page
        /// </summary>
        /// <param name="projectFile">The project file to load</param>
        public void OnPost([FromBody] string projectFile)
        {
            this.ViewData["File"] = projectFile;
        }
    }
}