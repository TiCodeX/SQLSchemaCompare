using SQLCompare.Core.Entities;

namespace SQLCompare.Core.Interfaces.Repository
{
    /// <summary>
    /// Defines a class that provides the mechanism to store and retrieve application settings
    /// </summary>
    public interface IAppSettingsRepository
    {
        /// <summary>
        /// Reads the application settings
        /// </summary>
        /// <returns>The application settings</returns>
        AppSettings Read();

        /// <summary>
        /// Writes the application settings
        /// </summary>
        /// <param name="appSettings">The application settings that must be written</param>
        void Write(AppSettings appSettings);
    }
}