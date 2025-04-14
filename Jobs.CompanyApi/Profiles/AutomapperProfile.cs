using AutoMapper;
using Jobs.DTO;
using Jobs.DTO.In;
using Jobs.Entities.Models;

namespace Jobs.CompanyApi.Profiles;

public class AutomapperProfile : Profile
{
    public AutomapperProfile()
    {
        CreateMap<CompanyDto, Company>();
            //.ForMember(x => x.Category, opt => opt.Ignore());

        CreateMap<Company,CompanyDto>()
            .ConstructUsing(x => new CompanyDto(x.CompanyId, x.CompanyName, x.CompanyDescription, x.CompanyLogoPath, x.CompanyLink, x.IsVisible, x.IsActive));

        CreateMap<CompanyInDto, Company>();
        //.ForMember(x => x.Category, opt => opt.Ignore());
        // Add more mappings here
    }
}