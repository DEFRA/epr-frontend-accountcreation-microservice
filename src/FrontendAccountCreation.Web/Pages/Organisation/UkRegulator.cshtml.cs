using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ErrorNext;
using FrontendAccountCreation.Web.FullPages.Radios;
using FrontendAccountCreation.Web.FullPages.Radios.Common;
using FrontendAccountCreation.Web.Sessions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using FrontendAccountCreation.Core.Sessions;

namespace FrontendAccountCreation.Web.Pages.Organisation;

public class UkRegulator : OrganisationPageModel<UkRegulator>, IRadiosPageModel
{
    public UkRegulator(
        ISessionManager<OrganisationSession> sessionManager,
        IStringLocalizer<SharedResources> sharedLocalizer,
        IStringLocalizer<UkRegulator> localizer)
        : base(sessionManager, sharedLocalizer, localizer)
    {
    }

    public IEnumerable<IRadio> Radios => CommonRadios.HomeNations(SharedLocalizer);

    [BindProperty]
    [Required(ErrorMessage = "UkRegulator.ErrorMessage")]
    public string? SelectedValue { get; set; }

    public IErrorState Errors { get; set; } = ErrorStateEmpty.Instance;

    public string? Legend => Localizer["UkRegulator.NonUkOrganisation.Question"];

    public string? Hint => Localizer["UkRegulator.NonUkHint"];

    public async Task<IActionResult> OnGet()
    {
        var session = await SessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.UkRegulator);

        if (session?.ReExManualInputSession?.UkRegulatorNation != null)
        {
            SelectedValue = session.ReExManualInputSession.UkRegulatorNation.ToString();
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var session = await SessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.UkRegulator);

            Errors = ErrorStateFromModelState.Create(ModelState);
            return Page();
        }

        session.ReExManualInputSession ??= new ReExManualInputSession();
        if (SelectedValue != null)
        {
            //todo: throw if parse fails
             Enum.TryParse<Nation>(SelectedValue, out var nation);
             session.ReExManualInputSession.UkRegulatorNation = nation;
        }

        return await SaveSessionAndRedirect(
            session,
            nameof(OrganisationController),
            nameof(OrganisationController.NonUkRoleInOrganisation),
            PagePath.UkRegulator,
            PagePath.NonUkRoleInOrganisation);
    }
}