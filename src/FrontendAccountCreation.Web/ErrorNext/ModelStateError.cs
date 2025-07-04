using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FrontendAccountCreation.Web.ErrorNext;

public class ModelStateError
    : IError
{
    public ModelStateError(KeyValuePair<string, ModelStateEntry?> modelStateEntry)
    {
        var firstError = modelStateEntry.Value.Errors.FirstOrDefault();
        if (firstError == null)
        {
            throw new InvalidOperationException("No errors found in the model state entry.");
        }
        Message = firstError.ErrorMessage;
        HtmlElementId = modelStateEntry.Key;
        InputErrorMessageParaId = $"error-message-{HtmlElementId}";
    }

    public string Message { get; }
    public string HtmlElementId { get; }
    public string InputErrorMessageParaId { get; }
}
