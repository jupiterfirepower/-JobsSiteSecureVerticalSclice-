using Jobs.Common.Helpers;
using Jobs.DTO.In;

namespace Jobs.VacancyApi.Features.Vacancies.Helpers;

public class SanitizeHelper
{
    public static VacancyInDto SanitizeVacancyInDto(VacancyInDto entity) => entity with { 
        VacancyTitle = HtmlSanitizerHelper.Sanitize(entity.VacancyTitle), 
        VacancyDescription = HtmlSanitizerHelper.Sanitize(entity.VacancyDescription),
    };
}