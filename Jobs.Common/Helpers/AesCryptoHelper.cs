using System.Security.Cryptography;

namespace Jobs.Common.Helpers;

public class AesCryptoHelper
{
    public static byte[] Encrypt(byte[] plainBytes, byte[] key, byte[] iv)
    {
        byte[] encryptedBytes = null;

        // Set up the encryption objects
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Encrypt the input plaintext using the AES algorithm
            using (ICryptoTransform encryptor = aes.CreateEncryptor())
            {
                encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            }
        }

        return encryptedBytes;
    }
    
    public static byte[] Decrypt(byte[] cipherBytes, byte[] key, byte[] iv)
    {
        byte[] decryptedBytes = null;

        // Set up the encryption objects
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Decrypt the input ciphertext using the AES algorithm
            using (ICryptoTransform decryptor = aes.CreateDecryptor())
            {
                decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            }
        }

        return decryptedBytes;
    }
    
    private static string EncryptDataWithAes(string plainText, string keyBase64, out string vectorBase64)
    {
        using (Aes aesAlgorithm = Aes.Create())
        {
            aesAlgorithm.Key = Convert.FromBase64String(keyBase64);
            aesAlgorithm.GenerateIV();
            Console.WriteLine($"Aes Cipher Mode : {aesAlgorithm.Mode}");
            Console.WriteLine($"Aes Padding Mode: {aesAlgorithm.Padding}");
            Console.WriteLine($"Aes Key Size : {aesAlgorithm.KeySize}");

            //set the parameters with out keyword
            vectorBase64 = Convert.ToBase64String(aesAlgorithm.IV);

            // Create encryptor object
            ICryptoTransform encryptor = aesAlgorithm.CreateEncryptor();

            byte[] encryptedData;

            //Encryption will be done in a memory stream through a CryptoStream object
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    encryptedData = ms.ToArray();
                }
            }

            return Convert.ToBase64String(encryptedData);
        }
    }
    private static string DecryptDataWithAes(string cipherText, string keyBase64, string vectorBase64)
    {
        using (Aes aesAlgorithm = Aes.Create())
        {
            aesAlgorithm.Key = Convert.FromBase64String(keyBase64);
            aesAlgorithm.IV = Convert.FromBase64String(vectorBase64);

            Console.WriteLine($"Aes Cipher Mode : {aesAlgorithm.Mode}");
            Console.WriteLine($"Aes Padding Mode: {aesAlgorithm.Padding}");
            Console.WriteLine($"Aes Key Size : {aesAlgorithm.KeySize}");
            Console.WriteLine($"Aes Block Size : {aesAlgorithm.BlockSize}");


            // Create decryptor object
            ICryptoTransform decryptor = aesAlgorithm.CreateDecryptor();

            byte[] cipher = Convert.FromBase64String(cipherText);

            //Decryption will be done in a memory stream through a CryptoStream object
            using (MemoryStream ms = new MemoryStream(cipher))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }
}