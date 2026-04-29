namespace TiCodeX.SQLSchemaCompare.UI.Pages;

/// <summary>
/// PageModel of the about page
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AboutPageModel"/> class
/// </remarks>
/// <param name="appGlobals">The injected application globals</param>
public class AboutPageModel(IAppGlobals appGlobals) : PageModel
{
    /// <summary>
    /// Gets or sets the name of the product
    /// </summary>
    public string ProductName { get; set; }

    /// <summary>
    /// Gets or sets the application version
    /// </summary>
    public string AppVersion { get; set; }

    /// <summary>
    /// Get the about page
    /// </summary>
    public void OnGet()
    {
        this.ProductName = appGlobals.ProductName;
        this.AppVersion = appGlobals.AppVersion;
    }
}
