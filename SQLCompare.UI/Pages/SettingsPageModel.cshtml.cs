using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities;
using SQLCompare.Core.Entities.Api;
using SQLCompare.Core.Entities.Project;
using SQLCompare.Core.Interfaces.Services;
using SQLCompare.UI.WebServer;

namespace SQLCompare.UI.Pages
{
    /// <summary>
    /// PageModel of the settings page
    /// </summary>
    public class SettingsPageModel : PageModel
    {
        private readonly ILogger logger;
        private readonly IAppSettingsService appSettingsService;
        private readonly IProjectService projectService;
        private readonly ILocalizationService localizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPageModel"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="appSettingsService">The injected app settings service</param>
        /// <param name="projectService">The injected project service</param>
        /// <param name="localizationService">The injected LocalizationService</param>
        public SettingsPageModel(
            ILoggerFactory loggerFactory,
            IAppSettingsService appSettingsService,
            IProjectService projectService,
            ILocalizationService localizationService)
        {
            this.logger = loggerFactory.CreateLogger(nameof(SettingsPageModel));
            this.appSettingsService = appSettingsService;
            this.projectService = projectService;
            this.localizationService = localizationService;
        }

        /// <summary>
        /// Gets the current project
        /// </summary>
        public CompareProject Project { get; internal set; }

        /// <summary>
        /// Gets the settings
        /// </summary>
        public AppSettings Settings { get; internal set; }

        /// <summary>
        /// Get the settings page
        /// </summary>
        public void OnGet()
        {
            this.Project = this.projectService.Project;
            this.Settings = this.appSettingsService.GetAppSettings();
        }

        /// <summary>
        /// Save the settings
        /// </summary>
        /// <param name="settings">The settings to save</param>
        /// <returns>TODO: boh</returns>
        public ActionResult OnPostSave([FromBody] AppSettings settings)
        {
            this.logger.LogDebug("Saving settings...");
            this.logger.LogDebug($"LogLevel => {settings.LogLevel}");
            this.logger.LogDebug($"Language => {settings.Language}");

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

            return new JsonResult(new ApiResponse());
        }
    }
}