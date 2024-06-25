namespace TiCodeX.SQLSchemaCompare.UI.TagHelpers
{
    using Microsoft.AspNetCore.Razor.TagHelpers;

    /// <summary>
    /// TagHelper for button
    /// </summary>
    public class ButtonTagHelper : TagHelper
    {
        /// <summary>
        /// Gets or sets the style of the button
        /// </summary>
        public ButtonStyle Style { get; set; }

        /// <summary>
        /// Gets or sets the size of the button
        /// </summary>
        public ButtonSize Size { get; set; }

        /// <summary>
        /// Gets or sets the status of the button
        /// </summary>
        public ButtonState Status { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the button should span the full width of a parent
        /// </summary>
        public bool BlockLevel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the button should be outlined
        /// </summary>
        public bool Outline { get; set; }

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

            output.Attributes.SetAttribute("type", "button");

            var className = new HashSet<string>();

            output.Attributes["class"]?.Value.ToString().Trim().Split(' ').ToList().ForEach(x => className.Add(x));

            className.Add("btn");

            var outline = this.Outline ? "outline-" : string.Empty;
            switch (this.Style)
            {
                case ButtonStyle.Primary:
                    className.Add($"btn-{outline}primary");
                    break;
                case ButtonStyle.Secondary:
                    className.Add($"btn-{outline}secondary");
                    break;
                case ButtonStyle.Success:
                    className.Add($"btn-{outline}success");
                    break;
                case ButtonStyle.Danger:
                    className.Add($"btn-{outline}danger");
                    break;
                case ButtonStyle.Warning:
                    className.Add($"btn-{outline}warning");
                    break;
                case ButtonStyle.Info:
                    className.Add($"btn-{outline}info");
                    break;
                case ButtonStyle.Light:
                    className.Add($"btn-{outline}light");
                    break;
                case ButtonStyle.Dark:
                    className.Add($"btn-{outline}dark");
                    break;
                case ButtonStyle.Link:
                    className.Add($"btn-{outline}link");
                    break;
                case ButtonStyle.TcxHighlight:
                    className.Add($"btn-{outline}tcx-highlight");
                    break;
                default:
                    // Do nothing
                    break;
            }

            switch (this.Size)
            {
                case ButtonSize.Small:
                    className.Add("btn-sm");
                    break;
                case ButtonSize.Large:
                    className.Add("btn-lg");
                    break;
                default:
                    // Do nothing
                    break;
            }

            switch (this.Status)
            {
                case ButtonState.Active:
                    className.Add("active");
                    break;
                case ButtonState.Disabled:
                    output.Attributes.SetAttribute("disabled", null);
                    break;
                default:
                    // Do nothing
                    break;
            }

            if (this.BlockLevel)
            {
                className.Add("btn-block");
            }

            output.Attributes.SetAttribute("class", string.Join(" ", className));
        }
    }
}
