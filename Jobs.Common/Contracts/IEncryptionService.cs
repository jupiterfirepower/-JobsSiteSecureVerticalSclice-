namespace Jobs.Common.Contracts;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cypherText);
}