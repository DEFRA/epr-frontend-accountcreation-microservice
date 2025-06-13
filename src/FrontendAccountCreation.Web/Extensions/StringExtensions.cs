namespace FrontendAccountCreation.Web.Extensions;

public static class StringExtensions
{
    public static string WithoutControllerSuffix(this string controllerName)
    {
        if (!string.IsNullOrEmpty(controllerName) && controllerName.ToLowerInvariant().EndsWith("controller"))
        {
            controllerName = controllerName.Remove(controllerName.Length - "controller".Length);
        }

        return controllerName;
    }
}
