namespace TiCodeX.SQLSchemaCompare.UI.TagHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
