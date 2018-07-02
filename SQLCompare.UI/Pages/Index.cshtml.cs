using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Interfaces.Services;

namespace SQLCompare.UI.Pages
{
    /// <summary>
    /// PageModel of the Index page
    /// </summary>
    public class Index : PageModel
    {
        private readonly ILogger<Index> logger;
        private readonly ILocalizationService localizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Index"/> class.
        /// </summary>
        /// <param name="logger">The injected Logger</param>
        /// <param name="localizationService">The injected LocalizationService</param>
        public Index(ILogger<Index> logger, ILocalizationService localizationService)
        {
            this.logger = logger;
            this.localizationService = localizationService;
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
            this.ViewData["Title"] = "Pluto";
        }
    }
}