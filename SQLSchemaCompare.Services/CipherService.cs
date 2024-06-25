namespace TiCodeX.SQLSchemaCompare.Services
{
    using System.Security.Cryptography;

    /// <summary>
    /// Implementation that provides the mechanisms to encrypt/decrypt strings
    /// </summary>
    public class CipherService : ICipherService
    {
        /// <summary>
        /// Gets the secret key
        /// </summary>
        private static byte[] Key { get; } = Encoding.UTF8.GetBytes("FischersFritzFischtFrischeFische");

        /// <inheritdoc/>
        [SuppressMessage("Security", "CA5401:Do not use CreateEncryptor with non-default IV", Justification = "IV is necessary here")]
        public string EncryptString(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            using (var aesAlg = Aes.Create())
            {
                if (aesAlg == null)
                {
                    throw new NotSupportedException();
                }

                using (var encryptor = aesAlg.CreateEncryptor(Key, aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }

                        var iv = aesAlg.IV;
                        var encryptedContent = msEncrypt.ToArray();

                        var result = new byte[iv.Length + encryptedContent.Length];
                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(encryptedContent, 0, result, iv.Length, encryptedContent.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public string DecryptString(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var fullCipher = Convert.FromBase64String(text);

            var iv = new byte[16];
            var cipher = new byte[fullCipher.Length - iv.Length];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, fullCipher.Length - iv.Length);

            using (var aesAlg = Aes.Create())
            {
                if (aesAlg == null)
                {
                    throw new NotSupportedException();
                }

                using (var decryptor = aesAlg.CreateDecryptor(Key, iv))
                {
                    string result;
                    using (var msDecrypt = new MemoryStream(cipher))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                result = srDecrypt.ReadToEnd();
                            }
                        }
                    }

                    return result;
                }
            }
        }
    }
}
