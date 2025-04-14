namespace Jobs.Core.Contracts;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cypherText);
}