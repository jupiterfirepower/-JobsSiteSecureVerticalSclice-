//using BlazorApp2.Client.Pages;

using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using BlazorApp2.Components;
using BlazorApp2.Contracts;
using BlazorApp2.Contracts.Clients;
using BlazorApp2.Responses;
using BlazorApp2.Services;
using BlazorApp2.Services.Clients;
using BlazorApp2.Settings;
using Jobs.Common.Extentions;
using Jobs.Common.Options;
using Jobs.Common.SerializationSettings;
using Jobs.Core.Contracts;
using Jobs.Core.Contracts.Providers;
using Jobs.Core.DataModel;
using Jobs.Core.Extentions;
using Jobs.Core.Helpers;
using Jobs.Core.Managers;
using Jobs.Core.Providers;
using Jobs.Core.Services;
using Jobs.Core.Settings;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using QAToolKit.Auth.Keycloak;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var companySecretKey = builder.Configuration["Company:SecretKey"];
var companyServiceApiUrl = builder.Configuration["ServicesSettings:CompanyApiServiceUrl"];
var serviceApiKey = builder.Configuration["ServicesSettings:ServiceApiKey"];
SecureApiKey.AccountFirstApiKey = builder.Configuration["AccountApiService:SecretApiKey"];
SecureApiKey.AccountSecretKey = builder.Configuration["AccountApiService:SecretKey"];

Console.WriteLine($"SecureApiKey.AccountFirstApiKey - {SecureApiKey.AccountFirstApiKey}");
Console.WriteLine($"SecureApiKey.AccountSecretKey - {SecureApiKey.AccountSecretKey}");

// Add this line to your program.cs
builder.Services.Configure<ServicesSettings>(builder.Configuration.GetSection("ServicesSettings"));
builder.Services.AddOptions();

//var settings = new RetrySettings { RetryCount = 3 };

CryptOptions cryptOptions = new();

builder.Configuration
    .GetRequiredSection(nameof(CryptOptions))
    .Bind(cryptOptions);

builder.Services.AddSingleton<ITypedClientConfig, TypedClientConfig>();

builder.Services.AddHttpClient<IAccountClientService, AccountClientService>()
    .AddHttpClientConfigurations();
    //.AddRetryPolicy(settings);

builder.Services.AddHttpClient<ICompanyClientService, CompanyClientService>()
    .AddHttpClientConfigurations();

builder.Services.AddHttpClient<IVacancyClientService, VacancyClientService>()
    .AddHttpClientConfigurations();

builder.Services.AddScoped<IPasswordStorageProvider, MemoryPasswordStorageProvider>();
builder.Services.AddScoped<IPasswordManagerServiceProvider, PasswordManagerServiceProvider>();



builder.Services.AddScoped<IAccountService, AccountService>();
//builder.Services.AddScoped<ICompanyService>(x => new CompanyService(companyServiceApiUrl,  serviceApiKey, companySecretKey));
builder.Services.AddScoped<ICompanyService, CompanyService>();
//builder.Services.AddScoped<IVacancyService>(x => new VacancyService(companySecretKey));
builder.Services.AddScoped<IVacancyService, VacancyService>();
builder.Services.AddScoped<IEncryptionService, NaiveEncryptionService>(p => 
    p.ResolveWith<NaiveEncryptionService>(Convert.FromBase64String(cryptOptions.PKey), Convert.FromBase64String(cryptOptions.IV)));

builder.Services.AddHttpContextAccessor();



//builder.Services.AddCascadingValue(someValue);

/*builder.Services.AddAuthentication().AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Google:ClientId"]!;
    googleOptions.ClientSecret = builder.Configuration["Google:ClientSecret"]!;
});*/

/*builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/account/login";
    })
    .AddOpenIdConnect(options =>
    {
        options.RequireHttpsMetadata = false;
        options.Authority = "http://localhost:9001/realms/mjobs/auth";
        options.ClientId = "confmjobs";
        options.ClientSecret = "SRrvfrHZ5dLcBJG8Qq8Ph8lIYrwtKqzj";
        options.ResponseType = "code";
        options.SaveTokens = true;
        options.Scope.Add("openid");
        options.CallbackPath = "/login-callback"; // Update callback path
        options.SignedOutCallbackPath = "/logout-callback"; // Update signout callback path
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "preferred_username",
            RoleClaimType = "roles"
        };
    });*/

builder.Services
    .AddAuthentication()
    .AddBearerToken();  //👈
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseOwin();
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.UseOwin(pipeline =>
{
    pipeline(next =>
    {
        return async environment =>
        {
            // Do something before.
            await next(environment);
            // Do something after.
        };
    });
});

var auth = new KeycloakAuthenticator(options =>
{
    options.AddClientCredentialFlowParameters(
        new Uri("http://localhost:9001/realms/mjobs/protocol/openid-connect/token"), 
        "confmjobs",
        "SRrvfrHZ5dLcBJG8Qq8Ph8lIYrwtKqzj"); 
});

//Get client credentials flow access token
var token = await auth.GetAccessToken();
Console.WriteLine($"{token}");
//Replace client credentials flow token for user access token
//var userToken = await auth.ExchangeForUserToken("jupiterfiretetraedr@gmail.com");

//Console.WriteLine($"{userToken}");
//eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJhS3VDVVBQV1RQd19vZDdpQUpzcDRPRkoxLXM2d0I5RVgzUDAyODhXRS1FIn0.eyJleHAiOjE2MjMzMDY0NzUsImlhdCI6MTYyMzMwNDY3NSwiYX...
//[FromServices] IHttpContextAccessor context,
app.MapGet("/login", (string username) =>
{
    var claimsPrincipal = new ClaimsPrincipal(
        new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, username)},
            BearerTokenDefaults.AuthenticationScheme  //👈
        )
    );
    //var r = OwinRequest;
    //var environment = new OwinEnvironment(context);
    //IOwinContext owinContext = OwinContext.GetOwinContext();
    //OwinRequestScopeContext.Current
    //Console.WriteLine("context.HttpContext != null" +context.HttpContext != null);
    //context.HttpContext?.SignInAsync(claimsPrincipal);
    return Results.SignIn(claimsPrincipal);
    //, authenticationScheme: CookieAuthenticationDefaults.AuthenticationScheme
});

app.MapGet("/user", (ClaimsPrincipal user) =>
{
    return Results.Ok($"Welcome {user.Identity?.Name}!");
}).RequireAuthorization();
    
// Add routes for callback handling
app.Map("/login-callback", loginCallbackApp =>
{
    loginCallbackApp.Run(async context =>
    {
        // Handle the callback from Keycloak after successful authentication
        await context.Response.WriteAsync("Authentication successful");
    });
});

app.Map("/logout-callback", logoutCallbackApp =>
{
    logoutCallbackApp.Run(async context =>
    {
        // Handle the callback from Keycloak after sign-out
        await context.Response.WriteAsync("Sign-out successful");
    });
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorApp2.Client._Imports).Assembly);

//http://localhost:5047/oauth?code=4%2F0AQlEd8w0yVB6824q1hNjsXzSClXUT4AA_DviFE4XjAwupUgPXOZ1DnUd6U9gDnAmUNPi6g&scope=email+openid+https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fuserinfo.email&authuser=0&prompt=consent
// Add routes for callback handling
app.MapGet("/oauth", async (string code, string scope, 
    [FromServices] IAccountService service,
    [FromServices] IEncryptionService cryptService,
    [FromServices] IPasswordManagerServiceProvider managerServiceProvider) =>
{
    Console.WriteLine("oauth");
    Console.WriteLine($"Code : {code}, Scope: {scope}");
    var googleAuth = new GoogleAuthService();
    var googleToken = await googleAuth.GetAccessToken(code);
    Console.WriteLine($"AccessToken : {googleToken.AccessToken}");
    var userInfo = await googleAuth.GetUserInfo(googleToken.AccessToken);
    Console.WriteLine($"UserInfo : {userInfo}");
    
    var userInfoData = JsonSerializer.Deserialize<GoogleUserInfo>(userInfo, JsonSerializerSetting.JsonSerializerOptions )!;
    Console.WriteLine($"GoogleUserInfo : {userInfoData != null}");
    var gen = new PasswordSaltGeneratorHelper();
    //var pwd = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(gen.GenerateKeycloakGooglePassword()));
    var pwd = gen.GenerateKeycloakGooglePassword();
    //var pwdc = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(pwd));
    Console.WriteLine($"Email : {userInfoData.Email}, Pwd : {pwd}");
    
    managerServiceProvider.AddUserCredential(new ExternalUserCredential { Email = userInfoData.Email, Password = pwd });
    //const string defPwd = "dfvgbh12347890";
    var result = await service.RegisterAsync(userInfoData.Email, pwd, userInfoData.GivenName, userInfoData.FamilyName);
    bool logged = false;

    var currentPassword = !result.IsAdded
        ? managerServiceProvider.GetUserCredential(userInfoData.Email).Password
        : pwd;
    
    //var currentPassword = pwd;
    
    Console.WriteLine($"Current Password : {currentPassword}");
    
    logged = await service.LoginAsync(userInfoData.Email,  currentPassword);
    Console.WriteLine($"Logged : {logged}");

    return Results.Redirect("/");
});

app.Run();
