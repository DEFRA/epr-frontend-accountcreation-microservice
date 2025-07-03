using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ErrorNext;

public class ErrorState : IErrorState
{
    public ErrorState(ImmutableDictionary<int, PossibleError> possibleErrors, IEnumerable<int> triggeredErrorIds)
    {
        ErrorIds = triggeredErrorIds;
        Errors = triggeredErrorIds.Select(e => new Error(possibleErrors[e], this));
    }

    public static IErrorState Create<T>(ImmutableDictionary<int, PossibleError> possibleErrors, params T[] triggeredErrorIds)
        where T : struct, Enum, IConvertible
    {
        if (triggeredErrorIds.Any())
        {
            return new ErrorState(possibleErrors, triggeredErrorIds.Select(e => (int)(IConvertible)e));
        }

        return ErrorStateEmpty.Instance;
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

    public IError? GetErrorIfTriggeredByElementId(params string[] mutuallyExclusiveErrorHtmlElementId)
    {
        //todo: untested for non dataannotation errors (but should work)
        
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