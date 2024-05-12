

using System.Security.Cryptography;

namespace Token.Services;

public class AesHelper
{

    public static string Encrypt(string text, Aes aes)
    {
        aes.GenerateIV();
        var symmetricEncryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using (var memoryStream = new MemoryStream())
        {
            memoryStream.Write(aes.IV, 0, aes.IV.Length);

            using (var cryptoStream = new CryptoStream(memoryStream, symmetricEncryptor, CryptoStreamMode.Write))
            {
                using (var streamWriter = new StreamWriter(cryptoStream))
                {
                    streamWriter.Write(text);
                }
            }
            return Convert.ToBase64String(memoryStream.ToArray());
        }
    }

    public static string Decrypt(string encryptedText, Aes aes)
    {
        var encryptedBytes = Convert.FromBase64String(encryptedText);

        var iv = new byte[aes.BlockSize / 8];
        Array.Copy(encryptedBytes, iv, iv.Length);

        var symmetricDecryptor = aes.CreateDecryptor(aes.Key, iv);

        using (var memoryStream = new MemoryStream(encryptedBytes, iv.Length, encryptedBytes.Length - iv.Length))
        {
            using (var cryptoStream = new CryptoStream(memoryStream, symmetricDecryptor, CryptoStreamMode.Read))
            {
                using (var streamReader = new StreamReader(cryptoStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
    }
}