namespace FrontendAccountCreation.Web.ErrorNext;

// we could use one of the other error states as the empty state, but this is clearer
public class ErrorStateEmpty : IErrorState
{
    public static ErrorStateEmpty Instance => new();

    public bool HasErrors => false;
    public IEnumerable<IError> Errors => [];
    public IError? GetErrorIfTriggeredByElementId(params string[] mutuallyExclusiveErrorHtmlElementId) => null;
}
