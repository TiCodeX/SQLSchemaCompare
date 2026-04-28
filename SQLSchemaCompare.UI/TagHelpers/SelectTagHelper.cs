namespace TiCodeX.SQLSchemaCompare.UI.TagHelpers
{
    using Microsoft.AspNetCore.Razor.TagHelpers;

    /// <summary>
    /// TagHelper for select
    /// </summary>
    public class SelectTagHelper : TagHelper
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SelectTagHelper"/> is disabled
        /// </summary>
        public bool Disabled { get; set; }

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
        }
    }
}
