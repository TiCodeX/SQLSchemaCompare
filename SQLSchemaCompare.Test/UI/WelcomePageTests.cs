using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TiCodeX.SQLSchemaCompare.UI.WebServer;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace TiCodeX.SQLSchemaCompare.Test.UI
{
    /// <summary>
    /// AAAAA
    /// </summary>
    public class WelcomePageTests : BaseWebServerTests<WelcomePageTests>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomePageTests"/> class.
        /// </summary>
        /// <param name="webApplicationFactory">The web application factory</param>
        /// <param name="output">The test output helper</param>
        public WelcomePageTests(WebApplicationFactory<WebServerStartup> webApplicationFactory, ITestOutputHelper output)
            : base(webApplicationFactory, output)
        {
        }

        /// <summary>
        /// Test the connection to the web service
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact(Skip = "Unable to start webserver")]
        [IntegrationTest]
        public async Task ConnectionTest()
        {
            var response = await this.HttpClient.GetAsync(new Uri("https://127.0.0.1:5000")).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Test the loading of the WelcomePage
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact(Skip = "Unable to start webserver")]
        [IntegrationTest]
        public async Task WelcomePageLoadTest()
        {
            var response = await this.HttpClient.GetAsync(new Uri("https://127.0.0.1:5000/WelcomePageModel")).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            content.Should().Contain("Welcome to TiCodeX SA SQL Schema Compare");
        }
    }
}
