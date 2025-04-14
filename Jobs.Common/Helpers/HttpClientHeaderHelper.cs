using System.Net.Http.Headers;
using System.Net.Mime;
using Jobs.Common.Constants;
using Microsoft.Net.Http.Headers;
using NanoidDotNet;
using NUlid;

namespace Jobs.Common.Helpers;

public static class HttpClientHeaderHelper
{
    public static HttpClient CreateHttpClientWithSecurityHeaders(string apiKey, string apiSecret, string token = null)
    {
        var httpClient = new HttpClient();
        SetHttpClientSecurityHeaders(httpClient, apiKey, apiSecret, token);
        return httpClient;
    }

    public static void SetHttpClientSecurityHeaders(HttpClient httpClient, string apiKey, string apiSecret, string token = null)
    {
        httpClient.DefaultRequestHeaders.Clear();
        
        httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "JobsSiteSpecialClientAgent");
        
        
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
        
        Console.WriteLine($"s-nonce : {nonceValue}");
        
        if (!string.IsNullOrWhiteSpace(token))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(HttpHeaderKeys.Bearer, token);
        }
        
        httpClient.DefaultRequestHeaders.Add(HttpHeaderKeys.XApiHeaderKey, apiKey);
        httpClient.DefaultRequestHeaders.Add(HttpHeaderKeys.SNonceHeaderKey, nonceValue);
        httpClient.DefaultRequestHeaders.Add(HttpHeaderKeys.XApiSecretHeaderKey, apiSecret);
        httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, MediaTypeNames.Application.Json);
    }
    
    public static void SetGatewayHttpClientSecurityHeaders(HttpClient httpClient, string token = null)
    {
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, MediaTypeNames.Application.Json);
        
        httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, Ulid.NewUlid().ToString());
        
        var ticks = DateTime.UtcNow.Ticks;
        var nonce = ticks.ToString();
        
        var nanoId = Nanoid.Generate(Nanoid.Alphabets.LowercaseLettersAndDigits);
        
        var nonceValue = $"{nonce}-{nanoId}";
        
        Console.WriteLine($"s-nonce : {nonceValue}");
        
        if (!string.IsNullOrWhiteSpace(token))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(HttpHeaderKeys.Bearer, token);
        }
        
        httpClient.DefaultRequestHeaders.Add(HttpHeaderKeys.XApiNonceHeaderKey, nonceValue);

        var appId = new GuidV7();
        var secretKey = GuidV8.NewGuidV8(1, 3, 28, 32);
        
        httpClient.DefaultRequestHeaders.Add(HttpHeaderKeys.XAppIdHeaderKey, appId.Value.ToString());
        httpClient.DefaultRequestHeaders.Add(HttpHeaderKeys.XApiSecretHeaderKey, secretKey.ToString());
    }
}