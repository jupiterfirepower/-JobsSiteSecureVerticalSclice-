using Jobs.Common.Contracts;
using Jobs.Entities.Models;
using Jobs.ReferenceApi.Repositories;
using Jobs.VacancyApi.Data;
using Jobs.VacancyApi.Repository;
using JobsWebApiNUnitTests.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JobsWebApiNUnitTests;

public class Tests
{
    private ServiceProvider _serviceProvider;
    private int CompanyId = 1;
    
    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();

        // Using In-Memory database for testing
        services.AddDbContext<JobsDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));
        
        services.AddScoped<IGenericRepository<EmploymentType>, EmploymentTypeRepository>();
        services.AddScoped<IGenericRepository<WorkType>, WorkTypeRepository>();
        services.AddScoped<IGenericRepository<Vacancy>, VacancyRepository>();

        _serviceProvider = services.BuildServiceProvider();
    }
    
    [TearDown]
    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    [Test]
    public void RepositoryAddVacancyTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var repository = scopedServices.GetRequiredService<IGenericRepository<Vacancy>>();
            var dbContext = scopedServices.GetRequiredService<JobsDbContext>();

            var vacancy = new Vacancy
            {
                CompanyId = CompanyId,
                VacancyTitle = "Test Vacancy",
                VacancyDescription = "Test Description",
                IsActive = true,
                SalaryFrom = 10000,
                SalaryTo = 120000
            };

            // Act
            repository.Add(vacancy);
            repository.SaveAsync();
    
            // Assert
            var addedItem = dbContext.Vacancies.Find(vacancy.Id);
            Assert.IsNotNull(addedItem);
            Assert.That(addedItem.VacancyTitle, Is.EqualTo(vacancy.VacancyTitle));
        }
        Assert.Pass();
    }
    
    [Test]
    public void RepositorySelectVacancyTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var repository = scopedServices.GetRequiredService<IGenericRepository<Vacancy>>();
            var dbContext = scopedServices.GetRequiredService<JobsDbContext>();

            var vacancy = new Vacancy
            {
                CompanyId = CompanyId,
                VacancyTitle = "Test Vacancy",
                VacancyDescription = "Test Description",
                IsActive = true,
                SalaryFrom = 10000,
                SalaryTo = 120000
            };

            // Act
            repository.Add(vacancy);
            repository.SaveAsync();
    
            // Assert
            var addedItem = dbContext.Vacancies.Find(vacancy.Id);
            Assert.IsNotNull(addedItem);
            Assert.That(addedItem.VacancyTitle, Is.EqualTo(vacancy.VacancyTitle));
            
            var current = repository.Select(x=>x.VacancyId==addedItem.VacancyId);
            Assert.IsNotNull(addedItem);
            Assert.That(addedItem.VacancyTitle, Is.EqualTo(current.VacancyTitle));
        }
        Assert.Pass();
    }
    
    [Test]
    public void RepositorySelectVacancyAsyncTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var repository = scopedServices.GetRequiredService<IGenericRepository<Vacancy>>();
            var dbContext = scopedServices.GetRequiredService<JobsDbContext>();

            var vacancy = new Vacancy
            {
                CompanyId = CompanyId,
                VacancyTitle = "Test Vacancy",
                VacancyDescription = "Test Description",
                IsActive = true,
                SalaryFrom = 10000,
                SalaryTo = 120000
            };

            // Act
            repository.Add(vacancy);
            repository.SaveAsync();
    
            // Assert
            var addedItem = dbContext.Vacancies.Find(vacancy.Id);
            Assert.IsNotNull(addedItem);
            Assert.That(addedItem.VacancyTitle, Is.EqualTo(vacancy.VacancyTitle));
            
            var current = repository.SelectAsync(x=>x.VacancyId==addedItem.VacancyId).Result;
            Assert.IsNotNull(addedItem);
            Assert.That(addedItem.VacancyTitle, Is.EqualTo(current.VacancyTitle));
        }
        Assert.Pass();
    }
    
    [Test]
    public void RepositoryGetByIdWithIncludesTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var repository = scopedServices.GetRequiredService<IGenericRepository<Vacancy>>();
            var dbContext = scopedServices.GetRequiredService<JobsDbContext>();
            
            var category = new Category
            {
                CategoryId = 3,
                CategoryName = "Root Category 3",
            };

            dbContext.Categories.Add(category);
            dbContext.SaveChanges();
            
            var category2 = new Category
            {
                CategoryId = 4,
                CategoryName = "Category 4",
                ParentId = 3
            };
            
            dbContext.Categories.Add(category2);
            dbContext.SaveChanges();

            var vacancy = new Vacancy
            {
                CompanyId = CompanyId,
                VacancyTitle = "Test Vacancy",
                VacancyDescription = "Test Description",
                IsActive = true,
                SalaryFrom = 10000,
                SalaryTo = 120000,
                CategoryId = 4
            };

            // Act
            repository.Add(vacancy);
            repository.SaveAsync();
    
            // Assert
            var addedItem = repository.GetById(vacancy.Id);
            Assert.IsNotNull(addedItem);
            Assert.That(addedItem.VacancyTitle, Is.EqualTo(vacancy.VacancyTitle));

            var current = repository.GetByIdWithIncludes(vacancy.Id);
            Assert.IsNotNull(current);
            Assert.That(current.Category.CategoryId, Is.EqualTo(4));
        }
        Assert.Pass();
    }
    
    [Test]
    public void RepositoryGetByIdWithIncludesAsyncTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var repository = scopedServices.GetRequiredService<IGenericRepository<Vacancy>>();
            var dbContext = scopedServices.GetRequiredService<JobsDbContext>();
            
            var category = new Category
            {
                CategoryId = 1,
                CategoryName = "Root Category",
            };

            dbContext.Categories.Add(category);
            dbContext.SaveChanges();
            
            var category2 = new Category
            {
                CategoryId = 2,
                CategoryName = "Category 2",
                ParentId = 1
            };
            
            dbContext.Categories.Add(category2);
            dbContext.SaveChanges();

            var vacancy = new Vacancy
            {
                CompanyId = CompanyId,
                CategoryId = 2,
                VacancyTitle = "Test Vacancy",
                VacancyDescription = "Test Description",
                IsActive = true,
                SalaryFrom = 10000,
                SalaryTo = 120000
            };

            // Act
            repository.Add(vacancy);
            repository.SaveAsync();
    
            // Assert
            var addedItem = repository.GetById(vacancy.Id);
            Assert.IsNotNull(addedItem);
            Assert.That(addedItem.VacancyTitle, Is.EqualTo(vacancy.VacancyTitle));

            var current = repository.GetByIdWithIncludesAsync(vacancy.Id).Result;
            Assert.IsNotNull(current);
            Assert.That(current.Category.CategoryId, Is.EqualTo(2));
        }
        Assert.Pass();
    }
    
    //(int id)
    
    [Test]
    public void RepositoryAddVacancyIdFailedTest()
    {
        // Arrange
        var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var repository = scopedServices.GetRequiredService<IGenericRepository<Vacancy>>();

        var vacancy = new Vacancy
        {
            VacancyId = -1,
            CompanyId = CompanyId,
            VacancyTitle = "Test Vacancy",
            VacancyDescription = "Test Description",
            IsActive = true,
            SalaryFrom = 10000,
            SalaryTo = 120000
        };
            
        try
        {
            // Act
            repository.Add(vacancy);
            repository.SaveAsync();
            Assert.Fail();
        }
        catch (Exception)
        {
            Assert.Pass();
        }
    }
    
    [Test]
    public void RepositoryUpdateVacancyTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var repository = scopedServices.GetRequiredService<IGenericRepository<Vacancy>>();
            var dbContext = scopedServices.GetRequiredService<JobsDbContext>();
            
            var workTypeOffice = new WorkType
            {
                WorkTypeId = 1,
                WorkTypeName = "Office",
                Created = DateTime.UtcNow
            };
            
            dbContext.WorkTypes.Add(workTypeOffice);
            dbContext.SaveChanges();
            
            var workType = new WorkType
            {
                WorkTypeId = 2,
                WorkTypeName = "Remote",
                Created = DateTime.UtcNow
            };
            
            dbContext.WorkTypes.Add(workType);
            dbContext.SaveChanges();

            var vacancy = new Vacancy
            {
                CompanyId = CompanyId,
                VacancyTitle = "Test Vacancy",
                VacancyDescription = "Test Description",
                IsActive = true,
                SalaryFrom = 10000,
                SalaryTo = 120000
            };

            // Act
            repository.Add(vacancy);
            repository.SaveAsync();
    
            // Assert
            var addedItem = dbContext.Vacancies.Find(vacancy.Id);
            Assert.IsNotNull(addedItem);
            Assert.IsTrue(addedItem.VacancyId > 0);
            Assert.That(addedItem.VacancyTitle, Is.EqualTo(vacancy.VacancyTitle));
            
            vacancy.VacancyTitle = "New Vacancy";
            vacancy.VacancyDescription = "New Description";
            vacancy.SalaryFrom = 10000;
            vacancy.SalaryTo = 120000;
            vacancy.IsActive = false;
            
            // Act
            repository.Update(vacancy);
            repository.SaveAsync();
            
            // Assert
            var updatedItem = dbContext.Vacancies.Find(vacancy.Id);
            Assert.IsNotNull(updatedItem);
            Assert.That(updatedItem.VacancyTitle, Is.EqualTo(vacancy.VacancyTitle));
            Assert.That(updatedItem.VacancyTitle, Is.EqualTo(vacancy.VacancyTitle));
            Assert.That(updatedItem.VacancyDescription, Is.EqualTo(vacancy.VacancyDescription));
            Assert.That(updatedItem.SalaryFrom, Is.EqualTo(vacancy.SalaryFrom));
            Assert.That(updatedItem.SalaryTo, Is.EqualTo(vacancy.SalaryTo));
            Assert.That(updatedItem.IsActive, Is.EqualTo(vacancy.IsActive));
        }
        Assert.Pass();
    }
    
    [Test]
    public void RepositoryAddVacancyVacancyNameFailedTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var repository = scopedServices.GetRequiredService<IGenericRepository<Vacancy>>();
            var dbContext = scopedServices.GetRequiredService<JobsDbContext>();

            var vacancy = new Vacancy
            {
                CompanyId = CompanyId,
                VacancyTitle = RandomStringHelper.RandomString(300),
                VacancyDescription = "Test Description",
                IsActive = true,
                SalaryFrom = 10000,
                SalaryTo = 120000
            };

            Assert.IsTrue(vacancy.VacancyTitle.Length > 256);
            //Assert.IsTrue(vacancy. VacancyName.Length > 256);

            try
            {
                // Act
                repository.Add(vacancy);
                repository.SaveAsync();
            }
            catch (Exception)
            {
                Assert.Pass();
            }
            
            var vacancy2 = new Vacancy
            {
                CompanyId = CompanyId,
                VacancyTitle = RandomStringHelper.RandomString(300),
                VacancyDescription = "Test Description",
                IsActive = true,
                SalaryFrom = 10000,
                SalaryTo = 120000
            };
            
            Assert.IsTrue(vacancy2.VacancyTitle.Length > 256);

            try
            {
                // Act
                repository.Add(vacancy2);
                repository.SaveAsync();
            }
            catch (Exception)
            {
                Assert.Pass();
            }
            
            var vacancy3 = new Vacancy
            {
                VacancyId = 0,
                CompanyId = CompanyId,
                VacancyTitle = "Test Vacancy",
                VacancyDescription = "Test Description",
                IsActive = true,
                SalaryFrom = -10000,
                SalaryTo = -120000
            };
            
            try
            {
                // Act
                repository.Add(vacancy3);
                repository.SaveAsync();
            }
            catch (Exception)
            {
                Assert.Pass();
            }
            
            var vacancy4 = new Vacancy
            {
                VacancyId = 0,
                CompanyId = CompanyId,
                VacancyTitle = "Test Vacancy",
                VacancyDescription = "Test Description",
                IsActive = true,
                SalaryFrom = 10000,
                SalaryTo = 120000
            };
            
            try
            {
                // Act
                repository.Add(vacancy4);
                repository.SaveAsync();
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }
    }
    
    [Test]
    public void RepositoryAddVacancyNameFailedTest()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        {
            var scopedServices = scope.ServiceProvider;
            var repository = scopedServices.GetRequiredService<IGenericRepository<Vacancy>>();

            var vacancy = new Vacancy
            {
                CompanyId = CompanyId,
                VacancyTitle = RandomStringHelper.RandomString(300),
                VacancyDescription = "Test Description",
                IsActive = true,
                SalaryFrom = 10000,
                SalaryTo = 120000
            };

            Assert.IsTrue(vacancy.VacancyTitle.Length > 256);

            try
            {
                // Act
                repository.Add(vacancy);
                repository.SaveAsync();
            }
            catch (Exception)
            {
                Assert.Pass();
            }
        }
    }
    
    [Test]
    public void RepositoryAddVacancyDescriptionFailedTest()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        {
            var scopedServices = scope.ServiceProvider;
            var repository = scopedServices.GetRequiredService<IGenericRepository<Vacancy>>();

            var vacancy = new Vacancy
            {
                CompanyId = CompanyId,
                VacancyTitle = "Test Vacancy",
                VacancyDescription = RandomStringHelper.RandomString(11000),
                IsActive = true,
                SalaryFrom = 10000,
                SalaryTo = 120000
            };

            Assert.IsTrue(vacancy.VacancyDescription.Length > 10000);

            try
            {
                // Act
                repository.Add(vacancy);
                repository.SaveAsync();
            }
            catch (Exception)
            {
                Assert.Pass();
            }
        }
    }
    
    [Test]
    public void RepositoryRemoveVacancyTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var repository = scopedServices.GetRequiredService<IGenericRepository<Vacancy>>();
            var dbContext = scopedServices.GetRequiredService<JobsDbContext>();

            var vacancy = new Vacancy
            {
                CompanyId = CompanyId,
                VacancyTitle = "Test Vacancy",
                VacancyDescription = "Test Description",
                IsActive = true,
                SalaryFrom = 10000,
                SalaryTo = 120000
            };

            // Act
            repository.Add(vacancy);
            repository.SaveAsync();
    
            // Assert
            var addedItem = dbContext.Vacancies.Find(vacancy.Id);
            Assert.IsNotNull(addedItem);
            Assert.IsTrue(addedItem.VacancyId > 0);
            Assert.That(addedItem.VacancyTitle, Is.EqualTo(vacancy.VacancyTitle));
            
            // Act
            repository.Remove(vacancy.VacancyId);
            repository.SaveAsync();
            // Assert
            var removedItem = dbContext.Vacancies.Find(vacancy.Id);
            Assert.IsNull(removedItem);
        }
        Assert.Pass();
    }
    
    [Test]
    public void RepositoryRemoveUnrealVacancyTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var repository = scopedServices.GetRequiredService<IGenericRepository<Vacancy>>();
            var dbContext = scopedServices.GetRequiredService<JobsDbContext>();

            var vacancy = new Vacancy
            {
                CompanyId = CompanyId,
                VacancyTitle = "Test Vacancy",
                VacancyDescription = "Test Description",
                IsActive = true,
                SalaryFrom = 10000,
                SalaryTo = 120000
            };

            // Act
            repository.Add(vacancy);
            repository.SaveAsync();
    
            // Assert
            var addedItem = dbContext.Vacancies.Find(vacancy.Id);
            Assert.IsNotNull(addedItem);
            Assert.IsTrue(addedItem.VacancyId > 0);
            Assert.That(addedItem.VacancyTitle, Is.EqualTo(vacancy.VacancyTitle));
            
            // Act
            repository.Remove(vacancy.VacancyId);
            repository.SaveAsync();
            // Assert
            var removedItem = dbContext.Vacancies.Find(vacancy.Id);
            Assert.IsNull(removedItem);
            
            repository.Remove(10000);
            repository.SaveAsync();
            var removedUnrealItem = dbContext.Vacancies.Find(10000);
            Assert.IsNull(removedUnrealItem);
        }
        Assert.Pass();
    }
    
    [Test]
    public void RepositoryGetVacancyTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var repository = scopedServices.GetRequiredService<IGenericRepository<Vacancy>>();
            var dbContext = scopedServices.GetRequiredService<JobsDbContext>();

            var vacancy = new Vacancy
            {
                CompanyId = CompanyId,
                VacancyTitle = "Test Vacancy",
                VacancyDescription = "Test Description",
                IsActive = true,
                SalaryFrom = 10000,
                SalaryTo = 120000
            };

            // Act
            repository.Add(vacancy);
            repository.SaveAsync();
    
            // Assert
            var addedItem = repository.GetById(vacancy.Id);
            Assert.IsNotNull(addedItem);
            Assert.That(addedItem.VacancyTitle, Is.EqualTo(vacancy.VacancyTitle));
        }
        Assert.Pass();
    }
    
    [Test]
    public void RepositoryGetAllTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var repository = scopedServices.GetRequiredService<IGenericRepository<Vacancy>>();
            var dbContext = scopedServices.GetRequiredService<JobsDbContext>();

            var vacancy = new Vacancy
            {
                CompanyId = CompanyId,
                VacancyTitle = "Test Vacancy",
                VacancyDescription = "Test Description",
                IsActive = true,
                SalaryFrom = 10000,
                SalaryTo = 120000
            };

            // Act
            repository.Add(vacancy);
            repository.SaveAsync();
    
            // Assert
            var allItems = repository.GetAll();
            Assert.IsNotNull(allItems);
            Assert.IsTrue(allItems.Any());
        }
        Assert.Pass();
    }
    
    [Test]
    public void RepositoryGetAllAsyncTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var repository = scopedServices.GetRequiredService<IGenericRepository<Vacancy>>();
            var dbContext = scopedServices.GetRequiredService<JobsDbContext>();

            var vacancy = new Vacancy
            {
                VacancyId = 0,
                CompanyId = CompanyId,
                VacancyTitle = "Test Vacancy",
                VacancyDescription = "Test Description",
                IsActive = true,
                SalaryFrom = 10000,
                SalaryTo = 120000
            };

            // Act
            repository.Add(vacancy);
            repository.SaveAsync();
    
            // Assert
            var allItems = repository.GetAllAsync().Result;
            Assert.IsNotNull(allItems);
            Assert.IsTrue(allItems.Any());
        }
        Assert.Pass();
    }
    
    [Test]
    public void RepositoryGetVacancyAsyncTest()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var repository = scopedServices.GetRequiredService<IGenericRepository<Vacancy>>();
            //var dbContext = scopedServices.GetRequiredService<JobsDbContext>();

            var vacancy = new Vacancy
            {
                CompanyId = CompanyId,
                VacancyTitle = "Test Vacancy",
                VacancyDescription = "Test Description",
                IsActive = true,
                SalaryFrom = 10000,
                SalaryTo = 120000
            };

            // Act
            repository.Add(vacancy);
            repository.SaveAsync();
    
            // Assert
            var addedItem = repository.GetByIdAsync(vacancy.Id).Result;
            Assert.IsNotNull(addedItem);
            Assert.That(addedItem.VacancyTitle, Is.EqualTo(vacancy.VacancyTitle));
        }
        Assert.Pass();
    }
}