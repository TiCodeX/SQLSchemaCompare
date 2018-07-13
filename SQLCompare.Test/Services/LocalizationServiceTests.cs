using FluentAssertions;
using SQLCompare.Core.Enums;
using SQLCompare.Services;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace SQLCompare.Test.Services
{
    /// <summary>
    /// Test class for the LocalizationService
    /// </summary>
    public class LocalizationServiceTests : BaseTests<LocalizationServiceTests>
    {
        private readonly LocalizationService localizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizationServiceTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper</param>
        public LocalizationServiceTests(ITestOutputHelper output)
            : base(output)
        {
            this.localizationService = new LocalizationService();
            this.localizationService.Init(Language.English);
        }

        /// <summary>
        /// Test changing the language
        /// </summary>
        [Fact]
        [UnitTest]
        public void LocalizationTest()
        {
            this.localizationService.SetLanguage(Language.English);

            Localization.ButtonNewProject.Should().Be("New Project");
            Localization.ButtonCancel.Should().Be("Cancel");

            var dict = this.localizationService.GetLocalizationDictionary();
            dict.Should().ContainKey(nameof(Localization.ButtonNewProject));
            dict[nameof(Localization.ButtonNewProject)].Should().Be("New Project");
            dict.Should().ContainKey(nameof(Localization.ButtonCancel));
            dict[nameof(Localization.ButtonCancel)].Should().Be("Cancel");

            this.localizationService.SetLanguage(Language.Italian);

            Localization.ButtonNewProject.Should().Be("Nuovo Progetto");
            Localization.ButtonCancel.Should().Be("Annulla");

            dict = this.localizationService.GetLocalizationDictionary();
            dict.Should().ContainKey(nameof(Localization.ButtonNewProject));
            dict[nameof(Localization.ButtonNewProject)].Should().Be("Nuovo Progetto");
            dict.Should().ContainKey(nameof(Localization.ButtonCancel));
            dict[nameof(Localization.ButtonCancel)].Should().Be("Annulla");

            this.localizationService.SetLanguage(Language.English);
        }
    }
}
