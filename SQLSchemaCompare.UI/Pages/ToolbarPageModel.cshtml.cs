using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using TiCodeX.SQLSchemaCompare.Core.Entities;
using TiCodeX.SQLSchemaCompare.Core.Entities.Api;
using TiCodeX.SQLSchemaCompare.Core.Interfaces;
using TiCodeX.SQLSchemaCompare.Core.Interfaces.Services;
using TiCodeX.SQLSchemaCompare.Services;
using TiCodeX.SQLSchemaCompare.UI.Models;

namespace TiCodeX.SQLSchemaCompare.UI.Pages
{
    /// <summary>
    /// PageModel of the Toolbar
    /// </summary>
    public class ToolbarPageModel : PageModel
    {
        private readonly ILogger logger;
        private readonly IAppGlobals appGlobals;
        private readonly IAccountService accountService;
        private readonly IAppSettingsService appSettingsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarPageModel"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="appGlobals">The injected app globals</param>
        /// <param name="accountService">The injected account service</param>
        /// <param name="appSettingsService">The injected app settings service</param>
        public ToolbarPageModel(ILoggerFactory loggerFactory, IAppGlobals appGlobals, IAccountService accountService, IAppSettingsService appSettingsService)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            this.logger = loggerFactory.CreateLogger(nameof(ToolbarPageModel));
            this.appGlobals = appGlobals;
            this.accountService = accountService;
            this.appSettingsService = appSettingsService;
        }

        /// <summary>
        /// Gets or sets the account email
        /// </summary>
        public string AccountEmail { get; set; }

        /// <summary>
        /// Gets a value indicating whether the feedback menu should be highlighted
        /// </summary>
        public bool HighlighFeedbackMenu { get; private set; } = false;

        /// <summary>
        /// Gets or sets the account URL
        /// </summary>
        public string MyAccountEndpoint { get; set; }

        /// <summary>
        /// Get the welcome page
        /// </summary>
        public void OnGet()
        {
            this.AccountEmail = this.accountService.CustomerInformation.Email;

            var session = string.Empty;
            string feedbackSent = null;
            try
            {
                feedbackSent = this.appSettingsService.GetAppSettings().FeedbackSent;
                session = this.appSettingsService.GetAppSettings().Session;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unable to get app settings");
            }

            if (feedbackSent == null)
            {
                this.HighlighFeedbackMenu = true;
            }

            this.MyAccountEndpoint = $"{this.appGlobals.MyAccountEndpoint}&s={Uri.EscapeDataString(session)}";
        }

        /// <summary>
        /// Send the feedback
        /// </summary>
        /// <param name="feedback">The feedback</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IActionResult> OnPostSendFeedback([FromBody] FeedbackRequest feedback)
        {
            try
            {
                if (feedback == null)
                {
                    return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.EInvalidFeedback, ErrorMessage = Localization.ErrorInvalidFeedback });
                }

                if (feedback.Rating.HasValue && (feedback.Rating.Value < 1 || feedback.Rating.Value > 5))
                {
                    return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.EInvalidFeedback, ErrorMessage = Localization.ErrorInvalidFeedback });
                }

                if (!string.IsNullOrWhiteSpace(feedback.Comment) && feedback.Comment.Length > 2500)
                {
                    return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.EInvalidFeedback, ErrorMessage = Localization.ErrorCommentMustBe2500Max });
                }

                if (feedback.Rating == null && string.IsNullOrWhiteSpace(feedback.Comment))
                {
                    return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.ENoFeedbackSpecified, ErrorMessage = Localization.ErrorNoFeedbackSpecified });
                }

                var session = string.Empty;
                AppSettings appSettings = null;
                try
                {
                    appSettings = this.appSettingsService.GetAppSettings();
                    session = appSettings.Session;
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Unable to get app settings");
                }

                await this.accountService.SendFeedback(session, feedback.Rating, feedback.Comment).ConfigureAwait(false);

                if (appSettings != null)
                {
                    appSettings.FeedbackSent = this.appGlobals.AppVersion;
                    this.appSettingsService.SaveAppSettings();
                }

                return new JsonResult(new ApiResponse());
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error sending feedback");
                return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.ErrorUnexpected, ErrorMessage = Localization.ErrorCannotSendFeedback });
            }
        }
    }
}