using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.ErrorNext;
using FrontendAccountCreation.Web.FullPages.SingleTextBox;
using FrontendAccountCreation.Web.Sessions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FrontendAccountCreation.Web.Pages.Re_Ex.Organisation;

[Feature(FeatureFlags.AddOrganisationCompanyHouseDirectorJourney)]
[OrganisationJourneyAccess(PagePath.TradingName)]
public class TradingName(
    ISessionManager<OrganisationSession> sessionManager,
    IStringLocalizer<SharedResources> sharedLocalizer,
    IStringLocalizer<TradingName> localizer)
    : OrganisationPageModel<TradingName>(sessionManager, sharedLocalizer, localizer), ISingleTextboxPageModel
{
    public string? Heading { get; private set; }
    public string? Hint => default;
    public string TextBoxLabel => "What is the service name?";
    public int? MaxLength => 10;
    public IErrorState Errors { get; set; } = ErrorStateEmpty.Instance;

    [BindProperty]
    public string? TextBoxValue { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var session = await SessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.TradingName);

        TextBoxValue = session.TradingName;

        if (!session.IsCompaniesHouseFlow)
        {
            Heading = Localizer["TradingName.NonCompaniesHouse.Question"];
        }

        return Page();
    }

    public void OnPost()
    {
    }
}