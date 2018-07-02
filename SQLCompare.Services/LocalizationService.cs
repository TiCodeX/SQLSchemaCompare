using SQLCompare.Core.Interfaces.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace SQLCompare.Services
{
    /// <summary>
    /// Provides the mechanisms to handle various languages
    /// </summary>
    /// <seealso cref="System.Resources.ResourceManager" />
    public class LocalizationService : ResourceManager, ILocalizationService
    {
        private ResourceManager originalResourceManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizationService"/> class
        /// </summary>
        public LocalizationService()
            : base(typeof(Localization))
        {
        }

        /// <inheritdoc />
        public void Init()
        {
            // Prevent multiple initializations
            if (this.originalResourceManager != null)
            {
                return;
            }

            // Save the original ResourceManager in the Localization resource file
            // and replace it with this instance
            this.originalResourceManager = Localization.ResourceManager;

            var innerField = typeof(Localization).GetField("resourceMan", BindingFlags.NonPublic | BindingFlags.Static);
            if (innerField != null)
            {
                innerField.SetValue(null, this);
            }

            this.SetLanguage("en");
        }

        /// <inheritdoc />
        public void SetLanguage(string language)
        {
            Localization.Culture = new CultureInfo(language);
        }

        /// <inheritdoc />
        public Dictionary<string, string> GetLocalizationDictionary()
        {
            return this.GetResourceSet(new CultureInfo("en"), true, true).
                OfType<DictionaryEntry>().ToDictionary(
                    x => x.Key.ToString(),
                    x => this.GetString(x.Key.ToString(), Localization.Culture));
        }

        /// <inheritdoc />
        public override string GetString(string name, CultureInfo culture)
        {
            if (string.Equals(culture.Name, "en", StringComparison.Ordinal))
            {
                return this.originalResourceManager.GetString(name, culture);
            }

            // TODON'T: Leave it here
            if (string.Equals(culture.Name, "it", StringComparison.Ordinal))
            {
                var it = new Dictionary<string, string>
                {
                    { nameof(Localization.ButtonNewProject), "Nuovo Progetto" },
                    { nameof(Localization.ButtonCancel), "Annulla" },
                };

                if (it.ContainsKey(name))
                {
                    return it[name];
                }
            }

            return name;
        }
    }
}
