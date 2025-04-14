using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.Builder;
using AutoMapper;
using DotNetEnv;
using Jobs.Common.Constants;
using Jobs.Common.Contracts;
using Jobs.Common.Extentions;
using Jobs.Common.Helpers;
using Jobs.Common.Options;
using Jobs.Common.Settings;
using Jobs.Core.Contracts;
using Jobs.Core.Contracts.Providers;
using Jobs.Core.DataModel;
using Jobs.Core.Extentions;
using Jobs.Core.Filters;
using Jobs.Core.Handlers;
using Jobs.Core.Managers;
using Jobs.Core.Middleware;
using Jobs.Core.Observability.Options;
using Jobs.Core.Options;
using Jobs.Core.Providers;
using Jobs.Core.Providers.Vault;
using Jobs.Core.Services;
using Jobs.DTO;
using Jobs.DTO.In;
using Jobs.Entities.Models;
using Jobs.ReferenceApi.Contracts;
using Jobs.ReferenceApi.Data;
using Jobs.ReferenceApi.Extentions;
using Jobs.ReferenceApi.Features.Categories;
using Jobs.ReferenceApi.Features.EmploymentTypes;
//using Jobs.ReferenceApi.Features.Queries;
using Jobs.ReferenceApi.Features.WorkTypes;
using Jobs.ReferenceApi.Repositories;
using Jobs.ReferenceApi.Services;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using Serilog;
using Serilog.Context;
using Steeltoe.Discovery.Client;
using Steeltoe.Discovery.Consul;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    Console.WriteLine($"ContentRootPath: {builder.Environment.ContentRootPath}");
    Console.WriteLine($"EnvironmentName: {builder.Environment.EnvironmentName}");
    
    builder.Configuration
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();
    
    builder.Services.Configure<VaultOptions>(builder.Configuration.GetSection(VaultOptions.SectionName));
    
    builder.Host.UseSerilog((context, loggerConfiguration) =>
    {
        loggerConfiguration.WriteTo.Console();
        loggerConfiguration.ReadFrom.Configuration(context.Configuration);
    });
    
    Log.Information("Starting WebApi Reference Service.");

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddApiVersionService();

    builder.Services.AddDbContext<JobsDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    
    // user-secrets
    /*var referenceSecretKey = builder.Configuration["ReferenceApiService:SecretKey"];
    Console.WriteLine($"referenceSecretKey: {referenceSecretKey}");
    var referenceServiceDefApiKey = builder.Configuration["ReferenceApiService:DefaultApiKey"];
    Console.WriteLine($"referenceServiceDefApiKey: {referenceServiceDefApiKey}");*/
    
    Env.Load();
    //Env.TraversePath().Load();
    
    var vaultUri = Environment.GetEnvironmentVariable("VAULT_ADDR");
    var vaultToken = Environment.GetEnvironmentVariable("VAULT_TOKEN");
    
    // Hashicorp Vault Secrets.
    var vaultSecretsProvider = new VaultSecretProvider(vaultUri, vaultToken);

    var vaultSecretKey = await vaultSecretsProvider.GetSecretValueAsync("secrets/services/reference", "SecretKey", "secrets");
    Console.WriteLine($"vaultSecretKey: {vaultSecretKey}");
    var vaultDefaultApiKey = await vaultSecretsProvider.GetSecretValueAsync("secrets/services/reference", "DefaultApiKey", "secrets");
    Console.WriteLine($"vaultDefaultApiKey: {vaultDefaultApiKey}");
    
    builder.Services.AddMemoryCache();
    
    // Configure Dependency Injection
    builder.Services.ConfigureDependencyInjection(builder.Configuration);

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.WriteIndented = true;
        options.SerializerOptions.IncludeFields = true;
    });

    //builder.Services.AddRateLimiterService();
    builder.Services.AddWindowRateLimiterService();
    
    ObservabilityOptions observabilityOptions = new();

    builder.Configuration
        .GetRequiredSection(nameof(ObservabilityOptions))
        .Bind(observabilityOptions);
    
    builder.AddSerilog(observabilityOptions);
    builder.Services
        .AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService(observabilityOptions.ServiceName))
        .AddMetrics(observabilityOptions)
        .AddTracing(observabilityOptions);
    /*
    builder.Services.AddOpenTelemetry().WithMetrics(opts => opts
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
    );
    
    builder.Services.AddOpenTelemetry().WithMetrics(opts => opts
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("JobStore.WebApi"))
        .AddMeter(builder.Configuration.GetValue<string>("JobsStoreMeterName"))
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddOtlpExporter(opts =>
        {
            opts.Endpoint = new Uri(builder.Configuration["Otlp:Endpoint"]);
        })); */
    
    // forward headers configuration for reverse proxy
    builder.Services.Configure<ForwardedHeadersOptions>(options => {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    builder.Services.AddResponseCompressionService();
    
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();
    
    /*builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", settings =>
            settings.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
    });*/
    
    CorsSettings corsSettings = new();

    builder.Configuration
        .GetRequiredSection(nameof(CorsSettings))
        .Bind(corsSettings);
    
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(build => {
            build.WithOrigins(corsSettings.CorsAllowedOrigins);
            build.AllowAnyMethod();
            build.AllowAnyHeader();
        });
    });
    
    builder.Services.AddEndpoints(typeof(Program).Assembly);
    
    // Configuring Health Check
    builder.Services.ConfigureHealthChecks(builder.Configuration);
    
    // Service Discovery Consul
    builder.Services.AddServiceDiscovery(o => o.UseConsul());
    
    if (!builder.Environment.IsDevelopment())
    {
        builder.Services.AddHttpsRedirection(options =>
        {
            options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
            options.HttpsPort = 443;
        });
    }

    var app = builder.Build();
    
    app.UseResponseCompression();
    app.UseForwardedHeaders();
    app.UseMiddleware<AdminSafeListMiddleware>(builder.Configuration["HostsSafeList"]);
    
    // Global Exception Handler.
    app.UseExceptionHandler();
    
   
    var version1 = new ApiVersion(1);

    var apiVersionSet = app.NewApiVersionSet()
        .HasApiVersion(version1)
        //.HasApiVersion(new ApiVersion(2))
        .ReportApiVersions()
        .Build();

    RouteGroupBuilder versionedGroup = app
        .MapGroup("api/v{version:apiVersion}")
        .WithApiVersionSet(apiVersionSet);

    app.MapEndpoints(versionedGroup);
    
// Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Get the Automapper, we can share this too
    var mapper = app.Services.GetService<IMapper>();
    if (mapper == null)
    {
        throw new InvalidOperationException("Mapper not found");
    }

//app.UseLogHeaders(); // add here right after you create app


   // app.UseExceptionHandlers();
   
   
//app.UseSecurityHeaders();

//app.UseErrorHandler(); // add here right after you create app

    app.UseHttpsRedirection();
    
    app.UseStaticFiles(); // 🔴 here it is
    app.UseRouting(); // 🔴 here it is

    //app.UseCors("AllowAll");
    app.UseCors();
    
    /*app.UseCors(options =>
        options
            .WithOrigins("https://localhost:7111","http://localhost:5206")
            .AllowAnyMethod()
            .AllowAnyHeader());*/
    
    //app.UseMiddleware<ErrorHandlerMiddleware>(); 
    // Enable compression
    

    app.UseRateLimiter();
    
    // HealthCheck Middleware
    app.AddHealthChecks();
    
    // CorrelationId Middleware
    app.Use(async (context, next) =>
    {
        var correlationId = context.Request.Headers[HttpHeaderKeys.XCorrelationIdHeaderKey].FirstOrDefault() ?? Guid.NewGuid().ToString();
        context.Response.Headers[HttpHeaderKeys.XCorrelationIdHeaderKey] = correlationId;

        using (LogContext.PushProperty(HttpHeaderKeys.SerilogCorrelationIdProperty, correlationId))
        {
            await next();
        }
    }); 

    // Ensure database is created during application startup
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<JobsDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    // Request Logging Middleware
    app.Use(async (context, next) =>
    {
        try
        {
            var key = context.Request.Headers[HttpHeaderKeys.XApiHeaderKey];
            var nonce = context.Request.Headers[HttpHeaderKeys.SNonceHeaderKey];
            var secret = context.Request.Headers[HttpHeaderKeys.XApiSecretHeaderKey];
            Log.Information($"Incoming Request: {context.Request.Protocol} {context.Request.Method} {context.Request.Path}{context.Request.QueryString}");
            Log.Information($"Key - {key}, Nonce - {nonce}, Secret - {secret}");
        }
        catch (Exception)
        {
            Log.Information($"Api Key or Nonce not found in Header.");
        }
        
        await next();
    });

    app.Use(async (context, next) =>
    {
        context.Response.OnStarting(async () =>
        {
            using var scope = app.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IApiKeyService>();
            var cryptService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();

            if (context.Response.StatusCode == StatusCodes.Status200OK || 
                context.Response.StatusCode == StatusCodes.Status201Created ||
                context.Response.StatusCode == StatusCodes.Status204NoContent
               )
            {
                var apiKey = await service.GenerateApiKeyAsync();
                var cryptApiKey = cryptService.Encrypt(apiKey.Key);
                context.Response.Headers.Append(HttpHeaderKeys.XApiHeaderKey, cryptApiKey);
            }
        });
        await next.Invoke();
    });

    app.UseSerilogRequestLogging();
    
    //app.Urls.Add("https://localhost:7006"); // 👈 Add the URL

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "server terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
