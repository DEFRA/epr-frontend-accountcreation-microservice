
namespace FrontendAccountCreation.Web.ErrorNext;

public class Error(PossibleError possibleError, ErrorState errorState)
    : IError
{
    //todo: does this need to be public?
    public int Id => possibleError.Id;
    public string Message => possibleError.ErrorMessage;

    public string HtmlElementId
    {
        get
        {
            if (errorState.ErrorIdToHtmlElementId == null)
            {
                throw new InvalidOperationException($"ErrorIdToHtmlElementId is null. Set it on {nameof(ErrorState)}.");
            }

            return errorState.ErrorIdToHtmlElementId(Id);
        }
    }

    //todo: tag helpers to add extra classes/aria-describedby to input element?

    public string InputErrorMessageParaId => $"{HtmlElementId}-error-message";
}