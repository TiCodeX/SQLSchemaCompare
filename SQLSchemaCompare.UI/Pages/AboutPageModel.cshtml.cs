namespace TiCodeX.SQLSchemaCompare.UI.Pages
{
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using TiCodeX.SQLSchemaCompare.Core.Interfaces;

    /// <summary>
    /// PageModel of the about page
    /// </summary>
    public class AboutPageModel : PageModel
    {
        /// <summary>
        /// The app globals
        /// </summary>
        private readonly IAppGlobals appGlobals;

        /// <summary>
        /// Initializes a new instance of the <see cref="AboutPageModel"/> class
        /// </summary>
        /// <param name="appGlobals">The injected application globals</param>
        public AboutPageModel(IAppGlobals appGlobals)
        {
            this.appGlobals = appGlobals;
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
        /// Get the about page
        /// </summary>
        public void OnGet()
        {
            this.ProductName = this.appGlobals.ProductName;
            this.AppVersion = this.appGlobals.AppVersion;
        }
    }
}
