namespace FrontendAccountCreation.Web.ErrorNext;

public interface IErrorState
{
    bool HasErrors { get; }

    //internal, rather than in interface?
    IEnumerable<IError> Errors { get; }

    Func<int, string>? ErrorIdToHtmlElementId { get; set; }

    //todo: generic version that uses GetErrorIfTriggered
    bool HasTriggeredError(params int[] errorIds);

    IError? GetErrorIfTriggered(params int[] mutuallyExclusiveErrorIds);

    IError? GetErrorIfTriggeredByElementId(params string[] mutuallyExclusiveErrorHtmlElementId);
}