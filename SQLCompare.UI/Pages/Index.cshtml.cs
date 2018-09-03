using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Api;
using SQLCompare.Core.Interfaces;
using SQLCompare.Core.Interfaces.Services;

namespace SQLCompare.UI.Pages
{
    /// <summary>
    /// PageModel of the Index page
    /// </summary>
    public class Index : PageModel
    {
        private readonly IAppGlobals appGlobals;
        private readonly ILocalizationService localizationService;
        private readonly ILogger logger;
        private readonly IAccountService accountService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Index"/> class.
        /// </summary>
        /// <param name="appGlobals">The injected app globals</param>
        /// <param name="localizationService">The injected LocalizationService</param>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="accountService">The injected account service</param>
        public Index(IAppGlobals appGlobals, ILocalizationService localizationService, ILoggerFactory loggerFactory, IAccountService accountService)
        {
            this.appGlobals = appGlobals;
            this.localizationService = localizationService;
            this.logger = loggerFactory.CreateLogger(nameof(Index));
            this.accountService = accountService;
        }

        /// <summary>
        /// Gets or sets the title of the page
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Get the localization
        /// </summary>
        /// <returns>A dictionary of the tokens as keys and the related localized strings</returns>
        public IActionResult OnGetLoadLocalization()
        {
            return new JsonResult(new ApiResponse<Dictionary<string, string>>
            {
                Result = this.localizationService.GetLocalizationDictionary()
            });
        }

        /// <summary>
        /// Initialize the state of the page
        /// </summary>
        /// <returns>The page</returns>
        public IActionResult OnGet()
        {
            this.Title = $"{this.appGlobals.ProductName} - {this.appGlobals.CompanyName}";
            return this.Page();
        }
    }
}