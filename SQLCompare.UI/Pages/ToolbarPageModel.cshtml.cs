using System;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Interfaces;
using SQLCompare.Core.Interfaces.Services;

namespace SQLCompare.UI.Pages
{
    /// <summary>
    /// PageModel of the Toolbar
    /// </summary>
    public class ToolbarPageModel : PageModel
    {
        private readonly ILogger logger;
        private readonly IAppGlobals appGlobals;
        private readonly IAccountService accountService;
        private readonly IAppSettingsService appSettingsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarPageModel"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="appGlobals">The injected app globals</param>
        /// <param name="accountService">The injected account service</param>
        /// <param name="appSettingsService">The injected app settings service</param>
        public ToolbarPageModel(ILoggerFactory loggerFactory, IAppGlobals appGlobals, IAccountService accountService, IAppSettingsService appSettingsService)
        {
            this.logger = loggerFactory.CreateLogger(nameof(Index));
            this.appGlobals = appGlobals;
            this.accountService = accountService;
            this.appSettingsService = appSettingsService;
        }

        /// <summary>
        /// Gets or sets the account email
        /// </summary>
        public string AccountEmail { get; set; }

        /// <summary>
        /// Gets or sets the account URL
        /// </summary>
        public string MyAccountEndpoint { get; set; }

        /// <summary>
        /// Get the welcome page
        /// </summary>
        public void OnGet()
        {
            this.AccountEmail = this.accountService.CustomerInformation.Email;

            var session = string.Empty;
            try
            {
                session = this.appSettingsService.GetAppSettings().Session;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unable to get app settings");
            }

            this.MyAccountEndpoint = $"{this.appGlobals.MyAccountEndpoint}&s={session}";
        }
    }
}