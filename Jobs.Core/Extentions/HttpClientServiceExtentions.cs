using System.Net;
using System.Security.Authentication;
using Jobs.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Net.Http.Headers;
using Polly;

namespace Jobs.Core.Extentions;

public static class HttpClientServiceExtention
{
    public static IHttpStandardResiliencePipelineBuilder AddHttpClientConfigurations(this IHttpClientBuilder builder)
    {
        return builder.AddHttpClientConfiguration()
            .AddPollyConfiguration();
    }

    private static IHttpClientBuilder AddHttpClientConfiguration(this IHttpClientBuilder builder)
    {
        return builder.ConfigureHttpClient((serviceProvider, httpClient) =>
            {
                var clientConfig = serviceProvider.GetRequiredService<ITypedClientConfig>();
                httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);

                httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, clientConfig.UserAgent);
                httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
                httpClient.DefaultRequestVersion = HttpVersion.Version30; // QUIC HTTP3
                httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
            })
            .UseSocketsHttpHandler((handler, _) =>
            {
                handler.PooledConnectionLifetime =
                    TimeSpan.FromSeconds(
                        60); // TimeSpan.FromSeconds(0); //Analog ConnectionLeaseTimeout 0 - close after request handle.
                handler.PooledConnectionIdleTimeout = TimeSpan.FromSeconds(30); //Analog MaxIdleTime
                //handler.MaxConnectionsPerServer = 5;
                handler.ConnectTimeout = TimeSpan.FromSeconds(2);
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5)) // Default is 2 mins
            .ConfigurePrimaryHttpMessageHandler(x =>
                new HttpClientHandler
                {
                    UseCookies = false,
                    AllowAutoRedirect = false,
                    UseDefaultCredentials = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate |
                                             DecompressionMethods.Brotli,
                    //SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
                    SslProtocols = SslProtocols.Tls13,
                    //ServerCertificateCustomValidationCallback =   
                    //HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });
    }

    private static IHttpStandardResiliencePipelineBuilder AddPollyConfiguration(this IHttpClientBuilder builder)
    {
        return builder
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.Delay = TimeSpan.FromMilliseconds(5);
                options.Retry.OnRetry = args =>
                {
                    Console.WriteLine($"Retry attempt : {args.AttemptNumber}");
                    return default;
                };
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
            })
            .Configure((options, _) => // (options, serviceProvider) =>
            {
                options.Retry.ShouldHandle = args =>
                {
                    if (args.Outcome.Result?.StatusCode == HttpStatusCode.NotFound)
                    {
                        //return PredicateResult.True();
                        return PredicateResult.False();
                    }

                    return args.Outcome.Exception switch
                    {
                        OperationCanceledException => PredicateResult.False(),
                        not null => PredicateResult.True(),
                        _ => PredicateResult.False()
                    };
                };
            });
    }
}