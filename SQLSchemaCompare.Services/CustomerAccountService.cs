using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web;
using TiCodeX.SQLSchemaCompare.Core.Entities.Api;
using TiCodeX.SQLSchemaCompare.Core.Entities.Exceptions;
using TiCodeX.SQLSchemaCompare.Core.Interfaces;
using TiCodeX.SQLSchemaCompare.Core.Interfaces.Services;

namespace TiCodeX.SQLSchemaCompare.Services
{
    /// <summary>
    /// Defines the service that handles customer account requests
    /// </summary>
    public class CustomerAccountService : IAccountService
    {
        private readonly IAppGlobals appGlobals;
        private readonly IProjectService projectService;
        private readonly IAppSettingsService appSettingsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerAccountService"/> class
        /// </summary>
        /// <param name="appGlobals">Injected application global constants</param>
        /// <param name="projectService">The injected project service</param>
        /// <param name="appSettingsService">The injected app settings service</param>
        public CustomerAccountService(IAppGlobals appGlobals, IProjectService projectService, IAppSettingsService appSettingsService)
        {
            this.appGlobals = appGlobals;
            this.projectService = projectService;
            this.appSettingsService = appSettingsService;
        }

        /// <inheritdoc/>
        public VerifySessionResult CustomerInformation { get; private set; }

        /// <inheritdoc/>
        public async Task VerifySession(string sessionToken)
        {
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                using (var client = new HttpClient(httpClientHandler))
                {
                    var request = new VerifySessionRequest
                    {
                        SessionToken = sessionToken,
                        AppVersion = this.appGlobals.AppVersion,
                        ProductCode = this.appGlobals.ProductCode,
                        OS = GetOSString(),
                    };

                    using (var response = await client.PostAsJsonAsync(this.appGlobals.VerifySessionEndpoint, request).ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                        var results = await response.Content.ReadAsAsync<ApiResponse<VerifySessionResult>>().ConfigureAwait(false);
                        if (!results.Success)
                        {
                            throw new AccountServiceException(results.ErrorMessage) { ErrorCode = results.ErrorCode };
                        }

                        this.CustomerInformation = results.Result;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async Task SendFeedback(string sessionToken, int? rating, string comment)
        {
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                using (var client = new HttpClient(httpClientHandler))
                {
                    var request = new SendFeedbackRequest
                    {
                        SessionToken = HttpUtility.UrlDecode(sessionToken),
                        Rating = rating,
                        Comment = comment,
                        AppVersion = this.appGlobals.AppVersion,
                        ProductCode = this.appGlobals.ProductCode,
                        OS = GetOSString(),
                    };

                    using (var response = await client.PostAsJsonAsync(this.appGlobals.SaveFeedbackEndpoint, request).ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                        var results = await response.Content.ReadAsAsync<ApiResponse>().ConfigureAwait(false);
                        if (!results.Success)
                        {
                            throw new AccountServiceException(results.ErrorMessage) { ErrorCode = results.ErrorCode };
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void Logout()
        {
            this.projectService.CloseProject();

            this.appSettingsService.GetAppSettings().Session = null;
            this.appSettingsService.SaveAppSettings();

            this.CustomerInformation = null;
        }

        private static string GetOSString()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "Windows";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "OSX";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "Linux";
            }
            else
            {
                return "Other";
            }
        }
    }
}
