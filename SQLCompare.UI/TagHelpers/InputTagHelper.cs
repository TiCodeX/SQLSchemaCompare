using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SQLCompare.UI.TagHelpers
{
    /// <summary>
    /// TagHelper for input
    /// </summary>
    [HtmlTargetElement("input", TagStructure = TagStructure.WithoutEndTag)]
    public class InputTagHelper : TagHelper
    {
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

            if (output.Attributes["type"] == null || !string.Equals(output.Attributes["type"].Value.ToString(), "hidden", StringComparison.OrdinalIgnoreCase))
            {
                className.Add("form-control");
            }

            output.Attributes.SetAttribute("class", string.Join(" ", className));
        }
    }
}
