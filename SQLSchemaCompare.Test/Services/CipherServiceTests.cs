using System;
using System.Text;
using FluentAssertions;
using TiCodeX.SQLSchemaCompare.Core.Interfaces.Services;
using TiCodeX.SQLSchemaCompare.Services;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace TiCodeX.SQLSchemaCompare.Test.Services
{
    /// <summary>
    /// Test class for the CipherService
    /// </summary>
    public class CipherServiceTests : BaseTests<CipherServiceTests>
    {
        private readonly ICipherService cipherService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherServiceTests"/> class
        /// </summary>
        /// <param name="output">The test output helper</param>
        public CipherServiceTests(ITestOutputHelper output)
            : base(output)
        {
            this.cipherService = new CipherService();
        }

        /// <summary>
        /// Test encrypting and decrypting multiple random strings
        /// </summary>
        [Fact]
        [UnitTest]
        public void EncryptDecryptRandomStrings()
        {
            var rnd = new Random();
            for (var i = 0; i <= 1000; i++)
            {
                var randomBytes = new byte[i];
                rnd.NextBytes(randomBytes);
                var randomString = Encoding.UTF8.GetString(randomBytes);
                var encryptedString = this.cipherService.EncryptString(randomString);
                var decryptedString = this.cipherService.DecryptString(encryptedString);
                decryptedString.Should().Be(randomString);
            }
        }

        /// <summary>
        /// Test decrypting multiple encrypted string to the same string
        /// </summary>
        [Fact]
        [UnitTest]
        public void DecryptMultipleStringsToSameString()
        {
            const string encryptedString1 = "2eOulskIl/UAgDqElyBpphxswbCNOhKImgQl315cj68=";
            const string encryptedString2 = "qSUWuBM3vN0BAxAV3ql/7IErYcYRfSTd4mF5X64ox1s=";
            const string expectedResult = "test1234";
            var decryptedString1 = this.cipherService.DecryptString(encryptedString1);
            decryptedString1.Should().Be(expectedResult);
            var decryptedString2 = this.cipherService.DecryptString(encryptedString2);
            decryptedString2.Should().Be(expectedResult);
        }
    }
}
