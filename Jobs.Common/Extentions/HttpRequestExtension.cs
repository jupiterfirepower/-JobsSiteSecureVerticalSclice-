using Microsoft.AspNetCore.Http;

namespace Jobs.Common.Extentions;

public static class HttpRequestExtension
{
    public static string GetIpAddress(this HttpRequest request)
    {
        var ipAddress = request?.Headers?["X-Real-IP"].ToString();

        if(!string.IsNullOrEmpty(ipAddress))
        {
            return ipAddress;
        }

        ipAddress = request?.Headers?["X-Forwarded-For"].ToString();

        if (!string.IsNullOrEmpty(ipAddress))
        {
            var parts = ipAddress.Split(',');
            return parts.Any() ? parts[0] : ipAddress;
        }
        
        ipAddress = request?.Headers?["X-Coming-From"].ToString();
        
        if (!string.IsNullOrEmpty(ipAddress))
        {
            var parts = ipAddress.Split(',');
            return parts.Any() ? parts[0] : ipAddress;
        }
        
        ipAddress = request?.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        return !string.IsNullOrEmpty(ipAddress) ? ipAddress : string.Empty;
    }
}