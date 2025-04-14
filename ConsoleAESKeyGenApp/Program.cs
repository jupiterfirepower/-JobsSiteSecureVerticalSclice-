// See https://aka.ms/new-console-template for more information

using System.Net.Mime;
using System.Security.Cryptography;
using FS.Keycloak.RestApiClient.Api;
using FS.Keycloak.RestApiClient.Authentication.ClientFactory;
using FS.Keycloak.RestApiClient.Authentication.Flow;
using FS.Keycloak.RestApiClient.ClientFactory;
using Jobs.Common.Helpers;
using Jobs.Core.Services;
using NanoidDotNet;
using NUlid;

var appId = new GuidV7();
var secretKey = GuidV8.NewGuidV8(1, 3, 28, 32);

Console.WriteLine(MediaTypeNames.Application.Json);
Console.WriteLine("ULID : " + Ulid.NewUlid());
Console.WriteLine("AppId : " + appId.Value.ToString());
Console.WriteLine("SecretKey : " + secretKey.ToString());
var ticks1 = DateTime.UtcNow.Ticks;
var nonce = ticks1.ToString();
Console.WriteLine("nonce : " + nonce);
var nanoId = Nanoid.Generate(Nanoid.Alphabets.LowercaseLettersAndDigits);
Console.WriteLine("nanoId : " + nanoId);

Console.WriteLine("Creating Aes Encryption 256 bit key");

var workTypes = new List<int>();
var paramWorkTypes = string.Join(",", workTypes);
Console.WriteLine($"paramWorkTypes - {paramWorkTypes}");
using (Aes aesAlgorithm = Aes.Create())
{
    aesAlgorithm.KeySize = 256;
    aesAlgorithm.GenerateKey();
    string keyBase64 = Convert.ToBase64String(aesAlgorithm.Key);
    Console.WriteLine($"Aes Key Size : {aesAlgorithm.KeySize}");
    Console.WriteLine("Here is the Aes key in Base64:");
    Console.WriteLine("Key - " + keyBase64);
    string ivBase64 = Convert.ToBase64String(aesAlgorithm.IV.ToArray());
    Console.WriteLine("IV - " + ivBase64);

    var testKey = "123456789123456789123";
    var guid = Guid.NewGuid();

    Console.WriteLine("TestKey: " + guid);
// ee9d5db0-33cb-4408-ba9c-71289a823ba5
    var key = Convert.FromBase64String("QsDJ84U8bbsrtekK8GvGUqo1Burnfy4qe2BU9OtLtJk=");
    var iv = Convert.FromBase64String("ELoct42LnTjZpfft5Y9TAw==");
    var cryptoProvider = new NaiveEncryptionService(key, iv);
    var result = cryptoProvider.Encrypt(testKey);
    Console.WriteLine(result);
    
    var accountApiSecretKey = "zc32423gfdgbvcxb34wrhyh254t63avdfvfdve2534zxxvasd";
    Console.WriteLine("AccountApiSecretKey.Length - " + accountApiSecretKey.Length);
    var accountSecretKeyResult = cryptoProvider.Encrypt(accountApiSecretKey);
    Console.WriteLine(accountSecretKeyResult);
    
    var defaultApiKey = "e37219fe-5f5e-4c11-9b84-d873d3e58eb1";
    var cryptoProvider1 = new NaiveEncryptionService(key, iv);
    var result1 = cryptoProvider1.Encrypt(defaultApiKey);
    Console.WriteLine("DefaultApiKey : " + result1);
    
    var newAccountApiSecretKey = "gNU7q%!!#BslCtK044m1XV95pFUCAm5JozthqJ+K1aRRN9A3n";
    var newAccountSecretKeyResult = cryptoProvider.Encrypt(newAccountApiSecretKey);
    Console.WriteLine("newAccountSecretKeyResult : " + newAccountSecretKeyResult);

    var accountApiSecretKeyRandom = RandomStringGeneratorHelper.GenRandomString(49);
    Console.WriteLine("accountApiSecretKeyRandom - " + accountApiSecretKeyRandom);

    var v7 = new GuidV7().Value.ToString();
    Console.WriteLine("GuidV7 - " + v7);
    
    var ticks = DateTime.UtcNow.Ticks;
    var sign = ticks * Math.PI * Math.E;
    var rounded = (long)Math.Ceiling(sign);
    var reverseNonce = new string(ticks.ToString().Reverse().ToArray());
        
    var signFirst = ticks * Math.PI;
    var roundedSignFirst = (long)Math.Ceiling(signFirst);
    var signSecond = ticks * Math.E;
    var roundedSignSecond = (long)Math.Ceiling(signSecond);
        
    var roundedSum = roundedSignFirst + roundedSignSecond;
        
    /*int[] intArray = ticks.ToString()
        .ToArray()
        .Select(x=>x.ToString())
        .Select(int.Parse)
        .ToArray(); */
        
    var nonceValue = $"{reverseNonce}-{rounded}-{roundedSum}";
    
    var nonceValueResult = cryptoProvider.Encrypt(nonceValue);
    Console.WriteLine("nonceValueResult : " + nonceValueResult);
   

    var credentials = new PasswordGrantFlow
    {
        KeycloakUrl = "http://localhost:9001",
        Realm = "master",
        UserName = "admin",
        Password = "newpwd"
    };

    using var httpClient = AuthenticationHttpClientFactory.Create(credentials);
    using var usersApi = ApiClientFactory.Create<UsersApi>(httpClient);

    var users = await usersApi.GetUsersAsync("mjobs", username:"jupiter");
    Console.WriteLine($"Users: {users.Count}");
    var credentials1 = new ClientCredentialsFlow
    {
        KeycloakUrl = "http://localhost:9001",
        Realm = "mjobs",
        ClientId = "confmjobs",
        ClientSecret = "SRrvfrHZ5dLcBJG8Qq8Ph8lIYrwtKqzj"
    };

    using var httpClient1 = AuthenticationHttpClientFactory.Create(credentials1);
    using var usersApi1 = ApiClientFactory.Create<UsersApi>(httpClient1);

    var users1 = await usersApi1.GetUsersAsync("mjobs");
    //Console.WriteLine($"Users: {users1.Count}");
}
Console.ReadLine();