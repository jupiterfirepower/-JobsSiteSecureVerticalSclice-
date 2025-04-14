using FluentValidation;
using Jobs.DTO.In;

namespace Jobs.VacancyApi.Validators;

public class VacancyInDtoValidator : AbstractValidator<VacancyInDto>
{
    public VacancyInDtoValidator()
    {
        RuleFor(current => current.VacancyId)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Invalid VacancyId.");
        
        RuleFor(current => current.CompanyId)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("CompanyId is required.");
        
        RuleFor(current => current.CategoryId)
            .GreaterThan(0)
            .WithMessage("Invalid CategoryId.");
        
        RuleFor(current => current.VacancyTitle)
            .NotEmpty()
            .Length(3,256)
            .WithMessage("VacancyTitle is required.");
        
        RuleFor(current => current.VacancyDescription)
            .NotEmpty()
            .Length(3,10000)
            .WithMessage("VacancyDescription is required.");

        
        
        /*RuleFor(current => current.WorkTypeId)
            .GreaterThan(0)
            .InclusiveBetween(1,5)
            .When(current => current.WorkTypeId.HasValue)
            .WithMessage("Invalid WorkTypeId.");

        RuleFor(current => current.EmploymentTypeId)
            .GreaterThan(0)
            .InclusiveBetween(1,5)
            .When(current => current.EmploymentTypeId.HasValue)
            .WithMessage("Invalid WorkTypeId.");*/
    }
}