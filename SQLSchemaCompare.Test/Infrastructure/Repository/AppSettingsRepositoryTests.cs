namespace TiCodeX.SQLSchemaCompare.Test.Infrastructure.Repository
{
    using System.IO;
    using System.Text.RegularExpressions;
    using FluentAssertions;
    using TiCodeX.SQLSchemaCompare.Core.Entities;
    using TiCodeX.SQLSchemaCompare.Core.Enums;
    using TiCodeX.SQLSchemaCompare.Core.Interfaces;
    using TiCodeX.SQLSchemaCompare.Infrastructure.Repository;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    /// <summary>
    /// Test class for the AppSettingsRepository
    /// </summary>
    public class AppSettingsRepositoryTests : BaseTests<AppSettingsRepositoryTests>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettingsRepositoryTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper</param>
        public AppSettingsRepositoryTests(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Test the read functionality
        /// </summary>
        [Fact]
        [UnitTest]
        public static void Read()
        {
            var appGlobals = new AppGlobals();
            var appSettingsRepository = new AppSettingsRepository(appGlobals);

            const string xmlFile = @"<?xml version=""1.0""?>
<AppSettings xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Language>English</Language>
  <LogLevel>Debug</LogLevel>
  <RecentProjects>
    <string>test.txt</string>
    <string>test2.txt</string>
    <string>test3.txt</string>
  </RecentProjects>
  <Session>TestSession</Session>
</AppSettings>";

            File.WriteAllText(appGlobals.AppSettingsFullFilename, xmlFile);

            var settings = appSettingsRepository.Read();

            settings.Should().NotBeNull();

            settings.Should().BeOfType<AppSettings>();
            settings.Language.Should().Be(Language.English);
            settings.LogLevel.Should().Be(LogLevel.Debug);
            settings.RecentProjects.Should().HaveCount(3);
            settings.RecentProjects.Should().Contain("test.txt");
            settings.Session.Should().Be("TestSession");
        }

        /// <summary>
        /// The the write functionality
        /// </summary>
        [Fact]
        [UnitTest]
        public static void Write()
        {
            var appSettings = new AppSettings
            {
                Language = Language.English,
                LogLevel = LogLevel.Debug,
                Session = "TestSession",
            };
            appSettings.RecentProjects.Add("test.txt");
            appSettings.RecentProjects.Add("test2.txt");
            appSettings.RecentProjects.Add("test3.txt");

            var appGlobals = new AppGlobals();

            new AppSettingsRepository(appGlobals).Write(appSettings);

            var xmlFile = File.ReadAllText(appGlobals.AppSettingsFullFilename);

            var xmlFileExpected = @"<?xml version=""1.0"" encoding=""utf-8""?>
<AppSettings xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Language>English</Language>
  <LogLevel>Debug</LogLevel>
  <RecentProjects>
    <string>test.txt</string>
    <string>test2.txt</string>
    <string>test3.txt</string>
  </RecentProjects>
  <Session>TestSession</Session>
</AppSettings>";

            // Remove line-endings for comparison
            xmlFile = Regex.Replace(xmlFile, "\\r|\\n", string.Empty);
            xmlFileExpected = Regex.Replace(xmlFileExpected, "\\r|\\n", string.Empty);

            xmlFile.Should().Be(xmlFileExpected);
        }

        /// <summary>
        /// The app globals
        /// </summary>
        private class AppGlobals : IAppGlobals
        {
            /// <summary>
            /// The temp setting file
            /// </summary>
            private static readonly string TempSettingFile = Path.GetTempFileName();

            /// <summary>
            /// Gets the company name
            /// </summary>
            public string CompanyName => throw new System.NotImplementedException();

            /// <summary>
            /// Gets the product name
            /// </summary>
            public string ProductName => throw new System.NotImplementedException();

            /// <summary>
            /// Gets a value indicating whether is development
            /// </summary>
            public bool IsDevelopment => throw new System.NotImplementedException();

            /// <summary>
            /// Gets the authoritation header name
            /// </summary>
            public string AuthorizationHeaderName => throw new System.NotImplementedException();

            /// <summary>
            /// Gets the app settings full filename
            /// </summary>
            public string AppSettingsFullFilename => TempSettingFile;

            /// <summary>
            /// Gets the logger layout
            /// </summary>
            public string LoggerLayout => throw new System.NotImplementedException();

            /// <summary>
            /// Gets the logger file
            /// </summary>
            public string LoggerFile => throw new System.NotImplementedException();

            /// <summary>
            /// Gets the logger max archive files
            /// </summary>
            public int LoggerMaxArchiveFiles => throw new System.NotImplementedException();

            /// <summary>
            /// Gets the electron auth app id
            /// </summary>
            public string ElectronAuthAppId => throw new System.NotImplementedException();

            /// <summary>
            /// Gets the product code
            /// </summary>
            public string ProductCode => throw new System.NotImplementedException();

            /// <summary>
            /// Gets or sets the app version
            /// </summary>
            public string AppVersion { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        }
    }
}
