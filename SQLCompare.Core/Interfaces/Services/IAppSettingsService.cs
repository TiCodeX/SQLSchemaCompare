using SQLCompare.Core.Entities;

namespace SQLCompare.Core.Interfaces.Services
{
    /// <summary>
    /// Defines a class that provides the mechanisms to handle application settings.
    /// </summary>
    public interface IAppSettingsService
    {
        /// <summary>
        /// Gets the current application settings
        /// </summary>
        /// <returns>The current apllication settings</returns>
        AppSettings GetAppSettings();

        /// <summary>
        /// Save the new application settings
        /// </summary>
        /// <param name="appSettings">The application settings to be saved</param>
        void SaveAppSettings(AppSettings appSettings);
    }
}