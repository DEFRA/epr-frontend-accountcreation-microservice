using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Sessions;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
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

        var answer = viewModel.ManageControlInUKAnswer!.Value;
        session.ReExUnincorporatedFlowSession.ManageControlAnswer = answer;

        if (viewModel.ManageControlInUKAnswer.GetValueOrDefault(ManageControlAnswer.NotSure) == ManageControlAnswer.Yes)
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
        return View(new ReExManageAccountPersonViewModel{ ManageAccountPersonAnswer = session.ReExUnincorporatedFlowSession.ManageAccountPersonAnswer });
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

        session.ReExUnincorporatedFlowSession.ManageAccountPersonAnswer = viewModel.ManageAccountPersonAnswer.GetValueOrDefault();

        return await SaveSessionAndRedirect(
            session,
            nameof(ManageControl),
            PagePath.UnincorporatedManageControl,
            PagePath.UnincorporatedManageAccountPerson);
    }

    [HttpGet]
    [Route(PagePath.UnincorporatedApprovedPerson)]
    public async Task<IActionResult> ApprovedPerson()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.UnincorporatedApprovedPerson);

        return View();
    }

    [HttpPost]
    [Route(PagePath.UnincorporatedApprovedPerson)]
    public async Task<IActionResult> ApprovedPerson(bool inviteApprovedPerson)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.UnincorporatedApprovedPerson);
            return View();
        }

        return inviteApprovedPerson
            ? // TODO: invite user path - Redirect to page 5 when implemented
            await SaveSessionAndRedirect(session, nameof(ApprovedPerson), PagePath.UnincorporatedApprovedPerson, PagePath.UnincorporatedManageAccountPerson)
            : // TODO: continue path - Redirect to Check your details - final page
            await SaveSessionAndRedirect(session, nameof(ManageAccountPerson), PagePath.UnincorporatedApprovedPerson, PagePath.UnincorporatedManageAccountPerson);
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

        session.ReExUnincorporatedFlowSession.ManageAccountPersonAnswer = viewModel.ManageAccountPersonAnswer!.Value;

        if (viewModel.ManageAccountPersonAnswer == ManageAccountPersonAnswer.IWillInviteATeamMemberToBeApprovedPersonInstead)
        {
            return await SaveSessionAndRedirect(
                session,
                nameof(ManageControl),
                PagePath.UnincorporatedManageControl,
                PagePath.UnincorporatedManageAccountPersonUserFromTeam);
        }

        //TODO Check Your Details
        return await SaveSessionAndRedirect(
            session,
            nameof(ManageAccountPersonUserFromTeam),
            PagePath.UnincorporatedNotImplemented,
            PagePath.UnincorporatedManageAccountPersonUserFromTeam);
    }
}