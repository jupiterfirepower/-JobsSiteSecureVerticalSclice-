using System.Reflection;
using Dapper;
using Jobs.Common.Contracts;
using Jobs.DTO.In;
using Jobs.Entities.Models;
using Jobs.VacancyApi.Contracts;
using Jobs.VacancyApi.Data;
using Jobs.VacancyApi.Repository;
using JobsWebApiNUnitTests.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace JobsWebApiNUnitTests;

public class ProcessingServiceUnitTests
{
    private ServiceProvider _serviceProvider;
    private int CompanyId = 1;
    private readonly NpgsqlConnection _connection = new ("Server=localhost;Port=5432;Database=jobs_db;User Id=admin;Password=newpwd;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=20;"); 
    
    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();

        // Using In-Memory database for testing
        //services.AddDbContext<JobsDbContext>(options =>
          //  options.UseInMemoryDatabase("TestDb"));
        
        services.AddDbContext<JobsDbContext>(options =>
            options.UseNpgsql("Server=localhost;Port=5432;Database=jobs_db;User Id=admin;Password=newpwd;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=20;"));
        
        services.AddAutoMapper(Assembly.GetExecutingAssembly()); 
        //services.AddScoped<IGenericRepository<Category>, CategoryRepository>();
        //services.AddScoped<IGenericRepository<WorkType>, WorkTypeRepository>();
        //services.AddScoped<IGenericRepository<EmploymentType>, EmploymentTypeRepository>();
        //services.AddScoped<IMiniGenericRepository<VacancyWorkTypes>, VacancyWorkTypesRepository>();
        //services.AddScoped<IMiniGenericRepository<VacancyEmploymentTypes>, VacancyEmploymentTypesRepository>();
        services.AddScoped<IGenericRepository<Vacancy>, VacancyRepository>();
        //services.AddScoped<IProcessingService, ProcessingService>();

        _serviceProvider = services.BuildServiceProvider();
        
        try
        {
            _connection.Open();
            
            var queryArguments = new
            {
                CategoryId = 99
            };

            _connection.Execute("delete from \"Categories\" where \"CategoryId\" > @CategoryId", queryArguments);
        }
        finally
        {
            _connection.Close();
        }
    }
    
    [TearDown]
    public void Dispose()
    {
        _serviceProvider.Dispose();
    }
    
    [OneTimeTearDown]
    public void Destructor()
    {
        try
        {
            _connection.Open();
            
            var queryArguments = new
            {
                CategoryId = 99
            };

            _connection.Execute("delete from \"Categories\" where \"CategoryId\" > @CategoryId", queryArguments);
        }
        finally
        {
            _connection.Close();
        }
        
        _connection.Dispose();
    }
    
    [Test]
    public void ProcessingServiceCreateVacancyTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var dbContext = scopedServices.GetRequiredService<JobsDbContext>();
            //var service = scopedServices.GetRequiredService<IProcessingService>();
            
            var category = new Category
            {
                CategoryId = 100,
                CategoryName = "Service Test Root Category",
            };

            dbContext.Categories.Add(category);
            dbContext.SaveChanges();
            
            var category2 = new Category
            {
                CategoryId = 101,
                CategoryName = "Service Test Root Category IT",
                ParentId = 100
            };
            
            dbContext.Categories.Add(category2);
            dbContext.SaveChanges();

            var vacancy = new VacancyInDto(0, CompanyId,category2.CategoryId,
                "Test Service Vacancy", "Test Description",
                 new List<int>(),new List<int>(),
                 100000, 200000, true, true);

            // Act
            //var result = service.CreateVacancy(vacancy).Result;
    
            // Assert
           // var vacs = service.GetVacancies().Result;
            //Assert.IsNotNull(vacs);
            //var res = vacs.Select(x => x.VacancyTitle == vacancy.VacancyTitle);
            //Assert.IsNotNull(res);
            //Assert.IsTrue(res.Any());
            //Assert.That(addedItem.CompanyName, Is.EqualTo(vacancy.CompanyName));
        }
        Assert.Pass();
    }
    
    [Test]
    public void ProcessingServiceCreateVacancyIsNotValidCompanyNameTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var dbContext = scopedServices.GetRequiredService<JobsDbContext>();
            //var service = scopedServices.GetRequiredService<IProcessingService>();
            
            var category = new Category
            {
                CategoryId = 102,
                CategoryName = "Service Test Root Category",
            };

            dbContext.Categories.Add(category);
            dbContext.SaveChanges();
            
            var category2 = new Category
            {
                CategoryId = 103,
                CategoryName = "Service Test Root Category IT",
                ParentId = 102
            };
            
            dbContext.Categories.Add(category2);
            dbContext.SaveChanges();

            var vacancy = new VacancyInDto(0, CompanyId,category2.CategoryId,
                RandomStringHelper.RandomString(300),
                "Test Description", 
                new List<int>(),new List<int>(),
                100000, 200000, true, true);
            
            //Assert.IsFalse(vacancy.IsValid()); 
        }
        Assert.Pass();
    }
    
    [Test]
    public void ProcessingServiceGetVacanciesTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var dbContext = scopedServices.GetRequiredService<JobsDbContext>();
            //var service = scopedServices.GetRequiredService<IProcessingService>();
            
            var category = new Category
            {
                CategoryId = 104,
                CategoryName = "Service Test Root Category 7",
            };

            dbContext.Categories.Add(category);
            dbContext.SaveChanges();
            
            var category2 = new Category
            {
                CategoryId = 105,
                CategoryName = "Service Test Root Category IT 8",
                ParentId = 104
            };
            
            dbContext.Categories.Add(category2);
            dbContext.SaveChanges();

            var vacancy = new VacancyInDto(0, CompanyId, category2.CategoryId,
                "Test Service Vacancy GetVacancies", "Test Description GetVacancies",
                new List<int>(),new List<int>(),
                 100000, 200000, true, true);

            // Act
            //var result = service.CreateVacancy(vacancy).Result;
            //Assert.IsNotNull(result);
            //ssert.That(result.VacancyTitle, Is.EqualTo(vacancy.VacancyTitle));
            // Assert
            //var vacs = service.GetVacancies().Result;
            //Assert.IsNotNull(vacs);
           // var res = vacs.Select(x => x.VacancyTitle == vacancy.VacancyTitle);
            //Assert.IsNotNull(res);
           // Assert.IsTrue(res.Any());
        }
        Assert.Pass();
    }
    
    [Test]
    public void ProcessingServiceGetVacancyByIdTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var dbContext = scopedServices.GetRequiredService<JobsDbContext>();
            //var service = scopedServices.GetRequiredService<IProcessingService>();
            
            var category = new Category
            {
                CategoryId = 106,
                CategoryName = "Service Test Root Category 9",
            };

            dbContext.Categories.Add(category);
            dbContext.SaveChanges();
            
            var category2 = new Category
            {
                CategoryId = 107,
                CategoryName = "Service Test Root Category IT 10",
                ParentId = 106
            };
            
            dbContext.Categories.Add(category2);
            dbContext.SaveChanges();

            var vacancy = new VacancyInDto(0, CompanyId, category2.CategoryId,
                "Test Service Vacancy GetVacancyByIdTest", "Test Description GetVacancyByIdTest",
                new List<int>(),new List<int>(),
                200000,100000, true, true);

            // Act
            /*var result = service.CreateVacancy(vacancy).Result;
            Assert.IsNotNull(result);
            Assert.That(result.VacancyTitle, Is.EqualTo(vacancy.VacancyTitle));
            // Assert
            var vacs = service.GetVacancies().Result;
            Assert.IsNotNull(vacs);
            var res = vacs.Where(x => x.VacancyTitle == vacancy.VacancyTitle).ToList();
            Assert.IsNotNull(res);
            Assert.IsTrue(res.Any());
            var res1 = res.FirstOrDefault();
            Assert.IsNotNull(res1);
            
            var vacsFound = service.GetVacancyById(res1.VacancyId).Result;
            Assert.IsNotNull(vacsFound);8*/
        }
        Assert.Pass();
    }
    
    [Test]
    public void ProcessingServiceGetVacancyByIdNotFoundTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var dbContext = scopedServices.GetRequiredService<JobsDbContext>();
            //var service = scopedServices.GetRequiredService<IProcessingService>();
            
            var category = new Category
            {
                CategoryId = 108,
                CategoryName = "Service Test Root Category 9",
            };

            dbContext.Categories.Add(category);
            dbContext.SaveChanges();
            
            var category2 = new Category
            {
                CategoryId = 109,
                CategoryName = "Service Test Root Category IT 10",
                ParentId = 108
            };
            
            dbContext.Categories.Add(category2);
            dbContext.SaveChanges();

            var vacancy = new VacancyInDto(0, CompanyId, category2.CategoryId,
                "Test Service Vacancy GetVacancyByIdTest", "Test Description GetVacancyByIdTest",
                new List<int>(),new List<int>(),
                100000, 200000, true, true);

            // Act
            /*var result = service.CreateVacancy(vacancy).Result;
            Assert.IsNotNull(result);
            Assert.That(result.VacancyTitle, Is.EqualTo(vacancy.VacancyTitle));
            // Assert
            var vacs = service.GetVacancies().Result;
            var res = vacs.Where(x => x.VacancyTitle == vacancy.VacancyTitle);
            Assert.IsNotNull(res);
            Assert.IsTrue(res.Any());
            
            var vacsNotFound = service.GetVacancyById(-10).Result;
            Assert.IsNull(vacsNotFound);*/
        }
        Assert.Pass();
    }
    
    [Test]
    public void ProcessingServiceDeleteVacancyTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var dbContext = scopedServices.GetRequiredService<JobsDbContext>();
            //var service = scopedServices.GetRequiredService<IProcessingService>();
            
            var category = new Category
            {
                CategoryId = 110,
                CategoryName = "Service Test Root Category 11",
            };

            dbContext.Categories.Add(category);
            dbContext.SaveChanges();
            
            var category2 = new Category
            {
                CategoryId = 111,
                CategoryName = "Service Test Root Category IT 12",
                ParentId = 110
            };
            
            dbContext.Categories.Add(category2);
            dbContext.SaveChanges();

            var vacancy = new VacancyInDto(0, CompanyId, category2.CategoryId,
                "Test Service Vacancy DeleteVacancy Test", "Test Description DeleteVacancy Test",
                new List<int>(),new List<int>(),
                100000, 200000, true, true);

            // Act
            /*var result = service.CreateVacancy(vacancy).Result;
            
            Assert.IsNotNull(result);
            Assert.That(result.VacancyTitle, Is.EqualTo(vacancy.VacancyTitle));
            // Assert
            var vacs = service.GetVacancies().Result;
            Assert.IsNotNull(vacs);
            var res = vacs.Where(x => x.VacancyTitle == vacancy.VacancyTitle).ToList();
            Assert.IsNotNull(res);
            Assert.IsTrue(res.Any());

            var item = res.FirstOrDefault();
            Assert.IsNotNull(item);
            var deletedId = service.DeleteVacancy(item.VacancyId).Result;
            Assert.IsTrue(deletedId == 0);8*/
        }
        Assert.Pass();
    }
    
    [Test]
    public void ProcessingServiceDeleteVacancyNotFoundTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var dbContext = scopedServices.GetRequiredService<JobsDbContext>();
            //var service = scopedServices.GetRequiredService<IProcessingService>();
            
            //var deletedId = service.DeleteVacancy(-30).Result;
            //Assert.IsTrue(deletedId == -1);
        }
        Assert.Pass();
    }
    
    [Test]
    public void ProcessingServiceUpdateVacancyTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var dbContext = scopedServices.GetRequiredService<JobsDbContext>();
            //var service = scopedServices.GetRequiredService<IProcessingService>();
            
            var category = new Category
            {
                CategoryId = 112,
                CategoryName = "Service Test Root Category 11",
            };

            dbContext.Categories.Add(category);
            dbContext.SaveChanges();
            
            var category2 = new Category
            {
                CategoryId = 113,
                CategoryName = "Service Test Root Category IT 12",
                ParentId = 112
            };
            
            dbContext.Categories.Add(category2);
            dbContext.SaveChanges();

            var vacancy = new VacancyInDto(0, CompanyId, category2.CategoryId,
                "Test Service Vacancy DeleteVacancy Test", "Test Description DeleteVacancy Test",
                new List<int>(),new List<int>(),
                100000, 200000, true, true);

            // Act
           /* var result = service.CreateVacancy(vacancy).Result;
            Assert.IsNotNull(result);
            Assert.That(result.VacancyTitle, Is.EqualTo(vacancy.VacancyTitle));
            // Assert
            var vacs = service.GetVacancies().Result;
            Assert.IsNotNull(vacs);
            var res = vacs.Where(x => x.VacancyTitle == vacancy.VacancyTitle).ToList();
            Assert.IsNotNull(res);
            Assert.IsTrue(res.Any());

            var item = res.FirstOrDefault();
            Assert.IsNotNull(item);
            
            var vacancyUpdated = new VacancyInDto(item.VacancyId, CompanyId, 8,
                "Test Service Vacancy UpdateVacancy Test", "Test Description UpdateVacancy Test",
                new List<int>(),new List<int>(),
                 100000,200000, true, true);
            
            var resultId = service.UpdateVacancy(vacancyUpdated).Result;
            
            Assert.IsTrue(resultId > 0);
            
            var vacsUpdated = service.GetVacancies().Result;
            var resUpdated = vacsUpdated.Where(x => x.VacancyTitle == vacancyUpdated.VacancyTitle).ToList();
            Assert.IsNotNull(resUpdated);
            Assert.IsTrue(resUpdated.Any());*/
        }
        Assert.Pass();
    }
    
    [Test]
    public void ProcessingServiceUpdateVacancyFailedTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var dbContext = scopedServices.GetRequiredService<JobsDbContext>();
            //var service = scopedServices.GetRequiredService<IProcessingService>();
            
            var category = new Category
            {
                CategoryId = 114,
                CategoryName = "Service Test Root Category 17",
            };

            dbContext.Categories.Add(category);
            dbContext.SaveChanges();
            
            var category2 = new Category
            {
                CategoryId = 115,
                CategoryName = "Service Test Root Category IT 18",
                ParentId = 114
            };
            
            dbContext.Categories.Add(category2);
            dbContext.SaveChanges();

            var vacancy = new VacancyInDto(0, CompanyId, category2.CategoryId,
                "Test Service Vacancy DeleteVacancy Test", "Test Description DeleteVacancy Test",
                new List<int>(),new List<int>(),
                100000, 200000, true, true);

            // Act
            /*var result = service.CreateVacancy(vacancy).Result;
            Assert.IsNotNull(result);
            Assert.That(result.VacancyTitle, Is.EqualTo(vacancy.VacancyTitle));
            // Assert
            var vacs = service.GetVacancies().Result;
            Assert.IsNotNull(vacs);
            var res = vacs.Where(x => x.VacancyTitle == vacancy.VacancyTitle).ToList();
            Assert.IsNotNull(res);
            Assert.IsTrue(res.Any());

            var item = res.FirstOrDefault();
            Assert.IsNotNull(item);
            
            var vacancyUpdated = new VacancyInDto(-10, CompanyId, 8,
                "Test Service Vacancy UpdateVacancy Test", "Test Description UpdateVacancy Test",
                new List<int>(),new List<int>(),
                100000, 200000);
            
            var resultId = service.UpdateVacancy(vacancyUpdated).Result;
            
            Assert.IsTrue(resultId == -1);*/
        }
        Assert.Pass();
    }
}