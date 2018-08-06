using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SQLCompare.Core.Entities;
using SQLCompare.Core.Interfaces.Services;
using SQLCompare.UI.WebServer;

namespace SQLCompare.UI.Pages
{
    /// <summary>
    /// PageModel of the settings page
    /// </summary>
    public class SettingsPageModel : PageModel
    {
        private readonly IAppSettingsService appSettingsService;
        private readonly ILocalizationService localizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPageModel"/> class.
        /// </summary>
        /// <param name="appSettingsService">The injected app settings service</param>
        /// <param name="localizationService">The injected LocalizationService</param>
        public SettingsPageModel(IAppSettingsService appSettingsService, ILocalizationService localizationService)
        {
            this.appSettingsService = appSettingsService;
            this.localizationService = localizationService;
        }

        /// <summary>
        /// Gets the settings
        /// </summary>
        public AppSettings Settings { get; internal set; }

        /// <summary>
        /// Get the settings page
        /// </summary>
        public void OnGet()
        {
            this.Settings = this.appSettingsService.GetAppSettings();
        }

        /// <summary>
        /// Save the settings
        /// </summary>
        /// <param name="settings">The settings to save</param>
        /// <returns>TODO: boh</returns>
        public ActionResult OnPostSave([FromBody] AppSettings settings)
        {
            var currentSettings = this.appSettingsService.GetAppSettings();

            if (currentSettings.Language != settings.Language)
            {
                this.localizationService.SetLanguage(settings.Language);
                currentSettings.Language = settings.Language;
            }

            if (currentSettings.LogLevel != settings.LogLevel)
            {
                Utility.SetLoggingLevel(settings.LogLevel);
                currentSettings.LogLevel = settings.LogLevel;
            }

            this.appSettingsService.SaveAppSettings();
            return new JsonResult(null);
        }
    }
}