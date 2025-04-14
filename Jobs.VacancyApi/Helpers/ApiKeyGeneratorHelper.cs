using System.Text;
using Jobs.Core.Helpers;
using Konscious.Security.Cryptography;
using PasswordGenerator;

namespace Jobs.VacancyApi.Helpers;

public static class ApiKeyGeneratorHelper
{
    public static string GenerateApiKey()
    {
        var helper = new PasswordSaltGeneratorHelper();
     
        var plainTextPassword = helper.GeneratePassword();
        var salt = helper.GenerateSalt();
        
        var passwordBytes = Encoding.UTF8.GetBytes(plainTextPassword);
        var saltBytes = Encoding.UTF8.GetBytes(salt);
        
        //var par = 1;
        var par = Environment.ProcessorCount;
        var iterations = 1;
        var memCost = 16536;
        var digest=24;

        var argon2Id = new Argon2id(passwordBytes);
        argon2Id.Salt = saltBytes;
        argon2Id.DegreeOfParallelism = par;
        argon2Id.Iterations = iterations;
        argon2Id.MemorySize = memCost;
        var res = argon2Id.GetBytes(digest);

        return Convert.ToHexString(res);
    }
}