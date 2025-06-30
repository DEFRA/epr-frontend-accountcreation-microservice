
namespace FrontendAccountCreation.Web.FullPages.Radios.Common;

public static class CommonRadios
{
    public static Radio[] YesNo => new[]
    {
        new Radio("Yes", true.ToString()),
        new Radio("No", false.ToString())
    };
}