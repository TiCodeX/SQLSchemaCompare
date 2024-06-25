namespace TiCodeX.SQLSchemaCompare.Services
{
    /// <summary>
    /// Defines the service that handle the application settings
    /// </summary>
    public class AppSettingsService : IAppSettingsService
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The app settings repository
        /// </summary>
        private readonly IAppSettingsRepository appSettingsRepository;

        /// <summary>
        /// The current app settings
        /// </summary>
        private AppSettings currentAppSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettingsService"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="appSettingsRepository">The injected entity repository</param>
        public AppSettingsService(ILoggerFactory loggerFactory, IAppSettingsRepository appSettingsRepository)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            this.logger = loggerFactory.CreateLogger(nameof(AppSettingsService));
            this.appSettingsRepository = appSettingsRepository;
        }

        /// <inheritdoc/>
        public AppSettings GetAppSettings()
        {
            if (this.currentAppSettings == null)
            {
                try
                {
                    this.currentAppSettings = this.appSettingsRepository.Read();
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Error reading configuration file.");
                    this.currentAppSettings = new AppSettings();
                }
            }

            return this.currentAppSettings;
        }

        /// <inheritdoc/>
        public void SaveAppSettings()
        {
            if (this.currentAppSettings != null)
            {
                this.appSettingsRepository.Write(this.currentAppSettings);
            }
        }
    }
}
