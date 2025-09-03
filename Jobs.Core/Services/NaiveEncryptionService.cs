using System.Security.Cryptography;
using System.Text;
using Jobs.Core.Contracts;

namespace Jobs.Core.Services;

public class NaiveEncryptionService(byte[] encryptionKey, byte[] iv) : IEncryptionService
{
    public string Encrypt(string plaintext)
    {
        byte[] encryptedBytes = null;
        
        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.KeySize = 256;
        aes.Key = encryptionKey;
        aes.IV = iv;
       
        
        //var encryptor = aes.CreateEncryptor(encryptionKey, iv);
        
        // Encrypt the input plaintext using the AES algorithm
        using (ICryptoTransform encryptor = aes.CreateEncryptor(encryptionKey, iv))
        {
            var plainBytes = Encoding.UTF8.GetBytes(plaintext);
            encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        }
        
        //using var memoryStream = new MemoryStream();
        //using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        //using var streamWriter = new StreamWriter(cryptoStream);
        //streamWriter.Write(plaintext);

        //var cypherTextBytes = memoryStream.ToArray();
            
        //return Convert.ToBase64String(cypherTextBytes);
        return Convert.ToBase64String(encryptedBytes);
    }

    public string Decrypt(string cyphertext)
    {
        byte[] decryptedBytes = null;
        var cypherTextBytes = Convert.FromBase64String(cyphertext);
        
        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.KeySize = 256;
        //aes.Key = encryptionKey;
        //aes.IV = iv;
        
        // Decrypt the input ciphertext using the AES algorithm
        using (ICryptoTransform decryptor = aes.CreateDecryptor(encryptionKey,iv))
        {
            decryptedBytes = decryptor.TransformFinalBlock(cypherTextBytes, 0, cypherTextBytes.Length);
        }
        
        //var decrypt = aes.CreateDecryptor(encryptionKey, iv);
        
        //using var memoryStream = new MemoryStream(cypherTextBytes);
        //using var cryptoStream = new CryptoStream(memoryStream, decrypt, CryptoStreamMode.Read);
        //using var streamReader = new StreamReader(cryptoStream);
        
        //return streamReader.ReadToEnd();
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}