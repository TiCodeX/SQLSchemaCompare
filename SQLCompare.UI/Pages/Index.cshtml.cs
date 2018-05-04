using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace SQLCompare.UI.Pages
{
    /// <summary>
    /// PageModel of the Index page
    /// </summary>
    public class Index : PageModel
    {
        private readonly ILogger<Index> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Index"/> class.
        /// </summary>
        /// <param name="logger">The injected Logger</param>
        public Index(ILogger<Index> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Initialize the state of the page
        /// </summary>
        public void OnGet()
        {
            this.ViewData["Title"] = "Pluto";
        }
    }
}