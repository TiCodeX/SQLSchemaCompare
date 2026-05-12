namespace TiCodeX.SQLSchemaCompare.CLI.Utils;

/// <summary>
/// The option group attribute, used to group related options together in the help output.
/// </summary>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Property)]
public sealed class OptionGroupAttribute(string name) : Attribute
{
    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; } = name;
}
