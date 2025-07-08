using System.Diagnostics.CodeAnalysis;
using FrontendAccountCreation.Core.Sessions;
using Microsoft.Extensions.Localization;

namespace FrontendAccountCreation.Web.FullPages.Radios.Common;

/// <summary>
/// Common radio button options used across various pages.
/// </summary>
/// <remarks>
/// Yes and No do not translate well into Welsh, as the Welsh language has different forms for "yes" and "no".
///
/// Pick the correct form based on the context of the question.
///
/// _AreThey
/// --------
/// Use the _AreThey variant for Welsh "yes/no" (Ydyn/Nac ydyn) responses.
/// This form is required when the question refers to "they" (third-person plural).
/// Examples of questions that use this form:
///   - "Are they managing or controlling the organisation?"
///   - "Do they have access to the system?"
///   - "Are they eligible for approval?"
/// In Welsh, the response to these questions is "Ydyn" (yes) or "Nac ydyn" (no),
/// which differs from other forms like "Ydy/Nac ydy" (is he/she/it) or "Ydw/Nac ydw" (am I).
///
/// _IsHeSheIt
/// ----------
/// Use the _IsHeSheIt variant for Welsh "yes/no" (Ydy/Nac ydy) responses.
/// This form is required when the question refers to "he", "she", or "it" (third-person singular).
/// Examples of questions that use this form:
///   - "Is he a director?"
///   - "Is she responsible for compliance?"
///   - "Is it registered as a charity?"
/// In Welsh, the response to these questions is "Ydy" (yes) or "Nac ydy" (no).
///
/// This helps ensure the correct grammatical form is used for Welsh translations.
/// </remarks>
[ExcludeFromCodeCoverage]
public static class CommonRadios
{
    //to-do: optimise these, so don't create new objects each time
    public static IRadio[] YesNo_IsHeSheIt(IStringLocalizer<SharedResources> sharedLocalizer) =>
    [
        new Radio(sharedLocalizer["Yes_IsHeSheIt"], true.ToString()),
        new Radio(sharedLocalizer["No_IsHeSheIt"], false.ToString())
    ];

#if not_needed_yet
    public static IRadio[] YesNoNotSure_IsHeSheIt(IStringLocalizer<SharedResources> sharedLocalizer) =>
    [
        new Radio(sharedLocalizer["Yes"], Core.Models.YesNoNotSure.Yes.ToString()),
        new Radio(sharedLocalizer["No"], Core.Models.YesNoNotSure.No.ToString()),
        new Radio(sharedLocalizer["NotSure"], Core.Models.YesNoNotSure.NotSure.ToString())
    ];
#endif

    public static IRadio[] YesNoNotSure_AreThey(IStringLocalizer<SharedResources> sharedLocalizer) =>
    [
        new Radio(sharedLocalizer["Yes_AreThey"], Core.Models.YesNoNotSure.Yes.ToString()),
        new Radio(sharedLocalizer["No_AreThey"], Core.Models.YesNoNotSure.No.ToString()),
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