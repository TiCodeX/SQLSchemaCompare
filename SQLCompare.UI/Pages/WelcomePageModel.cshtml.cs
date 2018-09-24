using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SQLCompare.Core.Interfaces;
using SQLCompare.Core.Interfaces.Services;

namespace SQLCompare.UI.Pages
{
    /// <summary>
    /// PageModel of the Welcome page
    /// </summary>
    public class WelcomePageModel : PageModel
    {
        private readonly IAppSettingsService appSettingsService;
        private readonly IAppGlobals appGlobals;

        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomePageModel"/> class.
        /// </summary>
        /// <param name="appSettingsService">The injected app settings service</param>
        /// <param name="appGlobals">The injected app globals</param>
        public WelcomePageModel(IAppSettingsService appSettingsService, IAppGlobals appGlobals)
        {
            this.appSettingsService = appSettingsService;
            this.appGlobals = appGlobals;
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
            this.RecentProjects = settings.RecentProjects.TakeLast(10).Reverse().ToList();
        }
    }
}