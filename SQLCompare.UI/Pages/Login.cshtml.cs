using System;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Api;
using SQLCompare.Core.Entities.Exceptions;
using SQLCompare.Core.Interfaces;
using SQLCompare.Core.Interfaces.Services;
using SQLCompare.Services;

namespace SQLCompare.UI.Pages
{
    /// <summary>
    /// PageModel for the Login page
    /// </summary>
    public class Login : PageModel
    {
        private readonly IAppSettingsService appSettingsService;
        private readonly IAccountService accountService;
        private readonly ILogger logger;
        private readonly IAppGlobals appGlobals;
        private readonly IProjectService projectService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Login"/> class.
        /// </summary>
        /// <param name="appSettingsService">The injected app settings service</param>
        /// <param name="accountService">The injected account service</param>
        /// <param name="logger">The injected logger</param>
        /// <param name="appGlobals">The injected application globals</param>
        /// <param name="projectService">The injected project service</param>
        public Login(IAppSettingsService appSettingsService, IAccountService accountService, ILogger<Login> logger, IAppGlobals appGlobals, IProjectService projectService)
        {
            this.appSettingsService = appSettingsService;
            this.accountService = accountService;
            this.logger = logger;
            this.appGlobals = appGlobals;
            this.projectService = projectService;
        }

        /// <summary>
        /// Gets or sets the title of the page
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the login URL
        /// </summary>
        public string LoginEndpoint { get; set; }

        /// <summary>
        /// Gets a value indicating whether the saved session has been verified
        /// </summary>
        public bool Verified { get; private set; }

        /// <summary>
        /// Get the Login page
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IActionResult> OnGet()
        {
            this.Title = $"{this.appGlobals.ProductName} - {this.appGlobals.CompanyName}";
            this.LoginEndpoint = this.appGlobals.LoginEndpoint;

            var appSettings = this.appSettingsService.GetAppSettings();
            var session = appSettings.Session;

            if (!string.IsNullOrEmpty(session))
            {
                try
                {
                    await this.accountService.VerifySession(session).ConfigureAwait(false);
                    if (this.accountService.CustomerInformation.SubscriptionPlan == null || !this.accountService.CustomerInformation.ExpirationDate.HasValue || this.accountService.CustomerInformation.ExpirationDate.Value < DateTime.Now)
                    {
                        this.logger.LogError("Saved session token is not valid anymore");

                        // Remove session from settings
                        appSettings.Session = null;
                        this.appSettingsService.SaveAppSettings();
                    }
                    else
                    {
                        // Session verified successfully
                        this.Verified = true;
                    }
                }
                catch (AccountServiceException ex)
                {
                    this.logger.LogError($"An error occurred while verifying the saved session token: {ex.ErrorCode} - {ex.Message}");

                    appSettings.Session = null;
                    this.appSettingsService.SaveAppSettings();
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"An unexpected error occurred while verifying the saved session token: {ex.Message}");
                }
            }

            return this.Page();
        }

        /// <summary>
        /// Verify login session token
        /// </summary>
        /// <param name="url">The redirect url containing the auth token</param>
        /// <returns>The resulting json</returns>
        public async Task<IActionResult> OnPostVerify([FromBody] Uri url)
        {
            if (url == null)
            {
                this.logger.LogError("Url is null");
                return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.ErrorRedirectUrlIsNull, ErrorMessage = Localization.ErrorLoginVerificationFailed });
            }

            var queryParams = HttpUtility.ParseQueryString(url.Query);

            var sessionToken = queryParams.Get("t");

            if (string.IsNullOrEmpty(sessionToken))
            {
                this.logger.LogError("Null or empty session token");
                return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.ErrorSessionTokenIsNullOrEmpty, ErrorMessage = Localization.ErrorLoginVerificationFailed });
            }

            try
            {
                await this.accountService.VerifySession(sessionToken).ConfigureAwait(false);
                if (this.accountService.CustomerInformation.SubscriptionPlan == null)
                {
                    this.logger.LogError("No subscription plan");
                    return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.ErrorNoSubscriptionAvailable, ErrorMessage = Localization.ErrorNoSubscriptionAvailable });
                }

                if (!this.accountService.CustomerInformation.ExpirationDate.HasValue || this.accountService.CustomerInformation.ExpirationDate.Value < DateTime.Now)
                {
                    if (this.accountService.CustomerInformation.IsTrial.HasValue && this.accountService.CustomerInformation.IsTrial.Value)
                    {
                        this.logger.LogError("Expired trial subscription");
                        return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.ErrorTrialSubscriptionExpired, ErrorMessage = Localization.ErrorTrialSubscriptionExpired });
                    }

                    this.logger.LogError("Expired subscription");
                    return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.ErrorSubscriptionExpired, ErrorMessage = Localization.ErrorSubscriptionExpired });
                }

                var settings = this.appSettingsService.GetAppSettings();
                settings.Session = sessionToken;
                this.appSettingsService.SaveAppSettings();

                return new JsonResult(new ApiResponse { Success = true }, new Newtonsoft.Json.JsonSerializerSettings { });
            }
            catch (AccountServiceException ex)
            {
                this.logger.LogError($"VerifySession error: {ex.ErrorCode} - {ex.Message}");
                string error;

                switch (ex.ErrorCode)
                {
                    case EErrorCode.ErrorAccountLocked:
                        error = Localization.ErrorAccountLocked;
                        break;
                    case EErrorCode.ErrorEmailNotVerified:
                        error = Localization.ErrorEmailNotVerified;
                        break;
                    case EErrorCode.ErrorSessionExpired:
                        error = Localization.ErrorSessionExpired;
                        break;
                    default:
                        error = Localization.ErrorGeneric;
                        break;
                }

                return new JsonResult(new ApiResponse { Success = false, ErrorCode = ex.ErrorCode, ErrorMessage = error });
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Error occured in index: {ex.Message}");
                return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.ErrorUnexpected, ErrorMessage = Localization.ErrorLoginVerificationFailed });
            }
        }

        /// <summary>
        /// Logout the users from the app
        /// </summary>
        /// <returns>The resulting json</returns>
        public IActionResult OnPostLogout()
        {
            if (this.projectService.NeedSave())
            {
                return new JsonResult(new { success = false, error = "Project need to be saved" });
            }

            this.appSettingsService.GetAppSettings().Session = null;
            this.appSettingsService.SaveAppSettings();

            this.accountService.Logout();

            return new JsonResult(new { success = true });
        }
    }
}