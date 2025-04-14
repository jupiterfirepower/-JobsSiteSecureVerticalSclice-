using System.Diagnostics;
using System.Net;
using System.Net.Mime;
using Jobs.Common.Constants;
using Jobs.YarpGateway.Contracts;
using Jobs.YarpGateway.Helpers;
using Jobs.YarpGateway.Services;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Net.Http.Headers;
using Steeltoe.Common.Http.Discovery;
using Steeltoe.Discovery.Client;
using Steeltoe.Discovery.Consul;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Service Discovery Consul
builder.Services.AddServiceDiscovery(o => o.UseConsul());

builder.Services.AddSingleton<ISessionService, SessionService>();
    
    /*.AddHttpClient().ConfigureHttpClientDefaults(static http =>
    {
        // Configure the HttpClient to use service discovery
        http.AddServiceDiscovery();
    });*/

// Grab a new client from the service provider
//var client = builder.Services.Get .GetService<HttpClient>()!;

/*
builder.Services.ConfigureHttpClientDefaults(clientBuilder => clientBuilder.AddServiceDiscovery());

builder.Services.ConfigureHttpClientDefaults(static client =>
{
    client.AddServiceDiscovery();
});*/


//builder.Services.AddConsulClient(builder.Configuration.GetSection("ConsulServiceDiscovery:ConsulClient"));

// Add YARP reverse proxy services and load configuration from appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    //.AddServiceDiscoveryDestinationResolver()
    .ConfigureHttpClient((_, handler) =>
    {
        handler.AllowAutoRedirect = true;
        handler.MaxConnectionsPerServer = 100;
        handler.EnableMultipleHttp2Connections = true;
    })
    .AddTransforms(transforms =>
    {
        transforms.AddRequestTransform(async context =>
        {
            Console.WriteLine(context.HttpContext.Request.Path);
            
            var sessionService = context.HttpContext.RequestServices.GetService<ISessionService>();
            
            //var sessionService = new SessionService();
            
            //  CorrelationId
            var correlationId = context.HttpContext.Request.Headers[HttpHeaderKeys.XCorrelationIdHeaderKey].FirstOrDefault() 
                                ?? Guid.NewGuid().ToString();
            
            Console.WriteLine($"CorrelationId: {correlationId}");

            var appId = context.HttpContext.Request.Headers[HttpHeaderKeys.XAppIdHeaderKey].Single();
            
            sessionService.AddSession(appId!, correlationId);

            var helper = new ServicesHeadersHelper();
            var currentApiKey = sessionService.GetApiKeyByAppId(context.HttpContext.Request.Path.ToString().EndsWith("/token") ? String.Empty : appId!);
            var (apiKey, secretKey, nonce) = helper.GetHeadersValues(ServiceNames.VacancyService, currentApiKey);
            Console.WriteLine(context.HttpContext.Request.Path);
            Console.WriteLine($"AddRequestTransform ApiKey - {apiKey}");
            sessionService.Trace();
            
            context.HttpContext.Request.Headers.Clear();
            context.ProxyRequest.Headers.Clear();
            
            context.ProxyRequest.Headers.TryAddWithoutValidation(HeaderNames.UserAgent, UserAgentConstant.AppUserAgent);
            context.ProxyRequest.Headers.TryAddWithoutValidation(HttpHeaderKeys.XApiHeaderKey, apiKey);
            context.ProxyRequest.Headers.TryAddWithoutValidation(HttpHeaderKeys.SNonceHeaderKey, nonce);
            context.ProxyRequest.Headers.TryAddWithoutValidation(HttpHeaderKeys.XApiSecretHeaderKey, secretKey);
            context.ProxyRequest.Headers.TryAddWithoutValidation(HttpHeaderKeys.XCorrelationIdHeaderKey, correlationId);
            context.ProxyRequest.Headers.TryAddWithoutValidation(HeaderNames.Accept, MediaTypeNames.Application.Json);

            //context.HttpContext.Request.Headers.Append(HeaderNames.ContentType, MediaTypeNames.Application.Json);
           // Console.WriteLine(context.HttpContext.Request.Host);
            //context.HttpContext.
            // implicit parsing

            // Copying the headers from the incoming request to the target request
            /*foreach (var header in context.HttpContext.Request.Headers)
            {
                context.ProxyRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }*/

            await Task.CompletedTask;
        });
    }).AddTransforms(builderContext =>  // Monitor YARP’s performance:
    {
        builderContext.AddRequestTransform(async transformContext =>
        {
            var activity = new Activity("ProxyRequest");
            activity.SetTag("Destination", transformContext.DestinationPrefix);
            activity.Start();
            
            transformContext.HttpContext.Items["ProxyActivity"] = activity;
            await Task.CompletedTask;
        });
        
        builderContext.AddResponseTransform(async transformContext =>
        {
            if (transformContext.HttpContext.Items["ProxyActivity"] is Activity activity)
            {
                activity.SetTag("StatusCode", transformContext.ProxyResponse?.StatusCode);
                activity.Stop();
            }
            await Task.CompletedTask;
        });
    });

// Add YARP Direct Forwarding with Service Discovery support
//builder.Services.AddHttpForwarderWithServiceDiscovery();

// Optional: Add logging for diagnostics (if needed)
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddFilter("Yarp", LogLevel.Debug);
});

builder.Services.AddHttpLogging(logging =>
{
    // Customize HTTP logging here.
    logging.LoggingFields = HttpLoggingFields.All;
    //logging.RequestHeaders.Add("sec-ch-ua");
    //logging.ResponseHeaders.Add("my-response-header");
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});

/*
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("authenticated", policy =>
        policy.RequireAuthenticatedUser());
});*/

builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
    {
        options.Window = TimeSpan.FromSeconds(60);
        options.PermitLimit = 50;
    });
});

var app = builder.Build();

/*using var scope = app.Services.CreateScope();
//var service = scope.ServiceProvider.GetRequiredService<HttpClient>();
var client = scope.ServiceProvider.GetService<HttpClient>();

// Call an API called `foo` using service discovery
var response = await client.GetAsync("https://vacancy-service/api/v1/token");
var body = await response.Content.ReadAsStringAsync();

Console.WriteLine(body);*/

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpLogging();
app.UseHttpsRedirection();

/*
app.UseAuthentication();
app.UseAuthorization();*/
app.UseRateLimiter();

app.Use(async (context, next) =>
{
    //using (LogContext.PushProperty(HttpHeaderKeys.SerilogCorrelationIdProperty, correlationId))
    //{
    Console.WriteLine("context.GetEndpoint()");
    if (SecurityHeaderHelper.IsHeadersValid(context))
    {
       await next();
    }
    else
    {
        var response = context.Response;
        if (!response.HasStarted)
        {
            response.ContentType = "application/json";
            response.ContentLength = 0;
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            await response.WriteAsync(String.Empty);
        }
    }
    //}
});   

// Map the reverse proxy routes  Register YARP middleware
app.MapReverseProxy(proxyPipeline =>
{
    //proxyPipeline.
    proxyPipeline.Use(async (context, next) =>
    {
        // Logging logic before passing to the next middleware
        await next();
        // Logging logic after the response is received
        
        var apiKey = context.Response.Headers[HttpHeaderKeys.XApiHeaderKey].FirstOrDefault();
        Console.WriteLine("!ApiKey! - " + apiKey);
        
        var correlationId = context.Response.Headers[HttpHeaderKeys.XCorrelationIdHeaderKey].FirstOrDefault();
        Console.WriteLine("*CorrelationId* - " + correlationId);
        
        var sessionService = context.RequestServices.GetService<ISessionService>();
        
        //var sessionService = new SessionService();
        
        if (!string.IsNullOrEmpty(apiKey))
        {
            sessionService.UpdateSession(correlationId!, apiKey!);
        }
        
        sessionService.Trace();
    });
});

// Map a Direct Forwarder which forwards requests to the resolved "catalogservice" endpoints
//app.MapForwarder("/api/v1/token", "https://vacancy-service", "/api/v1/token");

app.Run();
