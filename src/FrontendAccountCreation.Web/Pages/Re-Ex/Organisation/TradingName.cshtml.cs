using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ErrorNext;
using FrontendAccountCreation.Web.FullPages.SingleTextBox;
using FrontendAccountCreation.Web.Sessions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.Pages.Re_Ex.Organisation;

//todo: going back from address overseas goes to is trading name different
//todo: check original stories for what the content should be

[Feature(FeatureFlags.AddOrganisationCompanyHouseDirectorJourney)]
[OrganisationJourneyAccess(PagePath.TradingName)]
public class TradingName(
    ISessionManager<OrganisationSession> sessionManager,
    IStringLocalizer<SharedResources> sharedLocalizer,
    IStringLocalizer<TradingName> localizer)
    : OrganisationPageModel<TradingName>(sessionManager, sharedLocalizer, localizer), ISingleTextboxPageModel
{
    public string? Heading => null;

    public string TextBoxLabel { get; private set; } = null!;

    //todo: better to use this than have an error message (or have both?)
    public int? MaxLength => 170;
    public IErrorState Errors { get; set; } = ErrorStateEmpty.Instance;

    [BindProperty]
    [MaxLength(170, ErrorMessage = "TradingName.LengthErrorMessage")]
    [Required(ErrorMessage = "TradingName.ErrorMessage")]
    public string? TextBoxValue { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var session = await SessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.TradingName);

        TextBoxValue = session.TradingName;

        SetQuestion(session);

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var session = await SessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.TradingName);
            SetQuestion(session);

            return Page();
        }

        session.TradingName = TextBoxValue;

        string actionName, nextPage;
        if (session.IsCompaniesHouseFlow)
        {
            actionName = nameof(OrganisationController.IsOrganisationAPartner);
            nextPage = PagePath.IsPartnership;
        }
        else if (session.ReExManualInputSession?.ProducerType is ProducerType.NonUkOrganisation)
        {
            actionName = nameof(OrganisationController.AddressOverseas);
            nextPage = PagePath.AddressOverseas;
        }
        else
        {
            actionName = nameof(OrganisationController.TypeOfOrganisation);
            nextPage = PagePath.TypeOfOrganisation;
        }

        return await SaveSessionAndRedirect(
            session,
            nameof(OrganisationController),
            actionName,
            PagePath.TradingName,
            nextPage);
    }

    private void SetQuestion(OrganisationSession session)
    {
        TextBoxLabel = session.IsCompaniesHouseFlow
            ? Localizer["TradingName.Question"]
            : Localizer["TradingName.NonCompaniesHouse.Question"];
    }
}