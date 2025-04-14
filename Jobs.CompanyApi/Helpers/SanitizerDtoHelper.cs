using Jobs.Common.Helpers;
using Jobs.DTO.In;

namespace Jobs.CompanyApi.Helpers;

public static class SanitizerDtoHelper
{
    public static CompanyInDto SanitizeCompanyInDto(CompanyInDto entity)
    {
        var result = new CompanyInDto(entity.CompanyId,
            HtmlSanitizerHelper.Sanitize(entity.CompanyName),
            HtmlSanitizerHelper.Sanitize(entity.CompanyDescription),
            HtmlSanitizerHelper.Sanitize(entity.CompanyLogoPath),
            HtmlSanitizerHelper.Sanitize(entity.CompanyLink),
            entity.IsActive,
            entity.IsVisible);
       
        return result;
    }
}