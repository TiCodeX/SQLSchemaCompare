using System.Collections.Generic;
using System.Globalization;
using SQLCompare.Core.Enums;

namespace SQLCompare.Core.Interfaces.Services
{
    /// <summary>
    /// Defines a class that provides the mechanisms to handle the localization
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Sets the language
        /// </summary>
        /// <param name="language">The desired language</param>
        void SetLanguage(Language language);

        /// <summary>
        /// Gets the localization dictionary
        /// </summary>
        /// <returns>The dictionary with the tokens as key and their localized strings as values</returns>
        Dictionary<string, string> GetLocalizationDictionary();

        /// <summary>
        /// Gets the string localized in the current configured language
        /// </summary>
        /// <param name="token">The token</param>
        /// <returns>The localized string</returns>
        string GetString(string token);

        /// <summary>
        /// Gets the string localized in a specific language
        /// </summary>
        /// <param name="token">The token</param>
        /// <param name="culture">The language</param>
        /// <returns>The localized string</returns>
        string GetString(string token, CultureInfo culture);
    }
}
