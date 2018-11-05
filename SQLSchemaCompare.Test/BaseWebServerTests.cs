using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using TiCodeX.SQLSchemaCompare.Core.Enums;
using TiCodeX.SQLSchemaCompare.Services;
using TiCodeX.SQLSchemaCompare.UI.WebServer;
using Xunit;
using Xunit.Abstractions;

namespace TiCodeX.SQLSchemaCompare.Test
{
    /// <summary>
    /// Base class for WebServer tests
    /// </summary>
    /// <typeparam name="T">Type for the initialization of the Logger</typeparam>
    public abstract class BaseWebServerTests<T> : BaseTests<T>, IClassFixture<WebApplicationFactory<WebServerStartup>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseWebServerTests{T}"/> class.
        /// </summary>
        /// <param name="webApplicationFactory">The web application factory</param>
        /// <param name="output">The test output helper</param>
        protected BaseWebServerTests(WebApplicationFactory<WebServerStartup> webApplicationFactory, ITestOutputHelper output)
            : base(output)
        {
            this.HttpClient = webApplicationFactory.CreateDefaultClient();
            this.HttpClient.DefaultRequestHeaders.Add("CustomAuthToken", "prova");

            // Ensure the webserver starts with the english localization
            var localizationService = new LocalizationService();
            localizationService.SetLanguage(Language.English);
        }

        /// <summary>
        /// Gets the HttpClient
        /// </summary>
        protected HttpClient HttpClient { get; }
    }
}
