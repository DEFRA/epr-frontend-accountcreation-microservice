using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FrontendAccountCreation.Web.ErrorNext;

public class ErrorStateFromModelState : IErrorState
{
    public static IErrorState Create(ModelStateDictionary modelState)
    {
        if (modelState.Any())
        {
            return new ErrorStateFromModelState(modelState);
        }

        return Empty;
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

    public Func<int, string>? ErrorIdToHtmlElementId { get; set; }
    public bool HasErrors => ErrorIds.Any();
    private IEnumerable<int> ErrorIds { get; }
    public IEnumerable<IError> Errors { get; }
}

public class ErrorState : IErrorState
{
    public ErrorState(ImmutableDictionary<int, PossibleError> possibleErrors, IEnumerable<int> triggeredErrorIds)
    {
        ErrorIds = triggeredErrorIds;
        Errors = triggeredErrorIds.Select(e => new Error(possibleErrors[e], this));
    }

    public static IErrorState Empty { get; }
        = new ErrorState(ImmutableDictionary<int, PossibleError>.Empty, []);

    public static IErrorState Create<T>(ImmutableDictionary<int, PossibleError> possibleErrors, params T[] triggeredErrorIds)
        where T : struct, Enum, IConvertible
    {
        if (triggeredErrorIds.Any())
        {
            return new ErrorState(possibleErrors, triggeredErrorIds.Select(e => (int)(IConvertible)e));
        }

        return Empty;
    }

    public Func<int, string>? ErrorIdToHtmlElementId { get; set; }

    public bool HasErrors => ErrorIds.Any();

    //todo: remove this?
    private IEnumerable<int> ErrorIds { get; }
    public IEnumerable<IError> Errors { get; }

    public bool HasTriggeredError(params int[] errorIds)
    {
        return GetErrorIdIfTriggered(errorIds) != null;
    }

    //todo: roll into next method?
    [SuppressMessage("Minor Code Smell", "S3267:Loops should be simplified with \"LINQ\" expressions", Justification = "LINQ expression version is less simple")]
    private int? GetErrorIdIfTriggered(params int[] mutuallyExclusiveErrorIds)
    {
        if (!mutuallyExclusiveErrorIds.Any())
        {
            // if no error ids supplied, returns the first error (if there is one)
            // this is only really useful where there is only one input on the page
            return ErrorIds.Any() ? ErrorIds.First() : null;
        }

        foreach (int errorId in mutuallyExclusiveErrorIds)
        {
            if (ErrorIds.Contains(errorId))
            {
                return errorId;
            }
        }

        return null;
    }

    public IError? GetErrorIfTriggered(params int[] mutuallyExclusiveErrorIds)
    {
        int? currentErrorId = GetErrorIdIfTriggered(mutuallyExclusiveErrorIds);
        return currentErrorId != null ? Errors.First(e => e.Id == currentErrorId) : null;
    }
}