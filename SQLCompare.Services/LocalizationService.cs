using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
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

            this.SetLanguage(language);
        }

        /// <inheritdoc />
        public void SetLanguage(Language language)
        {
            CultureInfo culture;
            switch (language)
            {
                case Language.English:
                    culture = new CultureInfo("en");
                    break;
                case Language.German:
                    culture = new CultureInfo("de");
                    break;
                case Language.Italian:
                    culture = new CultureInfo("it");
                    break;
                default:
                    culture = new CultureInfo("en");
                    break;
            }

            Localization.Culture = culture;
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
                    { nameof(Localization.ButtonCancel), "Annulla" },
                    { nameof(Localization.ButtonCompareNow), "Compara adesso" },
                    { nameof(Localization.ButtonNewProject), "Nuovo Progetto" },
                    { nameof(Localization.ButtonOpenProject), "Apri Progetto" },
                    { nameof(Localization.ButtonSave), "Salva" },
                    { nameof(Localization.LabelDatabase), "Database" },
                    { nameof(Localization.LabelDataSources), "Sorgenti dati" },
                    { nameof(Localization.LabelHostname), "Hostname" },
                    { nameof(Localization.LabelIdentical), "Uguali" },
                    { nameof(Localization.LabelInBothButDifferent), "In tutti e due, ma differenti" },
                    { nameof(Localization.LabelLanguage), "Lingua" },
                    { nameof(Localization.LabelName), "Nome" },
                    { nameof(Localization.LabelOnlyInSource), "Solo in origine" },
                    { nameof(Localization.LabelOnlyInTarget), "Solo in destinazione" },
                    { nameof(Localization.LabelOptions), "Opzioni" },
                    { nameof(Localization.LabelOwnerMapping), "Mappatura proprietari" },
                    { nameof(Localization.LabelPassword), "Password" },
                    { nameof(Localization.LabelRecentProjects), "Progetti recenti" },
                    { nameof(Localization.LabelSource), "Origine" },
                    { nameof(Localization.LabelTable), "Tabella" },
                    { nameof(Localization.LabelTableMapping), "Mappatura tabelle" },
                    { nameof(Localization.LabelTarget), "Destinazione" },
                    { nameof(Localization.LabelType), "Tipo" },
                    { nameof(Localization.LabelUsername), "Utente" },
                    { nameof(Localization.LabelUseSSL), "Usa SSL" },
                    { nameof(Localization.LabelUseWindowsAuthentication), "Usa Autenticazione Windows" },
                    { nameof(Localization.LabelView), "Vista" },
                    { nameof(Localization.MenuAbout), "Informazioni" },
                    { nameof(Localization.MenuCloseProject), "Chiudi Progetto" },
                    { nameof(Localization.MenuEditProject), "Modifica" },
                    { nameof(Localization.MenuExit), "Esci" },
                    { nameof(Localization.MenuFile), "File" },
                    { nameof(Localization.MenuHelp), "Aiuto" },
                    { nameof(Localization.MenuNewProject), "Nuovo Progetto" },
                    { nameof(Localization.MenuOpenProject), "Apri Progetto" },
                    { nameof(Localization.MenuProject), "Progetto" },
                    { nameof(Localization.MenuSaveProject), "Salva Progetto" },
                    { nameof(Localization.MenuSettings), "Impostazioni" },
                    { nameof(Localization.TitleWelcome), "Benvenuti in {0}" },
                };

                if (it.ContainsKey(name))
                {
                    return it[name];
                }
            }

            return $"[[{name}]]";
        }
    }
}
