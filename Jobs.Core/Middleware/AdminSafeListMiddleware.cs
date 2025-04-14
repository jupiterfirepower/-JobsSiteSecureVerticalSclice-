using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Jobs.Core.Middleware;

public class AdminSafeListMiddleware(
    RequestDelegate next,
    ILogger<AdminSafeListMiddleware> logger,
    string adminWhiteList)
{
    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Method != "GET")
        {
            var remoteIp = context.Connection.RemoteIpAddress;
            logger.LogDebug($"Request from Remote IP address: {remoteIp}");

            string[] ip = adminWhiteList.Split(';');

            var bytes = remoteIp.GetAddressBytes();
            var badIp = true;
            foreach (var address in ip)
            {
                var testIp = IPAddress.Parse(address);
                if (testIp.GetAddressBytes().SequenceEqual(bytes))
                {
                    badIp = false;
                    break;
                }
            }

            if (badIp)
            {
                logger.LogInformation($"Forbidden Request from Remote IP address: {remoteIp}");
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }
        }

        await next.Invoke(context);

    }
}