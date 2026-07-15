using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace SmartCampus.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected async Task<ActionResult?> ValidateAsync<T>(
        IValidator<T> validator,
        T request,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(request, cancellationToken);
        if (result.IsValid)
        {
            return null;
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        return ValidationProblem(ModelState);
    }
}
