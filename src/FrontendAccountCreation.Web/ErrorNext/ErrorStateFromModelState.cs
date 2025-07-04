using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FrontendAccountCreation.Web.ErrorNext;

public class ErrorStateFromModelState : IErrorState
{
    public static IErrorState Create(ModelStateDictionary modelState)
    {
        return modelState.Count == 0 ? new ErrorStateEmpty() : new ErrorStateFromModelState(modelState);
    }

    public IEnumerable<IError> Errors { get; }

    public ErrorStateFromModelState(ModelStateDictionary? modelState)
    {
        Errors = modelState?.Select(e => new ModelStateError(e))
            ?? [];
    }

    public IError? GetErrorIfTriggeredByElementId(params string[] mutuallyExclusiveErrorHtmlElementId)
    {
        if (mutuallyExclusiveErrorHtmlElementId.Length == 0)
        {
            // if no element IDs supplied, return the first error (if there is one)
            return Errors.FirstOrDefault();
        }

        foreach (var htmlElementId in mutuallyExclusiveErrorHtmlElementId)
        {
            var error = Errors.FirstOrDefault(e => e.HtmlElementId == htmlElementId);
            if (error != null)
            {
                return error;
            }
        }

        return null;
    }
}
