namespace FrontendAccountCreation.Web.ErrorNext;

public interface IErrorState
{
    bool HasErrors => Errors.Any();

    IEnumerable<IError> Errors { get; }

    IError? GetErrorIfTriggeredByElementId(params string[] mutuallyExclusiveErrorHtmlElementId);
}