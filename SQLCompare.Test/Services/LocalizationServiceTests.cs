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
        /// Test changing language
        /// </summary>
        [Fact]
        [UnitTest]
        public void SetLanguageTest()
        {
            Assert.Equal("New Project", Localization.ButtonNewProject);
            Assert.Equal("Cancel", Localization.ButtonCancel);

            this.localizationService.SetLanguage(Language.Italian);

            Assert.Equal("Nuovo Progetto", Localization.ButtonNewProject);
            Assert.Equal("Annulla", Localization.ButtonCancel);
        }

        /// <summary>
        /// Test the GetLocalizationDictionary
        /// </summary>
        [Fact]
        [UnitTest]
        public void GetLocalizationDictionaryTest()
        {
            var dict = this.localizationService.GetLocalizationDictionary();

            Assert.Equal("New Project", dict[nameof(Localization.ButtonNewProject)]);
            Assert.Equal("Compare Now", dict[nameof(Localization.ButtonCompareNow)]);
        }
    }
}
