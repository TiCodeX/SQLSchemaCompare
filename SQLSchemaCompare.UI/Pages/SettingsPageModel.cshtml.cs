namespace TiCodeX.SQLSchemaCompare.UI.Pages
{
    /// <summary>
    /// PageModel of the settings page
    /// </summary>
    public class SettingsPageModel : PageModel
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The app settings service
        /// </summary>
        private readonly IAppSettingsService appSettingsService;

        /// <summary>
        /// The project service
        /// </summary>
        private readonly IProjectService projectService;

        /// <summary>
        /// The localization service
        /// </summary>
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
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

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
        /// <returns>The ApiResponse in JSON</returns>
        public ActionResult OnPostSave([FromBody] AppSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

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
