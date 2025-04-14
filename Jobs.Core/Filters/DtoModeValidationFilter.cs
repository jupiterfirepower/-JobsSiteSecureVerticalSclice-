using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Jobs.Core.Filters;

public class DtoModeValidationFilter<T>(IValidator<T> validator) : IEndpointFilter where T : class
{
    async ValueTask<object?> IEndpointFilter.InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (validator is not null)
        {
            var model = context.Arguments
                .OfType<T>()
                .FirstOrDefault(a => a?.GetType() == typeof(T));
                
            if ( model is not null)
            {
                var results = await validator.ValidateAsync(model);
                if (!results.IsValid)
                {
                    return TypedResults.BadRequest();
                }
            }
            else
            {
                return TypedResults.BadRequest();
            }
        }

        return await next(context);

    }
}
