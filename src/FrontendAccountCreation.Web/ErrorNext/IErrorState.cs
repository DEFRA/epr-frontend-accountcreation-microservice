namespace FrontendAccountCreation.Web.ErrorNext;

public interface IErrorState
{
    bool HasErrors { get; }

    //internal, rather than in interface?
    IEnumerable<Error> Errors { get; }

    Func<int, string>? ErrorIdToHtmlElementId { get; set; }

    bool HasTriggeredError(params int[] errorIds);

    Error? GetErrorIfTriggered(params int[] mutuallyExclusiveErrorIds);
}