using FrontendAccountCreation.Core.Sessions;
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

    public static IRadio[] YesNoNotSure(IStringLocalizer<SharedResources> sharedLocalizer) =>
    [
        new Radio(sharedLocalizer["Yes"], Core.Models.YesNoNotSure.Yes.ToString()),
        new Radio(sharedLocalizer["No"], Core.Models.YesNoNotSure.No.ToString()),
        new Radio(sharedLocalizer["NotSure"], Core.Models.YesNoNotSure.NotSure.ToString())
    ];

    public static IRadio[] HomeNations(IStringLocalizer<SharedResources> sharedLocalizer) =>
    [
        new Radio(sharedLocalizer["England"], Nation.England.ToString()),
        new Radio(sharedLocalizer["Scotland"], Nation.Scotland.ToString()),
        new Radio(sharedLocalizer["Wales"], Nation.Wales.ToString()),
        new Radio(sharedLocalizer["NorthernIreland"], Nation.NorthernIreland.ToString())
    ];
}