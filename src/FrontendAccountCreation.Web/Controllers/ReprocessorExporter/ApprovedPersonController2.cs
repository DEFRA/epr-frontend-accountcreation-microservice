using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter;

public partial class ApprovedPersonController
{
    [HttpGet]
    [Route(PagePath.TeamMemberRoleInOrganisation + "/Add")]
    [OrganisationJourneyAccess(PagePath.TeamMemberRoleInOrganisation)]
    public async Task<IActionResult> TeamMemberRoleInOrganisationAdd()
    {
        DeleteFocusId();
        return RedirectToAction(nameof(ApprovedPersonController.TeamMemberRoleInOrganisation));
    }

    [HttpGet]
    [Route(PagePath.TeamMemberRoleInOrganisation + "/Edit")]
    [OrganisationJourneyAccess(PagePath.TeamMemberRoleInOrganisation)]
    public async Task<IActionResult> TeamMemberRoleInOrganisationEdit([FromQuery] Guid id)
    {
        SetFocusId(id);
        return RedirectToAction(nameof(ApprovedPersonController.TeamMemberRoleInOrganisation));
    }

    [HttpGet]
    [Route(PagePath.TeamMemberDetails + "/Edit")]
    [OrganisationJourneyAccess(PagePath.TeamMemberDetails)]
    public async Task<IActionResult> TeamMemberDetailsEdit([FromQuery] Guid id)
    {
        SetFocusId(id);
        return RedirectToAction(nameof(ApprovedPersonController.TeamMemberDetails));
    }

    [HttpGet]
    [Route(PagePath.TeamMembersCheckInvitationDetails + "/Delete")]
    [OrganisationJourneyAccess(PagePath.TeamMembersCheckInvitationDetails)]
    public async Task<IActionResult> TeamMembersCheckInvitationDetailsDelete([FromQuery] Guid id)
    {
        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        session?.ReExCompaniesHouseSession?.TeamMembers?.RemoveAll(x => x.Id == id);

        return await SaveSessionAndRedirect(session, nameof(TeamMembersCheckInvitationDetails),
            PagePath.TeamMembersCheckInvitationDetails, null);
    }
}
