using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SQLCompare.Core.Entities;
using SQLCompare.Core.Interfaces;
using SQLCompare.Core.Interfaces.Services;

namespace SQLCompare.UI.Pages
{
    /// <summary>
    /// PageModel of the Welcome page
    /// </summary>
    public class Welcome : PageModel
    {
        private IAppSettingsService appSettingsService;
        private IAppGlobals appGlobals;

        /// <summary>
        /// Initializes a new instance of the <see cref="Welcome"/> class.
        /// </summary>
        /// <param name="appSettingsService">The injected app settings service</param>
        /// <param name="appGlobals">The injected application global constants</param>
        public Welcome(IAppSettingsService appSettingsService, IAppGlobals appGlobals)
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
            this.ViewData["Title"] = $"Welcome to {this.appGlobals.CompanyName} {this.appGlobals.ProductName}";
            AppSettings settings = this.appSettingsService.GetAppSettings();

            this.RecentProjects = settings.RecentProjects;
        }
    }
}