using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using SQLCompare.Core.Enums;
using SQLCompare.Core.Interfaces.Services;

namespace SQLCompare.Services
{
    /// <summary>
    /// Provides the mechanisms to handle various languages
    /// </summary>
    /// <seealso cref="System.Resources.ResourceManager" />
    public class LocalizationService : ResourceManager, ILocalizationService
    {
        private ResourceManager originalResourceManager;
        private CultureInfo currentCulture;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizationService"/> class
        /// </summary>
        public LocalizationService()
            : base(typeof(Localization))
        {
        }

        /// <inheritdoc />
        public void Init(Language language)
        {
            // Prevent multiple initializations
            if (this.originalResourceManager == null)
            {
                if (Localization.ResourceManager is LocalizationService intResourceManager)
                {
                    // If the resource manager it's already a LocalizationService, take
                    // it's original resource manager
                    this.originalResourceManager = intResourceManager.originalResourceManager;
                }
                else
                {
                    // Save the original ResourceManager in the Localization resource file
                    // and replace it with this instance
                    this.originalResourceManager = Localization.ResourceManager;

                    var innerField = typeof(Localization).GetField("resourceMan", BindingFlags.NonPublic | BindingFlags.Static);
                    if (innerField != null)
                    {
                        innerField.SetValue(null, this);
                    }
                }
            }

            this.SetLanguage(language);
        }

        /// <inheritdoc />
        public void SetLanguage(Language language)
        {
            switch (language)
            {
                case Language.English:
                    this.currentCulture = CultureInfo.GetCultureInfo("en");
                    break;
                case Language.German:
                    this.currentCulture = CultureInfo.GetCultureInfo("de");
                    break;
                case Language.Italian:
                    this.currentCulture = CultureInfo.GetCultureInfo("it");
                    break;
                default:
                    this.currentCulture = CultureInfo.GetCultureInfo("en");
                    break;
            }
        }

        /// <inheritdoc />
        public Dictionary<string, string> GetLocalizationDictionary()
        {
            return this.GetResourceSet(CultureInfo.GetCultureInfo("en"), true, true).
                OfType<DictionaryEntry>().ToDictionary(
                    x => x.Key.ToString(),
                    x => this.GetString(x.Key.ToString(), null));
        }

        /// <inheritdoc />
        public override string GetString(string name, CultureInfo culture)
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = this.currentCulture;
                Thread.CurrentThread.CurrentUICulture = this.currentCulture;
                var value = this.originalResourceManager.GetString(name, culture);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }

                return $"[[{name}]]";
            }
            catch
            {
                return $"[[{name}]]";
            }
        }
    }
}
