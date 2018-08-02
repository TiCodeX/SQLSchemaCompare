using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SQLCompare.Core.Entities.AccountService;
using SQLCompare.Core.Interfaces.Services;

namespace SQLCompare.Services
{
    /// <summary>
    /// Defines the service that handles customer account requests
    /// </summary>
    public class AccountService : IAccountService
    {
        private const string CreateAccountEndpoint = "http://localhost:7071/api/CreateAccount";
        /* private const string LoginEndpoint = "http://localhost:7071/api/Login"; */

        /// <inheritdoc/>
        public LoginResult CustomerInformation { get; private set; }

        /// <inheritdoc/>
        public async Task CreateAccountAsync(string email, string password, bool startTrial, string productName)
        {
            using (var client = new HttpClient())
            {
                var request = new CreateAccountRequest
                {
                    Email = email,
                    Password = password,
                    StartTrial = startTrial,
                    ProductName = productName,
                };

                using (HttpResponseMessage response = await client.PostAsJsonAsync(CreateAccountEndpoint, request).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    var results = await response.Content.ReadAsAsync<CreateAccountResponse>().ConfigureAwait(false);
                }
            }

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Login(string email, string password, string productName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendFeedback(int evalutation, string comment)
        {
            throw new NotImplementedException();
        }
    }
}
