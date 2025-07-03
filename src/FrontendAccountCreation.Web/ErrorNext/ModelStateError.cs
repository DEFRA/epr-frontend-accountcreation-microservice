using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FrontendAccountCreation.Web.ErrorNext;

public class ModelStateError
    : IError
{
    public ModelStateError(KeyValuePair<string, ModelStateEntry?> modelStateEntry)
    {
        //todo: what to throw?
        //todo: localize here, rather than in the view for better testability
        Message = modelStateEntry.Value.Errors.FirstOrDefault()?.ErrorMessage ?? throw new ArgumentNullException(nameof(modelStateEntry));
        HtmlElementId = modelStateEntry.Key;
        InputErrorMessageParaId = $"error-message-{HtmlElementId}";
    }

    public string Message { get; }
    public string HtmlElementId { get; }
    public string InputErrorMessageParaId { get; }
}
