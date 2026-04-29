namespace TiCodeX.SQLSchemaCompare.UI.Pages;

/// <summary>
/// PageModel of the Index page
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Index"/> class.
/// </remarks>
/// <param name="appGlobals">The injected app globals</param>
/// <param name="localizationService">The injected LocalizationService</param>
public class Index(IAppGlobals appGlobals, ILocalizationService localizationService) : PageModel
{
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
            Result = localizationService.GetLocalizationDictionary(),
        });
    }

    /// <summary>
    /// Initialize the state of the page
    /// </summary>
    /// <param name="v">The application version</param>
    /// <returns>The page</returns>
    public IActionResult OnGet(string v)
    {
        this.Title = $"{appGlobals.ProductName} - {appGlobals.CompanyName}";

        // Set application version
        appGlobals.AppVersion = v;
        return this.Page();
    }
}
