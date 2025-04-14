using AutoMapper;
using Jobs.DTO;
using Jobs.DTO.In;
using Jobs.Entities.Models;

namespace JobsWebApiNUnitTests.Profiles;

public class TestAutomapperProfile : Profile
{
    public TestAutomapperProfile()
    {
        CreateMap<VacancyDto, Vacancy>()
            .ForMember(x => x.Category, opt => opt.Ignore());
        
        //CreateMap<VacancyInDto, Vacancy>();
        //.ForMember(x => x.WorkTypeId, opt => opt.Ignore())
        //.ForMember(x => x.EmploymentTypeId, opt => opt.Ignore());

        CreateMap<Vacancy, VacancyDto>()
            .ConstructUsing(x => new VacancyDto(x.VacancyId, 
                x.CompanyId, x.CategoryId, 
                x.VacancyTitle, x.VacancyDescription, 
                x.SalaryFrom, x.SalaryTo,
                x.IsVisible, x.IsActive, 
                x.Created, x.Modified));
        
        CreateMap<VacancyInDto, Vacancy>()
            .ForMember(x => x.Category, opt => opt.Ignore());
        
        CreateMap<WorkTypeDto, WorkType>().ReverseMap();
        
        CreateMap<EmploymentTypeDto, EmploymentType>().ReverseMap();
        
        CreateMap<CategoryDto, Category>().ReverseMap();
        // Add more mappings here
    }
}