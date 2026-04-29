namespace TiCodeX.SQLSchemaCompare.UI.TagHelpers;

using Microsoft.AspNetCore.Razor.TagHelpers;

/// <summary>
/// TagHelper for option
/// </summary>
public class OptionTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="OptionTagHelper"/> is selected
    /// </summary>
    public bool Selected { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);

        ArgumentNullException.ThrowIfNull(output);

        if (this.Selected)
        {
            output.Attributes.SetAttribute("selected", "selected");
        }
    }
}
