using Microsoft.AspNetCore.Mvc.RazorPages;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Interfaces.Services;
using SQLCompare.Infrastructure.DatabaseProviders;
using System.Collections.Generic;

namespace SQLCompare.UI.Pages
{
    /// <summary>
    /// PageModel of the Welcome page
    /// </summary>
    public class Welcome : PageModel
    {
        private readonly IAppSettingsService appSettingsService;
        private readonly IDatabaseService s;

        /// <summary>
        /// Initializes a new instance of the <see cref="Welcome"/> class.
        /// </summary>
        /// <param name="appSettingsService">The injected app settings service</param>
        /// <param name="s">s</param>
        public Welcome(IAppSettingsService appSettingsService, IDatabaseService s)
        {
            this.appSettingsService = appSettingsService;
            this.s = s;
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
            this.s.GetDatabase(new MicrosoftSqlDatabaseProviderOptions() { Database = "brokerpro", Hostname = "localhost\\SQLEXPRESS", UseWindowsAuthentication = true });
            this.RecentProjects = settings.RecentProjects;
        }
    }
}