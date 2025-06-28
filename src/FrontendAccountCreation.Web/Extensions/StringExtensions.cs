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

    public static TEnum? ToEnumOrNull<TEnum>(this string? value) where TEnum : struct, Enum
    {
        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var result) ? result : (TEnum?)null;
    }
}
