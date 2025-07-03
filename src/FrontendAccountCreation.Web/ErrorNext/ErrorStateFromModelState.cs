using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FrontendAccountCreation.Web.ErrorNext;

public class ErrorStateFromModelState : IErrorState
{
    public static IErrorState Create(ModelStateDictionary modelState)
    {
        return modelState.Count == 0 ? Empty : new ErrorStateFromModelState(modelState);
    }

    public ErrorStateFromModelState(ModelStateDictionary? modelState)
    {
        //todo: errorids if we keep it
        //ErrorIds = triggeredErrorIds;
        Errors = modelState?.Select(e => new ModelStateError(e))
            ?? [];
    }

    public static IErrorState Empty { get; }
    //this would work, but better to return same type
    //= new ErrorState(ImmutableDictionary<int, PossibleError>.Empty, []);
        = new ErrorStateFromModelState(null);

    public bool HasTriggeredError(params int[] errorIds)
    {
        //todo: what do we use for errorIds for modelstate errors?
        return false;
    }

    public int? GetErrorIdIfTriggered(params int[] mutuallyExclusiveErrorIds)
    {
        //todo: what do we use for errorIds for modelstate errors?
        return null;
    }

    public IError? GetErrorIfTriggered(params int[] mutuallyExclusiveErrorIds)
    {
        //todo: 
        return null;
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

    public Func<int, string>? ErrorIdToHtmlElementId { get; set; }
    public bool HasErrors => Errors.Any();
    private IEnumerable<int> ErrorIds { get; }
    public IEnumerable<IError> Errors { get; }
}
