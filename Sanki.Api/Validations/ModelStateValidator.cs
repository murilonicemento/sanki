using Microsoft.AspNetCore.Mvc.ModelBinding;
using Sanki.Api.Validations.Interfaces;

namespace Sanki.Api.Validations;

public class ModelStateValidator : IModelStateValidator
{
    public bool ValidateModelState(ModelStateDictionary modelState, out string errorMessages)
    {
        if (modelState.IsValid)
        {
            errorMessages = string.Empty;

            return true;
        }

        var errors = modelState.Values
            .SelectMany(value => value.Errors)
            .Select(errors => errors.ErrorMessage);
        errorMessages = string.Join(" | ", errors);

        return false;
    }
}