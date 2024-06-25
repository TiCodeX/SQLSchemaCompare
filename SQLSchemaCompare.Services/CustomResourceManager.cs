namespace TiCodeX.SQLSchemaCompare.Services
{
    using System.Resources;

    /// <summary>
    /// Custom wrapper to properly handle the localization
    /// </summary>
    /// <seealso cref="System.Resources.ResourceManager" />
    public class CustomResourceManager : ResourceManager
    {
        /// <summary>
        /// The original resource manager
        /// </summary>
        private readonly ResourceManager originalResourceManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomResourceManager"/> class
        /// </summary>
        /// <param name="originalResourceManager">The original ResourceManager</param>
        public CustomResourceManager(ResourceManager originalResourceManager)
            : base(typeof(Localization))
        {
            this.originalResourceManager = originalResourceManager;
        }

        /// <summary>
        /// Gets the ResourceSet of a specific language
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>The localized ResourceSet</returns>
        public ResourceSet GetResourceSet(CultureInfo culture)
        {
            // TryParents argument is false, in order to return null if there isn't
            // a ResourceSet for that language
            return this.originalResourceManager.GetResourceSet(culture, true, false);
        }

        /// <inheritdoc />
        public override string GetString(string name, CultureInfo culture)
        {
            try
            {
                // Returns the token name if the ResourceSet doesn't exist for the specific language
                // or if there isn't a translation
                return this.GetResourceSet(culture)?.GetString(name) ?? $"[[{name}]]";
            }
            catch
            {
                return $"[[{name}]]";
            }
        }
    }
}
