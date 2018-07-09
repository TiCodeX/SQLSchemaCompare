using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SQLCompare.Core.Interfaces.Services;

namespace SQLCompare.UI.Pages
{
    /// <summary>
    /// PageModel of the Welcome page
    /// </summary>
    public class WelcomePageModel : PageModel
    {
        private readonly IAppSettingsService appSettingsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomePageModel"/> class.
        /// </summary>
        /// <param name="appSettingsService">The injected app settings service</param>
        public WelcomePageModel(IAppSettingsService appSettingsService)
        {
            this.appSettingsService = appSettingsService;
        }

        /// <summary>
        /// Gets the recently opened projects
        /// </summary>
        public List<string> RecentProjects { get; internal set; }

        /// <summary>
        /// Get the welcome page
        /// </summary>
        public void OnGet()
        {
            var settings = this.appSettingsService.GetAppSettings();

            this.RecentProjects = settings.RecentProjects;
        }
    }
}