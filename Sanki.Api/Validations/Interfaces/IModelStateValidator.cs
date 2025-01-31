using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Sanki.Api.Validations.Interfaces;

public interface IModelStateValidator
{
    public bool ValidateModelState(ModelStateDictionary modelState, out string errorMessages);
}