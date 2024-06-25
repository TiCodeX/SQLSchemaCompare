namespace TiCodeX.SQLSchemaCompare.UI.Pages
{
    /// <summary>
    /// PageModel of the Index page
    /// </summary>
    public class Index : PageModel
    {
        /// <summary>
        /// The app globals
        /// </summary>
        private readonly IAppGlobals appGlobals;

        /// <summary>
        /// The localization service
        /// </summary>
        private readonly ILocalizationService localizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Index"/> class.
        /// </summary>
        /// <param name="appGlobals">The injected app globals</param>
        /// <param name="localizationService">The injected LocalizationService</param>
        public Index(IAppGlobals appGlobals, ILocalizationService localizationService)
        {
            this.appGlobals = appGlobals;
            this.localizationService = localizationService;
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
                Result = this.localizationService.GetLocalizationDictionary(),
            });
        }

        /// <summary>
        /// Initialize the state of the page
        /// </summary>
        /// <param name="v">The application version</param>
        /// <returns>The page</returns>
        public IActionResult OnGet(string v)
        {
            this.Title = $"{this.appGlobals.ProductName} - {this.appGlobals.CompanyName}";

            // Set application version
            this.appGlobals.AppVersion = v;
            return this.Page();
        }
    }
}
