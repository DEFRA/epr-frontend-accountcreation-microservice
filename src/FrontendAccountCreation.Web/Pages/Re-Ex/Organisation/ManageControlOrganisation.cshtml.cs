using System.ComponentModel.DataAnnotations;
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
[OrganisationJourneyAccess(PagePath.ManageControlOrganisation)]
public class ManageControlOrganisation(
    ISessionManager<OrganisationSession> sessionManager,
    IStringLocalizer<SharedResources> sharedLocalizer,
    IStringLocalizer<ManageControlOrganisation> localizer)
    : OrganisationPageModel<ManageControlOrganisation>(sessionManager, sharedLocalizer, localizer), IRadiosPageModel
{
    public IEnumerable<IRadio> Radios => CommonRadios.YesNoNotSure_AreThey(SharedLocalizer);

    [BindProperty]
    [Required(ErrorMessage = "ManageControlOrganisation.ErrorMessage")]
    public string? SelectedValue { get; set; }

    public IErrorState Errors { get; set; } = ErrorStateEmpty.Instance;

    public async Task<IActionResult> OnGet(bool invitePerson = false)
    {
        var session = await SessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.ManageControlOrganisation);

        if (!invitePerson)
        {
            SelectedValue = session.TheyManageOrControlOrganisation?.ToString();
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var session = await SessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.ManageControlOrganisation);

            Errors = ErrorStateFromModelState.Create(ModelState);
            return Page();
        }

        session!.TheyManageOrControlOrganisation =
            Enum.Parse<Core.Models.YesNoNotSure>(SelectedValue!);

        if (session.TheyManageOrControlOrganisation is Core.Models.YesNoNotSure.Yes)
        {
            return await SaveSessionAndRedirect(
                session,
                nameof(ApprovedPersonController),
                nameof(ApprovedPersonController.NonCompaniesHouseTeamMemberDetails),
                PagePath.ManageControlOrganisation,
                PagePath.NonCompaniesHouseTeamMemberDetails);
        }

        return await SaveSessionAndRedirect(
            session,
            nameof(ApprovedPersonController),
            nameof(ApprovedPersonController.PersonCanNotBeInvited),
            PagePath.ManageControlOrganisation,
            PagePath.ApprovedPersonPartnershipCanNotBeInvited);
    }
}