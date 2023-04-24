namespace TiCodeX.SQLSchemaCompare.UI.Pages
{
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using TiCodeX.SQLSchemaCompare.Core.Interfaces;

    /// <summary>
    /// PageModel for the Error page
    /// </summary>
    public class ErrorPage : PageModel
    {
        /// <summary>
        /// The app globals
        /// </summary>
        private readonly IAppGlobals appGlobals;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorPage"/> class
        /// </summary>
        /// <param name="appGlobals">The application globals</param>
        public ErrorPage(IAppGlobals appGlobals)
        {
            this.appGlobals = appGlobals;
        }

        /// <summary>
        /// Gets or sets the title of the page
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Get the Error page
        /// </summary>
        public void OnGet()
        {
            this.Title = $"{this.appGlobals.ProductName} - {this.appGlobals.CompanyName}";
        }
    }
}
