namespace TiCodeX.SQLSchemaCompare.Test.Attributes;

/// <summary>
/// The IntegrationTestAttribute class is a custom attribute that can be applied to test methods or classes to indicate that they are integration tests.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class IntegrationTestAttribute : Attribute, ITraitAttribute
{
    /// <inheritdoc/>
    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
    {
        return [new KeyValuePair<string, string>("Category", "IntegrationTest")];
    }
}
