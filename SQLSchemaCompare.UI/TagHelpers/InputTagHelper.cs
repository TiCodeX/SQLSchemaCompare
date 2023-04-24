namespace TiCodeX.SQLSchemaCompare.UI.TagHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Razor.TagHelpers;

    /// <summary>
    /// TagHelper for input
    /// </summary>
    [HtmlTargetElement("input", TagStructure = TagStructure.WithoutEndTag)]
    public class InputTagHelper : TagHelper
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="InputTagHelper"/> is disabled
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="InputTagHelper"/> is checked
        /// </summary>
        /// <remarks>Works only when the type is checkbox</remarks>
        public bool Checked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="InputTagHelper"/> is required
        /// </summary>
        public bool Required { get; set; }

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            var className = new HashSet<string>();

            if (output.Attributes["class"] != null)
            {
                output.Attributes["class"].Value.ToString().Trim().Split(' ').ToList().ForEach(x => className.Add(x));
            }

            if (output.Attributes["type"] == null || output.Attributes["type"].Value.ToString().ToUpperInvariant() != "HIDDEN")
            {
                className.Add("form-control");
                className.Add("form-control-sm");
            }

            output.Attributes.SetAttribute("class", string.Join(" ", className));

            if (this.Disabled)
            {
                output.Attributes.SetAttribute("disabled", "disabled");
            }

            if (output.Attributes["type"] != null &&
                output.Attributes["type"].Value.ToString().ToUpperInvariant() == "CHECKBOX" &&
                this.Checked)
            {
                output.Attributes.SetAttribute("checked", "checked");
            }

            if (this.Required)
            {
                output.Attributes.SetAttribute("required", "required");
            }
        }
    }
}
