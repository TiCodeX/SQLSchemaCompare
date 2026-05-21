namespace TiCodeX.SQLSchemaCompare.Test.Attributes;

/// <summary>
/// The CategoryAttribute class is a custom attribute that can be applied to test methods or classes to indicate their category.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class CategoryAttribute(string categoryName) : Attribute, ITraitAttribute
{
    /// <inheritdoc/>
    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
    {
        return [new KeyValuePair<string, string>("Category", categoryName)];
    }
}
