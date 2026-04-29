namespace TiCodeX.SQLSchemaCompare.Services;

using System.Collections;
using System.Reflection;

/// <summary>
/// Provides the mechanisms to handle various languages
/// </summary>
/// <seealso cref="System.Resources.ResourceManager" />
public class LocalizationService : ILocalizationService
{
    /// <summary>
    /// The custom resource manager
    /// </summary>
    private readonly CustomResourceManager customResourceManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationService"/> class
    /// </summary>
    [SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "Necessary")]
    public LocalizationService()
    {
        // Prevent multiple initializations
        if (Localization.ResourceManager is CustomResourceManager customResMgr)
        {
            this.customResourceManager = customResMgr;
        }
        else
        {
            // Save the original ResourceManager in the Localization resource file
            // and replace it with the CustomResourceManager
            this.customResourceManager = new CustomResourceManager(Localization.ResourceManager);

            var innerField = typeof(Localization).GetField("resourceMan", BindingFlags.NonPublic | BindingFlags.Static);
            innerField?.SetValue(null, this.customResourceManager);
        }
    }

    /// <inheritdoc />
    public void SetLanguage(Language language)
    {
        Localization.Culture = language switch
        {
            Language.English => CultureInfo.GetCultureInfo("en"),
            Language.German => CultureInfo.GetCultureInfo("de"),
            Language.Italian => CultureInfo.GetCultureInfo("it"),
            _ => CultureInfo.GetCultureInfo("en"),
        };
    }

    /// <inheritdoc />
    public Dictionary<string, string> GetLocalizationDictionary()
    {
        return this.customResourceManager.GetResourceSet(CultureInfo.GetCultureInfo("en"))
            .OfType<DictionaryEntry>().ToDictionary(
                x => x.Key.ToString(),
                x => Localization.ResourceManager.GetString(x.Key.ToString(), Localization.Culture));
    }

    /// <inheritdoc />
    public string GetString(string token)
    {
        return this.GetString(token, Localization.Culture);
    }

    /// <inheritdoc />
    public string GetString(string token, CultureInfo culture)
    {
        return this.customResourceManager.GetString(token, culture);
    }
}
