using System;
using System.Net.Http;
using System.Threading.Tasks;
using SQLCompare.Core.Entities.Api;
using SQLCompare.Core.Entities.Exceptions;
using SQLCompare.Core.Interfaces;
using SQLCompare.Core.Interfaces.Services;

namespace SQLCompare.Services
{
    /// <summary>
    /// Defines the service that handles customer account requests
    /// </summary>
    public class CustomerAccountService : IAccountService
    {
        private readonly IAppGlobals appGlobals;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerAccountService"/> class
        /// </summary>
        /// <param name="appGlobals">Injected application global constants</param>
        public CustomerAccountService(IAppGlobals appGlobals)
        {
            this.appGlobals = appGlobals;
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
                    using (var response = await client.PostAsJsonAsync(this.appGlobals.VerifySessionEndpoint, sessionToken).ConfigureAwait(false))
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
        public void SendFeedback(int evalutation, string comment)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Logout()
        {
            this.CustomerInformation = null;
        }
    }
}
