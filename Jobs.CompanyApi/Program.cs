using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Security.Claims;
using Asp.Versioning;
using AutoMapper;
using DotNetEnv;
using Jobs.Common.Constants;
using Jobs.Common.Contracts;
using Jobs.Common.Extentions;
using Jobs.Common.Options;
using Jobs.Common.Settings;
using Jobs.CompanyApi.DbContext;
using Jobs.CompanyApi.Extentions;
using Jobs.CompanyApi.Features.Companies;
using Jobs.CompanyApi.Features.Notifications;
using Jobs.CompanyApi.Helpers;
using Jobs.CompanyApi.Repositories;
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
using Keycloak.AuthServices.Authentication;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
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
    
    builder.Services.Configure<VaultOptions>(builder.Configuration.GetSection(VaultOptions.SectionName));

    builder.Host.UseSerilog((context, loggerConfiguration) =>
    {
        loggerConfiguration.WriteTo.Console();
        loggerConfiguration.ReadFrom.Configuration(context.Configuration);
    });

    Log.Information("Starting WebApi Company Service.");

    // Add services to the container.
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    //builder.Services.AddSwaggerGen();

    KeycloakSwaggerSettings swaggerSettings = new();

    builder.Configuration
        .GetRequiredSection(nameof(KeycloakSwaggerSettings))
        .Bind(swaggerSettings);

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
        options.AddSecurityDefinition("Keycloak", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                Implicit = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri(swaggerSettings.AuthServerUrl),
                    Scopes = new Dictionary<string, string>
                    {
                        { "openid", "openid" },
                        { "profile", "profile" }
                    }
                }
            }
        });

        OpenApiSecurityScheme keycloakSecurityScheme = new()
        {
            Reference = new OpenApiReference
            {
                Id = "Keycloak",
                Type = ReferenceType.SecurityScheme,
            },
            In = ParameterLocation.Header,
            Name = "Bearer",
            Scheme = "Bearer",
        };

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { keycloakSecurityScheme, Array.Empty<string>() },
        });
    });

    builder.Services.AddApiVersionService();

    builder.Services.AddDbContext<CompanyDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    
    // user-secrets
    /*var companySecretKey = builder.Configuration["CompanyApiService:SecretKey"];
    Console.WriteLine($"companySecretKey: {companySecretKey}");
    var companyServiceDefApiKey = builder.Configuration["CompanyApiService:DefaultApiKey"];
    Console.WriteLine($"companyServiceDefApiKey: {companyServiceDefApiKey}");*/
    
    Env.Load();
    //Env.TraversePath().Load();
    
    var vaultUri = Environment.GetEnvironmentVariable("VAULT_ADDR");
    var vaultToken = Environment.GetEnvironmentVariable("VAULT_TOKEN");
    
    // Hashicorp Vault Secrets.
    var vaultSecretsProvider = new VaultSecretProvider(vaultUri, vaultToken);

    var vaultSecretKey = await vaultSecretsProvider.GetSecretValueAsync("secrets/services/company", "SecretKey", "secrets");
    Console.WriteLine($"vaultSecretKey: {vaultSecretKey}");
    var vaultDefaultApiKey = await vaultSecretsProvider.GetSecretValueAsync("secrets/services/company", "DefaultApiKey", "secrets");
    Console.WriteLine($"vaultDefaultApiKey: {vaultDefaultApiKey}");

    // Configure Dependency Injection
    builder.Services.ConfigureDependencyInjection(builder.Configuration);

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.WriteIndented = true;
        options.SerializerOptions.IncludeFields = true;
    });

    //builder.Services.AddRateLimiterService();
    builder.Services.AddWindowRateLimiterService();

    builder.Services.AddHttpClient();

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

    // forward headers configuration for reverse proxy
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    builder.Services.AddResponseCompressionService();
    builder.Services.AddHttpContextAccessor();

    // Keycloak Auth.
    builder.Services.AddKeycloakWebApiAuthentication(builder.Configuration);
    builder.Services.AddAuthorization();

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    CorsSettings corsSettings = new();

    builder.Configuration
        .GetRequiredSection(nameof(CorsSettings))
        .Bind(corsSettings);

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(build =>
        {
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

    app.UseAuthentication();
    app.UseAuthorization();

    var version1 = new ApiVersion(1);

    var apiVersionSet = app.NewApiVersionSet()
        .HasApiVersion(version1)
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
        //app.UseSwaggerUI();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
            options.OAuthClientId("confmjobs");
        });
    }

    app.UseResponseCompression();
    app.UseForwardedHeaders();
    app.UseMiddleware<AdminSafeListMiddleware>(builder.Configuration["HostsSafeList"]);

// Global Exception Handler.
    app.UseExceptionHandler();

//app.UseHttpsRedirection();

// Get the Automapper, we can share this too
    var mapper = app.Services.GetService<IMapper>();
    if (mapper == null)
    {
        throw new InvalidOperationException("Mapper not found");
    }

    // app.UseLogHeaders(); // add here right after you create app
   // app.UseExceptionHandlers();

    //app.UseSecurityHeaders();

    //app.UseErrorHandler(); // add here right after you create app

    
    
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

    app.Use(async (context, next) =>
    {
        try
        {
            var key = context.Request.Headers[HttpHeaderKeys.XApiHeaderKey];
            var nonce = context.Request.Headers[HttpHeaderKeys.SNonceHeaderKey];
            var secret = context.Request.Headers[HttpHeaderKeys.XApiSecretHeaderKey];
            Log.Information(
                $"Incoming Request: {context.Request.Protocol} {context.Request.Method} {context.Request.Path}{context.Request.QueryString}");
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

            if (!context.Request.Path.Equals(HealthConstants.HealthApiPath) && 
                (context.Response.StatusCode == StatusCodes.Status200OK || 
                 context.Response.StatusCode == StatusCodes.Status201Created ||
                 context.Response.StatusCode == StatusCodes.Status204NoContent) 
               )
            {
                var apiKey = await service.GenerateApiKeyAsync();
                var cryptApiKey = cryptService.Encrypt(apiKey.Key);
                context.Response.Headers.Append(HttpHeaderKeys.XApiHeaderKey, cryptApiKey);
            }
        });

        await next.Invoke();
    });

    app.UseCors();
    app.UseRateLimiter();

    app.UseSerilogRequestLogging();
    
    //app.Urls.Add("https://localhost:7056"); // 👈 Add the URL

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

