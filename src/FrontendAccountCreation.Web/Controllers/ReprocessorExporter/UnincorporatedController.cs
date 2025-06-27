using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Sessions;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using FrontendAccountCreation.Web.ViewModels.ReExAccount.Unincorporated;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

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
            return await SaveSessionAndRedirect(
                session,
                nameof(ManageAccountPerson),
                PagePath.UnincorporatedManageControl,
                PagePath.UnincorporatedManageAccountPerson);
        }
        else
        {
            return await SaveSessionAndRedirect(
                session,
                nameof(ManageAccountPersonUserFromTeam),
                PagePath.UnincorporatedManageControl,
                PagePath.UnincorporatedManageAccountPersonUserFromTeam);
        }
    }

    [HttpGet]
    [Route(PagePath.UnincorporatedManageAccountPerson)]
    public async Task<IActionResult> ManageAccountPerson()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.UnincorporatedManageAccountPerson);
        return View(new ReExManageAccountPersonViewModel { ManageAccountPersonAnswer = session.ReExUnincorporatedFlowSession.ManageAccountPersonAnswer });
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

        if (answer == ManageAccountPersonAnswer.IAgreeToBeAnApprovedPerson)
        {
            return await SaveSessionAndRedirect(
                session,
                nameof(ApprovedPerson),
                PagePath.UnincorporatedManageAccountPerson,
                PagePath.UnincorporatedApprovedPerson);
        }
        else if (answer == ManageAccountPersonAnswer.IWillInviteATeamMemberToBeApprovedPersonInstead)
        {
            return await SaveSessionAndRedirect(
                session,
                nameof(ManageControlOrganisation),
                PagePath.UnincorporatedManageAccountPerson,
                PagePath.UnincorporatedManageControlOrganisation);
        }

        return await SaveSessionAndRedirect(
            session,
            nameof(CheckYourDetails),
            PagePath.UnincorporatedManageAccountPerson,
            PagePath.UnincorporatedCheckYourDetails);
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

        if (inviteApprovedPerson)
        {
            return await SaveSessionAndRedirect(
                session,
                nameof(ManageControlOrganisation),
                PagePath.UnincorporatedApprovedPerson,
                PagePath.UnincorporatedManageControlOrganisation);
        }

        return await SaveSessionAndRedirect(
            session,
            nameof(CheckYourDetails),
            PagePath.UnincorporatedApprovedPerson,
            PagePath.UnincorporatedCheckYourDetails);
    }

    [HttpGet]
    [Route(PagePath.UnincorporatedManageAccountPersonUserFromTeam)]
    public async Task<IActionResult> ManageAccountPersonUserFromTeam()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.UnincorporatedManageAccountPersonUserFromTeam);
        return View(new ReExManageAccountPersonUserFromTeamViewModel
        {
            ManageAccountPersonAnswer = session.ReExUnincorporatedFlowSession.ManageAccountPersonAnswer
        });
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
                nameof(ManageControlOrganisation),
                PagePath.UnincorporatedManageAccountPersonUserFromTeam,
                PagePath.UnincorporatedManageControlOrganisation);
        }

        return await SaveSessionAndRedirect(
            session,
            nameof(CheckYourDetails),
            PagePath.UnincorporatedManageAccountPersonUserFromTeam,
            PagePath.UnincorporatedCheckYourDetails);
    }

    [HttpGet]
    [Route(PagePath.UnincorporatedManageControlOrganisation)]
    public async Task<IActionResult> ManageControlOrganisation()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.UnincorporatedManageControlOrganisation);
        return View(new ReExManageControlOrganisationViewModel
        {
            Answer = session.ReExUnincorporatedFlowSession.ManageControlOrganisationAnswer
        });
    }

    [HttpPost]
    [Route(PagePath.UnincorporatedManageControlOrganisation)]
    public async Task<IActionResult> ManageControlOrganisation(ReExManageControlOrganisationViewModel viewModel)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.UnincorporatedManageControlOrganisation);
            return View(viewModel);
        }

        session.ReExUnincorporatedFlowSession.ManageControlOrganisationAnswer = viewModel.Answer.Value;

        if (viewModel.Answer == ManageControlAnswer.Yes)
        {
            return await SaveSessionAndRedirect(
                session,
                nameof(TeamMemberDetails),
                PagePath.UnincorporatedManageControlOrganisation,
                PagePath.UnincorporatedTeamMemberDetails);
        }

        return await SaveSessionAndRedirect(
            session,
            nameof(ApprovedPersonCannotBeInvited),
            PagePath.UnincorporatedManageControlOrganisation,
            PagePath.UnincorporatedApprovedPersonCannotBeInvited);
    }

    [HttpGet]
    [Route(PagePath.UnincorporatedApprovedPersonCannotBeInvited)]
    [ExcludeFromCodeCoverage]
    public async Task<IActionResult> ApprovedPersonCannotBeInvited()
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route(PagePath.UnincorporatedTeamMemberDetails)]
    [ExcludeFromCodeCoverage]
    public async Task<IActionResult> TeamMemberDetails()
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route(PagePath.UnincorporatedCheckInvitation)]
    [ExcludeFromCodeCoverage]
    public async Task<IActionResult> CheckInvitation()
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route(PagePath.UnincorporatedCheckYourDetails)]
    [ExcludeFromCodeCoverage]
    public async Task<IActionResult> CheckYourDetails()
    {
        throw new NotImplementedException();
    }
}