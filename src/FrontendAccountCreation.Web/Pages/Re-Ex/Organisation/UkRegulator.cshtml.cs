using System.ComponentModel.DataAnnotations;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ErrorNext;
using FrontendAccountCreation.Web.FullPages.Radios;
using FrontendAccountCreation.Web.FullPages.Radios.Common;
using FrontendAccountCreation.Web.Sessions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FrontendAccountCreation.Web.Pages.Re_Ex.Organisation;

[Feature(FeatureFlags.AddOrganisationCompanyHouseDirectorJourney)]
[OrganisationJourneyAccess(PagePath.UkRegulator)]
public class UkRegulator(
    ISessionManager<OrganisationSession> sessionManager,
    IStringLocalizer<SharedResources> sharedLocalizer,
    IStringLocalizer<UkRegulator> localizer)
    : OrganisationPageModel<UkRegulator>(PagePath.UkRegulator, sessionManager, sharedLocalizer, localizer),
        IRadiosPageModel
{
    public IEnumerable<IRadio> Radios => CommonRadios.HomeNations(SharedLocalizer);

    [BindProperty]
    [Required(ErrorMessage = "UkRegulator.ErrorMessage")]
    public string? SelectedValue { get; set; }

    public IErrorState Errors { get; set; } = ErrorStateEmpty.Instance;

    public async Task<IActionResult> OnGet()
    {
        var session = await SetupPage();

        SelectedValue = session!.ReExManualInputSession?.UkRegulatorNation?.ToString();

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var session = await SetupPage();

        if (!ModelState.IsValid)
        {
            Errors = ErrorStateFromModelState.Create(ModelState);
            return Page();
        }

        session.ReExManualInputSession ??= new ReExManualInputSession();
        session.ReExManualInputSession.UkRegulatorNation = Enum.Parse<Nation>(SelectedValue!);

        return await SaveSessionAndRedirect(
            session,
            nameof(OrganisationController),
            nameof(OrganisationController.NonUkRoleInOrganisation),
            PagePath.NonUkRoleInOrganisation);
    }
}