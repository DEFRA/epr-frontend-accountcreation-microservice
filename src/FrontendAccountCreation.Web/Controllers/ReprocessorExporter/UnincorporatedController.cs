using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Sessions;
using FrontendAccountCreation.Web.ViewModels.ReExAccount.Unincorporated;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter;

[Feature(FeatureFlags.AddOrganisationCompanyHouseDirectorJourney)]
[Route("re-ex/organisation")]
public class UnincorporatedController : ControllerBase<OrganisationSession>
{
    private readonly ISessionManager<OrganisationSession> _sessionManager;

    public UnincorporatedController(ISessionManager<OrganisationSession> sessionManager) : base(sessionManager)
    {
        _sessionManager = sessionManager;
    }

    [HttpGet]
    [Route(PagePath.UnincorporatedRoleInOrganisation)]
    public async Task<IActionResult> RoleInOrganisation()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.UnincorporatedRoleInOrganisation);
        return View(new ReExRoleInOrganisationViewModel { Role = session.ReExManualInputSession.RoleInUnincorporatedOrganisation });
    }


    [HttpPost]
    [Route(PagePath.UnincorporatedRoleInOrganisation)]
    public async Task<IActionResult> RoleInOrganisation(ReExRoleInOrganisationViewModel viewModel)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.UnincorporatedRoleInOrganisation);
            return View(viewModel);
        }
        session.ReExManualInputSession ??= new ReExManualInputSession();
        session.ReExManualInputSession.RoleInUnincorporatedOrganisation = viewModel.Role;

        return await SaveSessionAndRedirect(session,
            controllerName: nameof(OrganisationController),
            actionName: nameof(OrganisationController.ManageControl),
            currentPagePath: PagePath.UnincorporatedRoleInOrganisation,
            nextPagePath: PagePath.ManageControl);
    }
}