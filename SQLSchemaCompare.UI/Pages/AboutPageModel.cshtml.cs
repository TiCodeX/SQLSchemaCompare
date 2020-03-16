using System;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using TiCodeX.SQLSchemaCompare.Core.Entities.Api;
using TiCodeX.SQLSchemaCompare.Core.Interfaces;
using TiCodeX.SQLSchemaCompare.Core.Interfaces.Services;

namespace TiCodeX.SQLSchemaCompare.UI.Pages
{
    /// <summary>
    /// PageModel of the about page
    /// </summary>
    public class AboutPageModel : PageModel
    {
        private readonly ILogger logger;
        private readonly IAppGlobals appGlobals;
        private readonly IAppSettingsService appSettingsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AboutPageModel"/> class
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="appGlobals">The injected application globals</param>
        /// <param name="appSettingsService">The injected app settings service</param>
        public AboutPageModel(ILoggerFactory loggerFactory, IAppGlobals appGlobals, IAppSettingsService appSettingsService)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            this.logger = loggerFactory.CreateLogger(nameof(AboutPageModel));
            this.appGlobals = appGlobals;
            this.appSettingsService = appSettingsService;
        }

        /// <summary>
        /// Gets or sets the name of the product
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the application version
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        /// Gets or sets the customer information
        /// </summary>
        public VerifySessionResult CustomerInformation { get; set; }

        /// <summary>
        /// Gets or sets the subscribe URL
        /// </summary>
        public string SubscribeEndpoint { get; set; }

        /// <summary>
        /// Get the about page
        /// </summary>
        public void OnGet()
        {
            this.ProductName = this.appGlobals.ProductName;
            this.AppVersion = this.appGlobals.AppVersion;
            this.CustomerInformation = new VerifySessionResult
            {
                BillingType = BillingType.Perpetual,
            };

            var session = string.Empty;
            try
            {
                session = this.appSettingsService.GetAppSettings().Session;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unable to get app settings");
            }

            this.SubscribeEndpoint = $"{this.appGlobals.SubscribeEndpoint}&s={Uri.EscapeDataString(session)}";
        }
    }
}