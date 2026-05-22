namespace TiCodeX.SQLSchemaCompare.Test.Attributes;

/// <summary>
/// The UnitTestAttribute class is a custom attribute that can be applied to test methods or classes to indicate that they are unit tests.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class UnitTestAttribute : Attribute, ITraitAttribute
{
    /// <inheritdoc/>
    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
    {
        return [new KeyValuePair<string, string>("Category", "UnitTest")];
    }
}
