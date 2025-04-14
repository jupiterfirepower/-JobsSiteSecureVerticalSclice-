using System.Net;
using Jobs.Common.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace Jobs.Common.Extentions;

public static class HttpClientExtentions
{
    public static IHttpClientBuilder AddRetryPolicy(
        this IHttpClientBuilder clientBuilder,
        IRetrySettings settings)
    {
        Func<IEnumerable<TimeSpan>> delay = () => Backoff.DecorrelatedJitterBackoffV2(
            medianFirstRetryDelay: TimeSpan.FromMilliseconds(10),
            retryCount: settings.RetryCount,
            fastFirst: true);
        
        return clientBuilder
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                //.Or<TimeoutRejectedException>()
                .OrResult(r => r.StatusCode == (HttpStatusCode) 404) // 429 Too Many Requests
                .WaitAndRetryAsync(
                    retryCount: settings.RetryCount,
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(5 * Math.Pow(2, attempt)),
                    onRetry: (message, timeSpan, context) =>
                    {
                        Console.WriteLine($"Retry attempt : {message.Exception.Message} - {timeSpan.Milliseconds} - {context.CorrelationId}");
                    }))
            .AddPolicyHandler(Policy
                .Handle<Exception>()
                .OrTransientHttpError()
                .CircuitBreakerAsync(
                    5,                       // how much subsequant failures should open circuit
                    TimeSpan.FromSeconds(30) // how long circuit should be opened before trying again
                ))
            .AddPolicyHandler(Policy
                .TimeoutAsync<HttpResponseMessage>(
                    TimeSpan.FromSeconds(1) // timeout for each request
                ));
    }
    
    public static IHttpResiliencePipelineBuilder AddRetryPolicy(
        this IHttpClientBuilder clientBuilder)
    {
        return clientBuilder
            .AddResilienceHandler("MyResilienceStrategy", resilienceBuilder => // Adds resilience policy named "MyResilienceStrategy"
        {
            // Retry Strategy configuration
            resilienceBuilder.AddRetry(new HttpRetryStrategyOptions // Configures retry behavior
            {
                MaxRetryAttempts = 4, // Maximum retries before throwing an exception (default: 3)

                Delay = TimeSpan.FromSeconds(2), // Delay between retries (default: varies by strategy)

                BackoffType = DelayBackoffType.Exponential, // Exponential backoff for increasing delays (default)

                UseJitter = true, // Adds random jitter to delay for better distribution (default: false)

                ShouldHandle = new PredicateBuilder<HttpResponseMessage>() // Defines exceptions to trigger retries
                .Handle<HttpRequestException>() // Includes any HttpRequestException
                .HandleResult(response => !response.IsSuccessStatusCode) // Includes non-successful responses
            });

            // Timeout Strategy configuration
            resilienceBuilder.AddTimeout(TimeSpan.FromSeconds(5)); // Sets a timeout limit for requests (throws TimeoutRejectedException)

            // Circuit Breaker Strategy configuration
            resilienceBuilder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions // Configures circuit breaker behavior
            {
                // Tracks failures within this time frame
                SamplingDuration = TimeSpan.FromSeconds(10),

                // Trips the circuit if failure ratio exceeds this within sampling duration (20% failures allowed)
                FailureRatio = 0.2,

                // Requires at least this many successful requests within sampling duration to reset
                MinimumThroughput = 3,

                // How long the circuit stays open after tripping
                BreakDuration = TimeSpan.FromSeconds(1),

                // Defines exceptions to trip the circuit breaker
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>() // Includes any HttpRequestException
                .HandleResult(response => !response.IsSuccessStatusCode) // Includes non-successful responses
            });
        });
    }
}