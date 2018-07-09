using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SQLCompare.UI.WebServer;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace SQLCompare.Test.UI
{
    /// <summary>
    /// AAAAA
    /// </summary>
    public class WelcomePageTests : BaseTests<WelcomePageTests>, IClassFixture<WebApplicationFactory<WebServerStartup>>
    {
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomePageTests"/> class
        /// </summary>
        /// <param name="webApplicationFactory">The web application factory</param>
        /// <param name="output">The test output helper</param>
        public WelcomePageTests(WebApplicationFactory<WebServerStartup> webApplicationFactory, ITestOutputHelper output)
            : base(output)
        {
            this.httpClient = webApplicationFactory.CreateDefaultClient();
        }

        /// <summary>
        /// Test the connection to the web service
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [UnitTest]
        public async Task ConnectionTest()
        {
            var response = await this.httpClient.GetAsync(new Uri("https://127.0.0.1:5000")).ConfigureAwait(false);
            response.IsSuccessStatusCode.Should().BeTrue();
        }

        /// <summary>
        /// Test the loading of the WelcomePage
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [UnitTest]
        public async Task WelcomePageLoadTest()
        {
            var response = await this.httpClient.GetAsync(new Uri("https://127.0.0.1:5000/WelcomePageModel")).ConfigureAwait(false);
            response.IsSuccessStatusCode.Should().BeTrue();

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            content.Should().Contain("Welcome to TiCodeX SA SQL Compare");
        }
    }
}
