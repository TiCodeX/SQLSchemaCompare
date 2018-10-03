using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities;
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
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="appGlobals">The injected application globals</param>
        /// <param name="projectService">The injected project service</param>
        public Login(IAppSettingsService appSettingsService, IAccountService accountService, ILoggerFactory loggerFactory, IAppGlobals appGlobals, IProjectService projectService)
        {
            this.appSettingsService = appSettingsService;
            this.accountService = accountService;
            this.logger = loggerFactory.CreateLogger(nameof(Login));
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
        /// Gets or sets the subscribe URL
        /// </summary>
        public string SubscribeEndpoint { get; set; }

        /// <summary>
        /// Gets the result of the verify session call
        /// </summary>
        public ApiResponse<string> VerifySessionResult { get; private set; }

        /// <summary>
        /// Get the Login page
        /// </summary>
        /// <param name="v">The application version</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IActionResult> OnGet(string v)
        {
            this.Title = $"{this.appGlobals.ProductName} - {this.appGlobals.CompanyName}";

            // Set application version
            this.appGlobals.AppVersion = v;
            this.LoginEndpoint = this.appGlobals.LoginEndpoint;
            this.SubscribeEndpoint = this.appGlobals.SubscribeEndpoint;

            AppSettings appSettings = null;
            string session = null;
            try
            {
                appSettings = this.appSettingsService.GetAppSettings();
                session = appSettings.Session;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unable to get app settings");
            }

            if (string.IsNullOrEmpty(session))
            {
                return this.Page();
            }

            this.VerifySessionResult = await this.VerifySessionAsync(session).ConfigureAwait(false);
            if (this.VerifySessionResult.Success)
            {
                // Session verified successfully
                return this.Page();
            }
            else if (this.VerifySessionResult.ErrorCode != EErrorCode.ErrorUnexpected)
            {
                this.logger.LogError($"Verify session error: {this.VerifySessionResult.ErrorCode} - {this.VerifySessionResult.ErrorMessage}");

                // Remove session from settings
                appSettings.Session = null;
                try
                {
                    this.appSettingsService.SaveAppSettings();
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Unable to save app settings");
                }

                return this.Page();
            }
            else
            {
                this.logger.LogError($"An unexpected error occurred while verifying the saved session token: {this.VerifySessionResult.ErrorMessage}");
                return this.Page();
            }
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
                return new JsonResult(new ApiResponse<string>
                {
                    Success = false,
                    ErrorCode = EErrorCode.ErrorRedirectUrlIsNull,
                    ErrorMessage = Localization.ErrorLoginVerificationFailed
                });
            }

            var queryParams = HttpUtility.ParseQueryString(url.Query);

            var sessionToken = queryParams.Get("t");

            if (string.IsNullOrEmpty(sessionToken))
            {
                this.logger.LogError("Null or empty session token");
                return new JsonResult(new ApiResponse<string>
                {
                    Success = false,
                    ErrorCode = EErrorCode.ErrorSessionTokenIsNullOrEmpty,
                    ErrorMessage = Localization.ErrorLoginVerificationFailed
                });
            }

            return new JsonResult(await this.VerifySessionAsync(sessionToken).ConfigureAwait(false));
        }

        /// <summary>
        /// Logout the users from the app
        /// </summary>
        /// <param name="ignoreDirty">Logout even if there are unsaved changes to the project</param>
        /// <returns>The resulting json</returns>
        public IActionResult OnPostLogout([FromBody] bool ignoreDirty)
        {
            try
            {
                if (this.projectService.NeedSave() && !ignoreDirty)
                {
                    return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.ErrorProjectNeedToBeSaved });
                }

                this.accountService.Logout();
                return new JsonResult(new ApiResponse());
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error logging out");
                return new JsonResult(new ApiResponse { Success = false, ErrorCode = EErrorCode.ErrorUnexpected, ErrorMessage = Localization.ErrorGeneric });
            }
        }

        private async Task<ApiResponse<string>> VerifySessionAsync(string sessionToken)
        {
            try
            {
                await this.accountService.VerifySession(sessionToken).ConfigureAwait(false);
                if (this.accountService.CustomerInformation.SubscriptionPlan == null)
                {
                    this.logger.LogError("No subscription plan");
                    return new ApiResponse<string>
                    {
                        Success = false,
                        ErrorCode = EErrorCode.ErrorNoSubscriptionAvailable,
                        ErrorMessage = Localization.ErrorNoSubscriptionAvailable,
                        Result = sessionToken,
                    };
                }

                if (!this.accountService.CustomerInformation.ExpirationDate.HasValue || this.accountService.CustomerInformation.ExpirationDate.Value < DateTime.Now)
                {
                    if (this.accountService.CustomerInformation.IsTrial.HasValue && this.accountService.CustomerInformation.IsTrial.Value)
                    {
                        this.logger.LogError("Expired trial subscription");
                        return new ApiResponse<string>
                        {
                            Success = false,
                            ErrorCode = EErrorCode.ErrorTrialSubscriptionExpired,
                            ErrorMessage = Localization.ErrorTrialSubscriptionExpired,
                            Result = sessionToken,
                        };
                    }

                    this.logger.LogError("Expired subscription");
                    return new ApiResponse<string>
                    {
                        Success = false,
                        ErrorCode = EErrorCode.ErrorSubscriptionExpired,
                        ErrorMessage = Localization.ErrorSubscriptionExpired,
                        Result = sessionToken,
                    };
                }

                var settings = this.appSettingsService.GetAppSettings();
                settings.Session = sessionToken;
                this.appSettingsService.SaveAppSettings();

                return new ApiResponse<string> { Success = true };
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
                    case EErrorCode.ErrorApplicationUpdateNeeded:
                        error = string.Format(CultureInfo.InvariantCulture, Localization.ErrorApplicationUpdateNeeded, this.appGlobals.ProductName);
                        break;
                    default:
                        error = Localization.ErrorGeneric;
                        break;
                }

                return new ApiResponse<string>
                {
                    Success = false,
                    ErrorCode = ex.ErrorCode,
                    ErrorMessage = error
                };
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Error occured in index: {ex.Message}");
                return new ApiResponse<string>
                {
                    Success = false,
                    ErrorCode = EErrorCode.ErrorUnexpected,
                    ErrorMessage = Localization.ErrorLoginVerificationFailed
                };
            }
        }
    }
}