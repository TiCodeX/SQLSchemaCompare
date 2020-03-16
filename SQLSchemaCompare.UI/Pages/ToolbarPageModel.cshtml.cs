using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using TiCodeX.SQLSchemaCompare.Core.Entities;
using TiCodeX.SQLSchemaCompare.Core.Entities.Api;
using TiCodeX.SQLSchemaCompare.Core.Interfaces;
using TiCodeX.SQLSchemaCompare.Core.Interfaces.Services;
using TiCodeX.SQLSchemaCompare.Services;
using TiCodeX.SQLSchemaCompare.UI.Models;

namespace TiCodeX.SQLSchemaCompare.UI.Pages
{
    /// <summary>
    /// PageModel of the Toolbar
    /// </summary>
    public class ToolbarPageModel : PageModel
    {
        private readonly ILogger logger;
        private readonly IAppGlobals appGlobals;
        private readonly IAppSettingsService appSettingsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarPageModel"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="appGlobals">The injected app globals</param>
        /// <param name="appSettingsService">The injected app settings service</param>
        public ToolbarPageModel(ILoggerFactory loggerFactory, IAppGlobals appGlobals, IAppSettingsService appSettingsService)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            this.logger = loggerFactory.CreateLogger(nameof(ToolbarPageModel));
            this.appGlobals = appGlobals;
            this.appSettingsService = appSettingsService;
        }

        /// <summary>
        /// Gets or sets the account email
        /// </summary>
        public string AccountEmail { get; set; }

        /// <summary>
        /// Gets a value indicating whether the feedback menu should be highlighted
        /// </summary>
        public bool HighlighFeedbackMenu { get; private set; } = false;

        /// <summary>
        /// Gets or sets the account URL
        /// </summary>
        public string MyAccountEndpoint { get; set; }

        /// <summary>
        /// Get the welcome page
        /// </summary>
        public void OnGet()
        {
            this.AccountEmail = "User";

            var session = string.Empty;
            string feedbackSent = null;
            try
            {
                feedbackSent = this.appSettingsService.GetAppSettings().FeedbackSent;
                session = this.appSettingsService.GetAppSettings().Session;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unable to get app settings");
            }

            if (feedbackSent == null)
            {
                this.HighlighFeedbackMenu = true;
            }

            this.MyAccountEndpoint = $"{this.appGlobals.MyAccountEndpoint}&s={Uri.EscapeDataString(session)}";
        }
    }
}