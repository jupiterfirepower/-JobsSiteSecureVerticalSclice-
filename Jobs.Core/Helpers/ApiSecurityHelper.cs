using Jobs.Common.Constants;
using Jobs.Core.Contracts;
using Microsoft.AspNetCore.Http;

namespace Jobs.Core.Helpers;

public static class ApiSecurityHelper
{
    public static bool IsBadRequest(IHttpContextAccessor httpContextAccessor, 
        IEncryptionService cryptService,
        ISignedNonceService signedNonceService,
        IApiKeyService service,
        string apiKey, 
        string signedNonce,
        string apiSecret)
    {
        if (!UserAgentConstant.AppUserAgent.Equals(httpContextAccessor.HttpContext?.Request.Headers.UserAgent))
        {
            return true;
        }

        var signedNonceReal = cryptService.Decrypt(signedNonce);
        var (longNonce , resultParse) = signedNonceService.IsSignedNonceValid(signedNonceReal);

        /*if (builder.Environment.IsDevelopment())
        {
            longNonce = DateTime.Now.Ticks;
        }*/

        if (!resultParse)
        {
            Console.WriteLine($"ApiSecurityHelper resultParse : false");
            return true;
        }
            
        // apiKey must be in Base64
        var realApiKey = cryptService.Decrypt(apiKey);
        var realApiSecret = cryptService.Decrypt(apiSecret);
            
        if (!service.IsValid(realApiKey, longNonce, realApiSecret))
        {
            return true;
        }

        return false;
    }
    
    public static bool IsTrustBadRequest(IHttpContextAccessor httpContextAccessor, 
        IEncryptionService cryptService,
        ISignedNonceService signedNonceService,
        IApiKeyService service,
        string apiKey, 
        string signedNonce,
        string apiSecret)
    {
        if (!UserAgentConstant.AppUserAgent.Equals(httpContextAccessor.HttpContext?.Request.Headers.UserAgent))
        {
            Console.WriteLine("User Agent : " + httpContextAccessor.HttpContext?.Request.Headers.UserAgent);
            return true;
        }
        
        /*var currentUserAgent = httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
        if (currentUserAgent != null && !currentUserAgent.Contains(UserAgentConstant.AppUserAgent))
        {
            Console.WriteLine("User Agent : " + httpContextAccessor.HttpContext?.Request.Headers.UserAgent);
            return true;
        }*/
        
        var signedNonceReal = cryptService.Decrypt(signedNonce);
        var (longNonce , resultParse) = signedNonceService.IsSignedNonceValid(signedNonceReal);
        Console.WriteLine($"longNonce : {longNonce}, resultParse : {resultParse}");
        longNonce = DateTime.UtcNow.Ticks;
        //Console.WriteLine("User Agent : " + httpContextAccessor.HttpContext?.Request.Headers.UserAgent);
        /*if (builder.Environment.IsDevelopment())
        {
            longNonce = DateTime.Now.Ticks;
        }*/

        if (!resultParse)
        {
            return true;
        }
            
        // apiKey must be in Base64
        var realApiKey = cryptService.Decrypt(apiKey);
        var realApiSecret = cryptService.Decrypt(apiSecret);

        if (!service.IsTrustValid(realApiKey, longNonce, realApiSecret))
        {
            return true;
        }

        return false;
    }
}