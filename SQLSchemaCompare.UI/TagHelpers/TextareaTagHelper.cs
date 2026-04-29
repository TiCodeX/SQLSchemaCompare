namespace TiCodeX.SQLSchemaCompare.UI.TagHelpers;

using Microsoft.AspNetCore.Razor.TagHelpers;

/// <summary>
/// TagHelper for textarea
/// </summary>
[HtmlTargetElement("textarea", TagStructure = TagStructure.NormalOrSelfClosing)]
public class TextareaTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="TextareaTagHelper"/> is disabled
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="TextareaTagHelper"/> is required
    /// </summary>
    public bool Required { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        var className = new HashSet<string>();

        output.Attributes["class"]?.Value.ToString().Trim().Split(' ').ToList().ForEach(x => className.Add(x));

        className.Add("form-control");
        className.Add("form-control-sm");

        output.Attributes.SetAttribute("class", string.Join(" ", className));

        if (this.Disabled)
        {
            output.Attributes.SetAttribute("disabled", "disabled");
        }

        if (this.Required)
        {
            output.Attributes.SetAttribute("required", "required");
        }
    }
}
