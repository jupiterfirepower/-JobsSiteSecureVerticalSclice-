using Jobs.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Jobs.VacancyApi.Extentions;

public static class ModelBuilderExtension
{
    public static void Seed(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                CategoryId = 1,
                CategoryName = "Категорії вакансій",
                ParentId = null
            }
        );
        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 2, CategoryName = "IT, комп''ютери, інтернет", ParentId = 1 },
            new Category { CategoryId = 3, CategoryName = "Адмiнiстрацiя, керівництво середньої ланки", ParentId = 1 },
            new Category { CategoryId = 4, CategoryName = "Будівництво, архітектура", ParentId = 1 },
            new Category { CategoryId = 5, CategoryName = "Бухгалтерія, аудит", ParentId = 1 },
            new Category { CategoryId = 6, CategoryName = "Готельно-ресторанний бізнес, туризм", ParentId = 1 },
            new Category { CategoryId = 7, CategoryName = "Дизайн, творчість", ParentId = 1 },
            new Category { CategoryId = 8, CategoryName = "ЗМІ, видавництво, поліграфія", ParentId = 1 },
            new Category { CategoryId = 9, CategoryName = "Краса, фітнес, спорт", ParentId = 1 },
            new Category { CategoryId = 10, CategoryName = "Культура, музика, шоу-бізнес", ParentId = 1 },
            new Category { CategoryId = 11, CategoryName = "Логістика, склад, ЗЕД", ParentId = 1 },
            new Category { CategoryId = 12, CategoryName = "Маркетинг, реклама, PR", ParentId = 1 },
            new Category { CategoryId = 13, CategoryName = "Медицина, фармацевтика", ParentId = 1 },
            new Category { CategoryId = 14, CategoryName = "Нерухомість", ParentId = 1 },
            new Category { CategoryId = 15, CategoryName = "Освіта, наука", ParentId = 1 },
            new Category { CategoryId = 16, CategoryName = "Охорона, безпека", ParentId = 1 },
            new Category { CategoryId = 17, CategoryName = "Продаж, закупівля", ParentId = 1 },
            new Category { CategoryId = 18, CategoryName = "Робочі спеціальності, виробництво", ParentId = 1 },
            new Category { CategoryId = 19, CategoryName = "Роздрібна торгівля", ParentId = 1 },
            new Category { CategoryId = 20, CategoryName = "Секретаріат, діловодство, АГВ", ParentId = 1 },
            new Category { CategoryId = 21, CategoryName = "Сільське господарство, агробізнес", ParentId = 1 },
            new Category { CategoryId = 22, CategoryName = "Страхування", ParentId = 1 },
            new Category { CategoryId = 23, CategoryName = "Сфера обслуговування", ParentId = 1 },
            new Category { CategoryId = 24, CategoryName = "Телекомунікації та зв'язок", ParentId = 1 },
            new Category { CategoryId = 25, CategoryName = "Топменеджмент, керівництво вищої ланки", ParentId = 1 },
            new Category { CategoryId = 26, CategoryName = "Транспорт, автобізнес", ParentId = 1 },
            new Category { CategoryId = 27, CategoryName = "Управління персоналом", ParentId = 1 },
            new Category { CategoryId = 28, CategoryName = "Фінанси, банк", ParentId = 1 },
            new Category { CategoryId = 29, CategoryName = "Юриспруденція", ParentId = 1 }
        );
        
        modelBuilder.Entity<SecretApiKey>().HasData(
            new SecretApiKey
            {
                KeyId = 1,
                Key = "123456789123456789123",
                IsActive = true,
                Created = DateTime.UtcNow
            }
        );
        
        modelBuilder.Entity<Company>().HasData(
            new Company { CompanyId = 1, CompanyName = "Test Company", 
                CompanyDescription = "Test Company Description",
                CompanyLogoPath = "/company/logo.png",
                CompanyLink = "/company/link",
                IsActive = true, Created = DateTime.UtcNow
            }
        );
        
        modelBuilder.Entity<WorkType>().HasData(
            new WorkType { WorkTypeId = 1, WorkTypeName = "Office", Created = DateTime.UtcNow },
            new WorkType { WorkTypeId = 2, WorkTypeName = "Remote", Created = DateTime.UtcNow },
            new WorkType { WorkTypeId = 3, WorkTypeName = "Office/Remote", Created = DateTime.UtcNow },
            new WorkType { WorkTypeId = 4, WorkTypeName = "Hybrid", Created = DateTime.UtcNow }
        );
        
        //  full-time, part-time, temporary, contract, and freelance
        modelBuilder.Entity<EmploymentType>().HasData(
            new EmploymentType { EmploymentTypeId = 1, EmploymentTypeName = "full-time", Created = DateTime.UtcNow },
            new EmploymentType { EmploymentTypeId = 2, EmploymentTypeName = "part-time", Created = DateTime.UtcNow },
            new EmploymentType { EmploymentTypeId = 3, EmploymentTypeName = "temporary", Created = DateTime.UtcNow },
            new EmploymentType { EmploymentTypeId = 4, EmploymentTypeName = "contract", Created = DateTime.UtcNow },
            new EmploymentType { EmploymentTypeId = 5, EmploymentTypeName = "freelance", Created = DateTime.UtcNow }
        );
        
        // test data
        modelBuilder.Entity<CompanyOwnerEmails>().HasData(
            new CompanyOwnerEmails { CompanyOwnerEmailsId = 1, CompanyId = 1, UserEmail = "jupiterfiretetraedr@gmail.com", Created = DateTime.UtcNow }
        );
    }
}

