using FluentValidation;
using Jobs.DTO.In;

namespace Jobs.CompanyApi.Validators;

public class CompanyInDtoValidator : AbstractValidator<CompanyInDto>
{
    public CompanyInDtoValidator()
    {
        RuleFor(current => current.CompanyId)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Invalid CompanyId.");
        
        RuleFor(current => current.CompanyName)
            .NotEmpty()
            .Length(3,256)
            .WithMessage("VacancyTitle is required.");
        
        RuleFor(current => current.CompanyDescription)
            .NotEmpty()
            .Length(3,1000)
            .WithMessage("VacancyDescription is required.");
        
        RuleFor(current => current.CompanyLogoPath)
            .NotEmpty()
            .Length(3,256)
            .WithMessage("CompanyLogoPath is required.");
        
        /*RuleFor(current => current.CompanyLink)
            .NotEmpty()
            .Length(3,256)
            .WithMessage("CompanyLogoPath is required.");*/
    }
}