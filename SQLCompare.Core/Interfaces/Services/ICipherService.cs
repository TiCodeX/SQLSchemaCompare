namespace SQLCompare.Core.Interfaces.Services
{
    /// <summary>
    /// Defines a class that provides the mechanisms to encrypt/decrypt strings
    /// </summary>
    public interface ICipherService
    {
        /// <summary>
        /// Encrypts the string
        /// </summary>
        /// <param name="text">The text to encrypt</param>
        /// <returns>The encrypted string</returns>
        string EncryptString(string text);

        /// <summary>
        /// Decrypts the string
        /// </summary>
        /// <param name="text">The text to decrypt</param>
        /// <returns>The decrypted string</returns>
        string DecryptString(string text);
    }
}
