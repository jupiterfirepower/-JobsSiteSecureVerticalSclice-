using PasswordGenerator;

namespace Jobs.Common.Helpers;

public class PasswordSaltGeneratorHelper
{
    private readonly Random _rnd = new Random(new Random().Next());
    
    public string GeneratePassword()
    {
        var passwordLength = _rnd.Next(25, 50);
        var pwd = new Password(passwordLength)
            .IncludeLowercase()
            .IncludeUppercase()
            .IncludeNumeric()
            .IncludeSpecial();
        return pwd.Next();
    }
    
    public string GenerateKeycloakGooglePassword()
    {
        var passwordLength = _rnd.Next(20, 30);
        var pwd = new Password(passwordLength)
            .IncludeLowercase()
            .IncludeUppercase()
            .IncludeNumeric()
            .IncludeSpecial();
        return pwd.Next();
    }
    
    public string GenerateSalt()
    {
        var saltLength = _rnd.Next(15, 35);
        var salt = new Password(saltLength)
            .IncludeLowercase()
            .IncludeUppercase()
            .IncludeNumeric();
        return salt.Next();
    }
}