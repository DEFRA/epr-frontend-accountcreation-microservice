
using Microsoft.Extensions.Localization;

namespace FrontendAccountCreation.Web.FullPages.Radios.Common;

public static class CommonRadios
{
    //todo: optimise this, so don't create new objects each time
    public static IRadio[] YesNo(IStringLocalizer<SharedResources> sharedLocalizer) =>
    [
        new Radio(sharedLocalizer["Yes"], true.ToString()),
        new Radio(sharedLocalizer["No"], false.ToString())
    ];
    
    //public static Radio[] YesNo =>
    //[
    //    new("Yes", true.ToString()),
    //    new("No", false.ToString())
    //];

    //public static Radio[] HomeNations => new[]
    //{
    //    new Radio("England", Country.England.ToString()),
    //    new Radio("Scotland", Country.Scotland.ToString()),
    //    new Radio("Wales", Country.Wales.ToString()),
    //    new Radio("Northern Ireland", Country.NorthernIreland.ToString())
    //};
}