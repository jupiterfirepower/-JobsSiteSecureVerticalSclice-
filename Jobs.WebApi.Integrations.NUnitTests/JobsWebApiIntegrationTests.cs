using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Jobs.Common.Contracts;
using Jobs.DTO;
using Jobs.DTO.In;
using Jobs.Entities.Models;
using Jobs.VacancyApi.Data;
using Jobs.VacancyApi.Repository;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace JobsWebApiIntegrationsTests;

public class JobsWebApiIntegrationsTests
{
    private ServiceProvider _serviceProvider;
    private const int CompanyId = 1;
    private const string TestApiKey = "123456789123456789123";
    
    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();

        // Using In-Memory database for testing
        services.AddDbContext<JobsDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));
        
        services.AddScoped<IGenericRepository<Vacancy>, VacancyRepository>();

        _serviceProvider = services.BuildServiceProvider();
    }
    
    [TearDown]
    public void Dispose()
    {
        _serviceProvider.Dispose();
    }
    
    [Test]
    public async Task AddVacancyTest()
    {
        await using var application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => { });
        using var client = application.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", TestApiKey);
        client.DefaultRequestHeaders.Add("nonce", DateTime.UtcNow.Ticks.ToString());
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        
        var vacancy = new VacancyInDto(0, CompanyId,  2,
            "Test Vacancy","Test Description",
            new List<int>(),new List<int>(),
             10000,100000, true, true);

        var json = System.Text.Json.JsonSerializer.Serialize(vacancy);
        var data = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/v1/vacancies", data);
        response.EnsureSuccessStatusCode();

        string result = response.Content.ReadAsStringAsync().Result;
        
        var settings = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            IncludeFields = true
        };
        
        var vacancyAdded = System.Text.Json.JsonSerializer.Deserialize<VacancyDto>(result,settings);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        ClassicAssert.IsNotNull(vacancyAdded);
        ClassicAssert.IsTrue(vacancyAdded?.VacancyId > 0);
    }
    
    [Test]
    public async Task UpdateVacancyTest()
    {
        await using var application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => { });
        using var client = application.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", TestApiKey);
        client.DefaultRequestHeaders.Add("nonce", DateTime.UtcNow.Ticks.ToString());
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        
        var vacancy = new VacancyInDto(0, CompanyId, 2,
            "Test Vacancy","Test Description",
            new List<int>(),new List<int>(),
            10000,100000, true, true);

        var json = JsonSerializer.Serialize(vacancy);
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/v1/vacancies", data);
        response.EnsureSuccessStatusCode();

        string result = response.Content.ReadAsStringAsync().Result;
        
        var settings = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            IncludeFields = true
        };
        
        var vacancyAdded = JsonSerializer.Deserialize<VacancyDto>(result, settings);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        ClassicAssert.IsNotNull(vacancyAdded);
        ClassicAssert.IsTrue(vacancyAdded?.VacancyId > 0);

        var vacancyName = "Updated Vacancy Name";

        var vacUpdated = new VacancyInDto(vacancyAdded.VacancyId, CompanyId, vacancyAdded.CategoryId, vacancyName,
            vacancyAdded.VacancyDescription, 
            new List<int>(),new List<int>(),
            vacancyAdded.SalaryFrom, vacancyAdded.SalaryTo, vacancyAdded.IsVisible, vacancyAdded.IsActive);
        
        var jsonUpdated = JsonSerializer.Serialize(vacUpdated);
        var dataUpdated = new StringContent(jsonUpdated, Encoding.UTF8, "application/json");
        
        var responseUpdated = await client.PutAsync($"/api/v1/vacancies/{vacUpdated.VacancyId}", dataUpdated);
        responseUpdated.EnsureSuccessStatusCode();
        responseUpdated.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var responseGetVacancy = await client.GetAsync($"/api/v1/vacancies/{vacUpdated.VacancyId}");
        responseGetVacancy.StatusCode.Should().Be(HttpStatusCode.OK);
        
        string resultUpd = responseGetVacancy.Content.ReadAsStringAsync().Result;
        var vacancyUpd = JsonSerializer.Deserialize<VacancyDto>(resultUpd, settings);

        ClassicAssert.IsNotNull(vacancyUpd);
        ClassicAssert.IsTrue(vacancyUpd?.VacancyId > 0);
        ClassicAssert.IsTrue(vacancyUpd?.VacancyTitle == vacancyName);
    }
    
    [Test]
    public async Task GetVacanciesTest()
    {
        await using var application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => { });
        using var client = application.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", TestApiKey);
        client.DefaultRequestHeaders.Add("nonce", DateTime.UtcNow.Ticks.ToString());
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        var response = await client.GetAsync("/api/v1/vacancies");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Test]
    public async Task GetVacancyByIdTest()
    {
        await using var application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => { });
        using var client = application.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", TestApiKey);
        client.DefaultRequestHeaders.Add("nonce", DateTime.UtcNow.Ticks.ToString());
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        
        var vacancy = new VacancyInDto(0, CompanyId,  2,
            "Test Vacancy 1","Test Description 1",
            new List<int>(),new List<int>(),
            10000,100000,true, true);

        var json = System.Text.Json.JsonSerializer.Serialize(vacancy);
        var data = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");

        var responsePost = await client.PostAsync("/api/v1/vacancies", data);
        responsePost.EnsureSuccessStatusCode();
        responsePost.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var settings = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            IncludeFields = true
        };

        string result = responsePost.Content.ReadAsStringAsync().Result;
        var vacancyAdded = System.Text.Json.JsonSerializer.Deserialize<Vacancy>(result, settings);
        ClassicAssert.IsNotNull(vacancyAdded);
        ClassicAssert.IsTrue(vacancyAdded?.VacancyId > 0);

        var response = await client.GetAsync($"/api/v1/vacancies/{vacancyAdded?.VacancyId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseDelete = await client.DeleteAsync($"/api/v1/vacancies/{vacancyAdded?.VacancyId}");
        responseDelete.EnsureSuccessStatusCode();
        responseDelete.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [Test]
    public async Task DeleteTest()
    {
        await using var application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => { });
        using var client = application.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", TestApiKey);
        client.DefaultRequestHeaders.Add("nonce", DateTime.UtcNow.Ticks.ToString());
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        
        var vacancy = new VacancyInDto(0, CompanyId,   2,
            "Test Vacancy Delete","Test Description Delete",
            new List<int>(),new List<int>(),
            10000,100000, true, true);

        var json = JsonSerializer.Serialize(vacancy);
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        var responsePost = await client.PostAsync("/api/v1/vacancies", data);
        responsePost.EnsureSuccessStatusCode();
        responsePost.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var settings = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            IncludeFields = true
        };

        string result = responsePost.Content.ReadAsStringAsync().Result;
        var vacancyAdded = JsonSerializer.Deserialize<Vacancy>(result, settings);
        ClassicAssert.IsNotNull(vacancyAdded);
        ClassicAssert.IsTrue(vacancyAdded?.VacancyId > 0);

        var response = await client.GetAsync($"/api/v1/vacancies/{vacancyAdded?.VacancyId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseDelete = await client.DeleteAsync($"/api/v1/vacancies/{vacancyAdded?.VacancyId}");
        responseDelete.EnsureSuccessStatusCode();
        responseDelete.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}

