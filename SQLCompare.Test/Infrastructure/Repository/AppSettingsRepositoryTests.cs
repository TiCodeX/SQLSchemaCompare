using System.IO;
using FluentAssertions;
using SQLCompare.Core.Entities;
using SQLCompare.Core.Enums;
using SQLCompare.Core.Interfaces;
using SQLCompare.Infrastructure.Repository;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace SQLCompare.Test.Infrastructure.Repository
{
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

            const string xmlFileExpected = @"<?xml version=""1.0""?>
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

            xmlFile.Should().Be(xmlFileExpected);
        }

        private class AppGlobals : IAppGlobals
        {
            private static readonly string TempSettingFile = Path.GetTempFileName();

            public string CompanyName => throw new System.NotImplementedException();

            public string ProductName => throw new System.NotImplementedException();

            public bool IsDevelopment => throw new System.NotImplementedException();

            public string AuthorizationHeaderName => throw new System.NotImplementedException();

            public string AppSettingsFullFilename => TempSettingFile;

            public string LoggerLayout => throw new System.NotImplementedException();

            public string LoggerFile => throw new System.NotImplementedException();

            public int LoggerMaxArchiveFiles => throw new System.NotImplementedException();

            public string ElectronAuthAppId => throw new System.NotImplementedException();

            public string ProductCode => throw new System.NotImplementedException();

            public string MyAccountEndpoint => throw new System.NotImplementedException();

            public string LoginEndpoint => throw new System.NotImplementedException();

            public string SubscribeEndpoint => throw new System.NotImplementedException();

            public string VerifySessionEndpoint => throw new System.NotImplementedException();
        }
    }
}
