
namespace FrontendAccountCreation.Web.FullPages.Radios.Common;

public static class CommonRadios
{
    public static Radio[] YesNo =>
    [
        new("Yes", true.ToString()),
        new("No", false.ToString())
    ];

    //public static Radio[] HomeNations => new[]
    //{
    //    new Radio("England", Country.England.ToString()),
    //    new Radio("Scotland", Country.Scotland.ToString()),
    //    new Radio("Wales", Country.Wales.ToString()),
    //    new Radio("Northern Ireland", Country.NorthernIreland.ToString())
    //};
}