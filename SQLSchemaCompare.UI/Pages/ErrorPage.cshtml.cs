namespace TiCodeX.SQLSchemaCompare.UI.Pages;

/// <summary>
/// PageModel for the Error page
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ErrorPage"/> class
/// </remarks>
/// <param name="appGlobals">The application globals</param>
public class ErrorPage(IAppGlobals appGlobals) : PageModel
{
    /// <summary>
    /// Gets or sets the title of the page
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Get the Error page
    /// </summary>
    public void OnGet()
    {
        this.Title = $"{appGlobals.ProductName} - {appGlobals.CompanyName}";
    }
}
