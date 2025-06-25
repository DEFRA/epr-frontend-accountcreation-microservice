using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Sessions;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
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
        return View(new ReExRoleInOrganisationViewModel { Role = session.ReExUnincorporatedFlowSession.RoleInOrganisation });
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

        session.ReExUnincorporatedFlowSession.RoleInOrganisation = viewModel.Role;

        return await SaveSessionAndRedirect(
            session,
            nameof(ManageControl),
            PagePath.UnincorporatedRoleInOrganisation,
            PagePath.UnincorporatedManageControl);
    }

    [HttpGet]
    [Route(PagePath.UnincorporatedManageControl)]
    public async Task<IActionResult> ManageControl()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.UnincorporatedManageControl);

        return View(new ReExManageControlViewModel { ManageControlInUKAnswer = session.ReExUnincorporatedFlowSession.ManageControlAnswer });
    }

    [HttpPost]
    [Route(PagePath.UnincorporatedManageControl)]
    public async Task<IActionResult> ManageControl(ReExManageControlViewModel viewModel)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.UnincorporatedManageControl);
            return View(viewModel);
        }

        session.ReExUnincorporatedFlowSession.ManageControlAnswer = viewModel.ManageControlInUKAnswer.Value;

        if (viewModel.ManageControlInUKAnswer.GetValueOrDefault(ManageControlAnswer.Yes) == ManageControlAnswer.Yes)
        {
            return await SaveSessionAndRedirect(session, nameof(ManageAccountPerson), PagePath.UnincorporatedManageControl, PagePath.UnincorporatedManageAccountPerson);
        }
        else
        {
            return await SaveSessionAndRedirect(session, nameof(ManageAccountPersonUserFromTeam), PagePath.UnincorporatedManageControl, PagePath.UnincorporatedManageAccountPersonUserFromTeam);
        }
    }


    [HttpGet]
    [Route(PagePath.UnincorporatedManageAccountPerson)]
    public async Task<IActionResult> ManageAccountPerson()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.UnincorporatedManageAccountPerson);
        return View(new ReExManageAccountPersonViewModel());
    }

    [HttpPost]
    [Route(PagePath.UnincorporatedManageAccountPerson)]
    public async Task<IActionResult> ManageAccountPerson(ReExManageAccountPersonViewModel viewModel)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.UnincorporatedManageAccountPerson);
            return View(viewModel);
        }

        var answer = viewModel.ManageAccountPersonAnswer.GetValueOrDefault();
        session.ReExUnincorporatedFlowSession.ManageAccountPersonAnswer = answer;

        return await SaveSessionAndRedirect(
            session,
            nameof(ManageControl),
            PagePath.UnincorporatedManageControl,
            PagePath.UnincorporatedManageAccountPerson);
    }

    [HttpGet]
    [Route(PagePath.UnincorporatedManageAccountPersonUserFromTeam)]
    public async Task<IActionResult> ManageAccountPersonUserFromTeam()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.UnincorporatedManageAccountPersonUserFromTeam);
        return View(new ReExManageAccountPersonUserFromTeamViewModel());
    }

    [HttpPost]
    [Route(PagePath.UnincorporatedManageAccountPersonUserFromTeam)]
    public async Task<IActionResult> ManageAccountPersonUserFromTeam(ReExManageAccountPersonUserFromTeamViewModel viewModel)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.UnincorporatedManageAccountPersonUserFromTeam);
            return View(viewModel);
        }

        var answer = viewModel.ManageAccountPersonAnswer!.Value;
        session.ReExUnincorporatedFlowSession.ManageAccountPersonAnswer = answer;

        var (action, returnToPage) = answer == ManageAccountPersonAnswer.IWillInviteATeamMemberToBeApprovedPersonInstead
            ? (nameof(ManageControl), PagePath.UnincorporatedManageControl)
            : (nameof(UnincorporatedCheckYourDetails), PagePath.UnincorporatedCheckYourDetails);

        return await SaveSessionAndRedirect(session, action, returnToPage, PagePath.UnincorporatedManageAccountPersonUserFromTeam);
    }


    [HttpGet]
    [Route(PagePath.UnincorporatedCheckYourDetails)]
    public async Task<IActionResult> UnincorporatedCheckYourDetails()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.UnincorporatedCheckYourDetails);
        return View(new ReExCheckYourDetailsViewModel());
    }
}