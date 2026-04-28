namespace TiCodeX.SQLSchemaCompare.UI.Pages
{
    /// <summary>
    /// PageModel of the Welcome page
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="WelcomePageModel"/> class.
    /// </remarks>
    /// <param name="appSettingsService">The injected app settings service</param>
    public class WelcomePageModel(IAppSettingsService appSettingsService) : PageModel
    {
        /// <summary>
        /// Gets the recently opened projects
        /// </summary>
        public List<string> RecentProjects { get; internal set; }

        /// <summary>
        /// Get the welcome page
        /// </summary>
        public void OnGet()
        {
            var settings = appSettingsService.GetAppSettings();
            this.RecentProjects = [.. settings.RecentProjects.TakeLast(10).Reverse()];
        }
    }
}
