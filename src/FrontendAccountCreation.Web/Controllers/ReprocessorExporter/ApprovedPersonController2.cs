using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

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
    public async Task<IActionResult> TeamMemberRoleInOrganisationEdit([FromQuery] Guid id)
    {
        SetFocusId(id);
        return RedirectToAction(nameof(ApprovedPersonController.TeamMemberRoleInOrganisation));
    }

    [HttpGet]
    [Route(PagePath.TeamMemberDetails + "/Edit")]
        public async Task<IActionResult> TeamMemberDetailsEdit([FromQuery] Guid id)
    {
        SetFocusId(id);

        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        // for navigation purposes, force team member details page to be after team member role
        return await SaveSessionAndRedirect(session, nameof(ApprovedPersonController.TeamMemberDetails), PagePath.TeamMemberRoleInOrganisation, PagePath.TeamMemberDetails);
    }

    [HttpGet]
    [Route(PagePath.TeamMembersCheckInvitationDetails + "/Delete")]
    public async Task<IActionResult> TeamMembersCheckInvitationDetailsDelete([FromQuery] Guid id)
    {
        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        session?.ReExCompaniesHouseSession?.TeamMembers?.RemoveAll(x => x.Id == id);

        return await SaveSessionAndRedirect(session, nameof(TeamMembersCheckInvitationDetails),
            PagePath.TeamMembersCheckInvitationDetails, null);
    }
}
