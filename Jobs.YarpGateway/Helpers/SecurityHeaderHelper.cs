using Jobs.Common.Constants;
using Jobs.Common.Helpers;
using NUlid;

namespace Jobs.YarpGateway.Helpers;

public static class SecurityHeaderHelper
{
    public static bool IsHeadersValid(HttpContext context) => IsUserAgentValid(context)
                                                              && IsAppIdValid(context)
                                                              && IsNonceValid(context)
                                                              && IsSecureKeyValid(context);

    private static bool IsUserAgentValid(HttpContext context)
    {
        var userAgent = context.Request.Headers.UserAgent.FirstOrDefault();
        bool isUlidValid = Ulid.TryParse(userAgent, out Ulid _); // ULID
        Console.WriteLine($"userAgent: {userAgent} isUlidValid: {isUlidValid}");
        return !String.IsNullOrEmpty(userAgent) && isUlidValid;
    }

    private static bool IsAppIdValid(HttpContext context)
    {
        var appId = context.Request.Headers[HttpHeaderKeys.XAppIdHeaderKey].FirstOrDefault();
        bool isAppIdGuidValid = Guid.TryParse(appId, out var appIdGuid);
        var version = GuidDecoder.GetVersion(appIdGuid); // AppId version 7
        GuidDecoder.TryDecodeTimestamp(appIdGuid, out var timestamp);
        var timeSpan = DateTimeOffset.UtcNow.Subtract(timestamp);
        Console.WriteLine($"timeSpan: {timeSpan}");
        Console.WriteLine($"appId: {appId} isAppIdGuidValid: {isAppIdGuidValid} version {version} timeSpan: {timeSpan.Hours}");
        return !String.IsNullOrEmpty(appId) && isAppIdGuidValid && version == 7 && timeSpan < new TimeSpan(0,24,0,0);
    }

    private static bool IsNonceValid(HttpContext context)
    {
        var nonce = context.Request.Headers[HttpHeaderKeys.XApiNonceHeaderKey].FirstOrDefault();
        Console.WriteLine($"nonce: {nonce}");
        return !String.IsNullOrEmpty(nonce);
    }

    private static bool IsSecureKeyValid(HttpContext context)
    {
        var secretKey = context.Request.Headers[HttpHeaderKeys.XApiSecretHeaderKey].FirstOrDefault();
        bool isSecretGuidValid = Guid.TryParse(secretKey, out var secretGuid);
        var versionSecretGuidSecret = GuidDecoder.GetVersion(secretGuid); // Secure Key version 8
        Console.WriteLine($"secretKey: {secretKey} isSecretGuidValid {isSecretGuidValid} version {versionSecretGuidSecret}");
        
        return !String.IsNullOrEmpty(secretKey)
               && isSecretGuidValid
               && versionSecretGuidSecret == 8;
    }
}