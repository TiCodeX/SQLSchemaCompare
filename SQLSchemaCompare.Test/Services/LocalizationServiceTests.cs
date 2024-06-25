namespace TiCodeX.SQLSchemaCompare.Test.Services
{
    /// <summary>
    /// Test class for the LocalizationService
    /// </summary>
    public class LocalizationServiceTests : BaseTests<LocalizationServiceTests>
    {
        /// <summary>
        /// The localization service
        /// </summary>
        private readonly LocalizationService localizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizationServiceTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper</param>
        public LocalizationServiceTests(ITestOutputHelper output)
            : base(output)
        {
            this.localizationService = new LocalizationService();
            this.localizationService.SetLanguage(Language.English);
        }

        /// <summary>
        /// Test changing the language
        /// </summary>
        [Fact]
        [UnitTest]
        public void LocalizationTest()
        {
            try
            {
                this.localizationService.SetLanguage(Language.English);

                Localization.ButtonNewProject.Should().Be("New Project");
                Localization.ButtonCancel.Should().Be("Cancel");

                this.localizationService.SetLanguage(Language.Italian);

                Localization.ButtonNewProject.Should().Be("Nuovo Progetto");
                Localization.ButtonCancel.Should().Be("Annulla");
            }
            finally
            {
                // Restore the English language
                this.localizationService.SetLanguage(Language.English);
            }
        }

        /// <summary>
        /// Test getting the translation of specific language
        /// </summary>
        [Fact]
        [UnitTest]
        public void GetSpecificStringTest()
        {
            try
            {
                this.localizationService.SetLanguage(Language.English);
                this.localizationService.GetString("ButtonNewProject", Localization.Culture).Should().Be("New Project");
                this.localizationService.SetLanguage(Language.Italian);
                this.localizationService.GetString("ButtonNewProject", Localization.Culture).Should().Be("Nuovo Progetto");

                this.localizationService.GetString("ButtonNewProject", CultureInfo.GetCultureInfo("en")).Should().Be("New Project");
                this.localizationService.GetString("ButtonNewProject", CultureInfo.GetCultureInfo("it")).Should().Be("Nuovo Progetto");
                this.localizationService.GetString("ButtonNewProject", CultureInfo.GetCultureInfo("cz")).Should().Be("[[ButtonNewProject]]");
            }
            finally
            {
                // Restore the English language
                this.localizationService.SetLanguage(Language.English);
            }
        }

        /// <summary>
        /// Test the GetLocalizationDictionary
        /// </summary>
        [Fact]
        [UnitTest]
        public void GetLocalizationDictionaryTest()
        {
            try
            {
                this.localizationService.SetLanguage(Language.English);

                var dict = this.localizationService.GetLocalizationDictionary();
                dict.Should().ContainKey(nameof(Localization.ButtonNewProject));
                dict[nameof(Localization.ButtonNewProject)].Should().Be("New Project");
                dict.Should().ContainKey(nameof(Localization.ButtonCancel));
                dict[nameof(Localization.ButtonCancel)].Should().Be("Cancel");

                this.localizationService.SetLanguage(Language.Italian);

                dict = this.localizationService.GetLocalizationDictionary();
                dict.Should().ContainKey(nameof(Localization.ButtonNewProject));
                dict[nameof(Localization.ButtonNewProject)].Should().Be("Nuovo Progetto");
                dict.Should().ContainKey(nameof(Localization.ButtonCancel));
                dict[nameof(Localization.ButtonCancel)].Should().Be("Annulla");
            }
            finally
            {
                // Restore the English language
                this.localizationService.SetLanguage(Language.English);
            }
        }

        /// <summary>
        /// Checks the tokens used in TypeScript if they are present in the resource file
        /// </summary>
        [Fact]
        [UnitTest]
        public void CheckTypeScriptTokens()
        {
            this.localizationService.SetLanguage(Language.English);
            var dict = this.localizationService.GetLocalizationDictionary();

            // Find the root path of the solution
            var solutionDir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (solutionDir != null && !solutionDir.GetFiles("*.sln").Any())
            {
                solutionDir = solutionDir.Parent;
            }

            var expr = new Regex("Localization\\.Get\\(\\\"(?<token>\\w+)\\\"\\)");
            foreach (var file in Directory.EnumerateFiles(Path.Combine(solutionDir.FullName, "SQLSchemaCompare.UI", "wwwroot", "js"), "*.ts", SearchOption.AllDirectories))
            {
                var content = File.ReadAllText(file);
                foreach (Match x in expr.Matches(content))
                {
                    var token = x.Groups["token"].Value;
                    dict.Should().ContainKey(token);
                }
            }
        }
    }
}
