using System.IO;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities;
using SQLCompare.Core.Interfaces.Repository;
using SQLCompare.Core.Interfaces.Services;

namespace SQLCompare.Services
{
    /// <summary>
    /// Defines the service that handle the application settings
    /// </summary>
    public class AppSettingsService : IAppSettingsService
    {
        private readonly ILogger<AppSettingsService> logger;
        private readonly IAppSettingsRepository appSettingsRepository;
        private AppSettings currentAppSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettingsService"/> class.
        /// </summary>
        /// <param name="logger">The injected logger</param>
        /// <param name="appSettingsRepository">The injected entity repository</param>
        public AppSettingsService(ILogger<AppSettingsService> logger, IAppSettingsRepository appSettingsRepository)
        {
            this.logger = logger;
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
                catch (IOException ex)
                {
                    this.logger.LogError(ex, "Error reading configuration file.");
                    this.currentAppSettings = new AppSettings();
                    this.appSettingsRepository.Write(this.currentAppSettings);
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
