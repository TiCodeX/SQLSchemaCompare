using System.Collections.Generic;
using SQLCompare.Core.Enums;

namespace SQLCompare.Core.Interfaces.Services
{
    /// <summary>
    /// Defines a class that provides the mechanisms to handle the localization
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Initializes the service with English language
        /// </summary>
        /// <param name="language">The desired language</param>
        void Init(Language language);

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
    }
}
