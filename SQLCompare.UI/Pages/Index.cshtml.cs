using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Index"/> class.
        /// </summary>
        /// <param name="appGlobals">The injected app globals</param>
        /// <param name="localizationService">The injected LocalizationService</param>
        /// <param name="logger">The injected logger</param>
        public Index(IAppGlobals appGlobals, ILocalizationService localizationService, ILogger<Index> logger)
        {
            this.appGlobals = appGlobals;
            this.localizationService = localizationService;
            this.logger = logger;
        }

        /// <summary>
        /// Get the localization
        /// </summary>
        /// <returns>A dictionary of the tokens as keys and the related localized strings</returns>
        public ActionResult OnGetLoadLocalization()
        {
            return new JsonResult(this.localizationService.GetLocalizationDictionary());
        }

        /// <summary>
        /// Initialize the state of the page
        /// </summary>
        public void OnGet()
        {
            this.ViewData["Title"] = $"{this.appGlobals.ProductName} - {this.appGlobals.CompanyName}";
        }
    }
}