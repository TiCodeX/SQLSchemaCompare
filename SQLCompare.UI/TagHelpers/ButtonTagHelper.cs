using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json;
using SQLCompare.UI.Enums;
using SQLCompare.UI.Models;
using System;
using System.Collections.Generic;
using System.Web;

namespace SQLCompare.UI.TagHelpers
{
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
        /// Gets or sets the WebRequest to load the page in the modal dialog
        /// </summary>
        public WebRequest OnclickOpenModal { get; set; }

        /// <summary>
        /// Gets or sets the WebRequest to load the page in the main layout
        /// </summary>
        public WebRequest OnclickOpenMain { get; set; }

        /// <summary>
        /// Gets or sets the WebRequest to load the select values
        /// </summary>
        public WebRequest OnclickLoadSelectValues { get; set; }

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

            if (output.Attributes["class"] != null)
            {
                className.Add(output.Attributes["class"].Value.ToString());
            }

            className.Add("btn");
            switch (this.Style)
            {
                case ButtonStyle.Primary:
                    className.Add("btn-primary");
                    break;
                case ButtonStyle.Secondary:
                    className.Add("btn-secondary");
                    break;
                case ButtonStyle.Success:
                    className.Add("btn-success");
                    break;
                case ButtonStyle.Danger:
                    className.Add("btn-danger");
                    break;
                case ButtonStyle.Warning:
                    className.Add("btn-warning");
                    break;
                case ButtonStyle.Info:
                    className.Add("btn-info");
                    break;
                case ButtonStyle.Light:
                    className.Add("btn-light");
                    break;
                case ButtonStyle.Dark:
                    className.Add("btn-dark");
                    break;
                case ButtonStyle.Link:
                    className.Add("btn-link");
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
            }

            switch (this.Status)
            {
                case ButtonState.Active:
                    className.Add("active");
                    break;
                case ButtonState.Disabled:
                    output.Attributes.SetAttribute("disabled", null);
                    break;
            }

            if (this.BlockLevel)
            {
                className.Add("btn-block");
            }

            output.Attributes.SetAttribute("class", string.Join(" ", className));

            if (this.OnclickOpenModal != null)
            {
                output.Attributes.SetAttribute("load-modal", this.OnclickOpenModal.Url);
                output.Attributes.SetAttribute("load-modal-method", this.OnclickOpenModal.Method.Method);
                if (this.OnclickOpenModal.Data != null)
                {
                    output.Attributes.SetAttribute("load-modal-data", HttpUtility.JavaScriptStringEncode(JsonConvert.SerializeObject(this.OnclickOpenModal.Data)));
                }

                if (!string.IsNullOrWhiteSpace(this.OnclickOpenModal.SerializeDataFromDiv))
                {
                    output.Attributes.SetAttribute("load-modal-data-from-div", this.OnclickOpenModal.SerializeDataFromDiv);
                }
            }

            if (this.OnclickOpenMain != null)
            {
                output.Attributes.SetAttribute("load-main", this.OnclickOpenMain.Url);
                output.Attributes.SetAttribute("load-main-method", this.OnclickOpenMain.Method.Method);
                if (this.OnclickOpenMain.Data != null)
                {
                    output.Attributes.SetAttribute("load-main-data", HttpUtility.JavaScriptStringEncode(JsonConvert.SerializeObject(this.OnclickOpenMain.Data)));
                }

                if (!string.IsNullOrWhiteSpace(this.OnclickOpenMain.SerializeDataFromDiv))
                {
                    output.Attributes.SetAttribute("load-main-data-from-div", this.OnclickOpenMain.SerializeDataFromDiv);
                }
            }

            if (this.OnclickLoadSelectValues != null)
            {
                output.Attributes.SetAttribute("load-select", this.OnclickLoadSelectValues.Url);
                output.Attributes.SetAttribute("load-select-method", this.OnclickLoadSelectValues.Method.Method);
                output.Attributes.SetAttribute("load-select-target", this.OnclickLoadSelectValues.Target);
                if (this.OnclickLoadSelectValues.Data != null)
                {
                    output.Attributes.SetAttribute("load-select-data", HttpUtility.JavaScriptStringEncode(JsonConvert.SerializeObject(this.OnclickLoadSelectValues.Data)));
                }

                if (!string.IsNullOrWhiteSpace(this.OnclickLoadSelectValues.SerializeDataFromDiv))
                {
                    output.Attributes.SetAttribute("load-select-data-from-div", this.OnclickLoadSelectValues.SerializeDataFromDiv);
                }
            }
        }
    }
}
