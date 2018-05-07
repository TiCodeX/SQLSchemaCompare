using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SQLCompare.Core.Entities;
using SQLCompare.Core.Interfaces.Services;

namespace SQLCompare.UI.Pages
{
    /// <summary>
    /// PageModel of the Welcome page
    /// </summary>
    public class Welcome : PageModel
    {
        private IAppSettingsService appSettingsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Welcome"/> class.
        /// </summary>
        /// <param name="appSettingsService">The injected app settings service</param>
        public Welcome(IAppSettingsService appSettingsService)
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
            AppSettings settings = this.appSettingsService.GetAppSettings();

            this.RecentProjects = settings.RecentProjects;
        }
    }
}