namespace TiCodeX.SQLSchemaCompare.UI.TagHelpers
{
    using System;
    using Microsoft.AspNetCore.Razor.TagHelpers;

    /// <summary>
    /// TagHelper for errors
    /// </summary>
    public class ErrorTagHelper : TagHelper
    {
        /// <summary>
        /// Gets or sets the title for the error modal
        /// </summary>
        public string ErrorTitle { get; set; }

        /// <summary>
        /// Gets or sets the message for the error modal
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the link if needed for the error modal
        /// </summary>
        public string ErrorLink { get; set; }

        /// <summary>
        /// Gets or sets the link text if needed for the error modal
        /// </summary>
        public string ErrorLinkText { get; set; }

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

            output.TagName = "input";
            output.Attributes.SetAttribute("class", "page-error");
            output.Attributes.SetAttribute("type", "hidden");
            output.Attributes.SetAttribute("error-title", this.ErrorTitle);
            output.Attributes.SetAttribute("error-message", this.ErrorMessage);
            output.Attributes.SetAttribute("error-link", this.ErrorLink);
            output.Attributes.SetAttribute("error-link-text", this.ErrorLink);
        }
    }
}
