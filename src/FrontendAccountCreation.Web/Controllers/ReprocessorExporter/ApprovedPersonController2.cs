using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter;

public partial class ApprovedPersonController
{
    [HttpGet]
    [Route(PagePath.TeamMemberRoleInOrganisationAdd)]
    [OrganisationJourneyAccess(PagePath.TeamMemberRoleInOrganisation)]
    public async Task<IActionResult> TeamMemberRoleInOrganisationAdd()
    {
        DeleteFocusId();

        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        return await SaveSessionAndRedirect(session, nameof(ApprovedPersonController.TeamMemberRoleInOrganisation),
            PagePath.TeamMemberRoleInOrganisation, null);
    }

    [HttpGet]
    [Route(PagePath.TeamMemberRoleInOrganisationEdit)]
    public async Task<IActionResult> TeamMemberRoleInOrganisationEdit([FromQuery] Guid id)
    {
        SetFocusId(id);

        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        return await SaveSessionAndRedirect(session, nameof(ApprovedPersonController.TeamMemberRoleInOrganisation),
            PagePath.TeamMemberRoleInOrganisation, null);
    }

    [HttpGet]
    [Route(PagePath.TeamMemberDetailsEdit)]
    public async Task<IActionResult> TeamMemberDetailsEdit([FromQuery] Guid id)
    {
        SetFocusId(id);

        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        return await SaveSessionAndRedirect(session, nameof(ApprovedPersonController.TeamMemberDetails),
            PagePath.TeamMemberDetails, null);
    }

    [HttpGet]
    [Route(PagePath.TeamMembersCheckInvitationDetailsDelete)]
    public async Task<IActionResult> TeamMembersCheckInvitationDetailsDelete([FromQuery] Guid id)
    {
        DeleteFocusId();

        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        session?.ReExCompaniesHouseSession?.TeamMembers?.RemoveAll(x => x.Id == id);

        return await SaveSessionAndRedirect(session, nameof(TeamMembersCheckInvitationDetails),
            PagePath.TeamMembersCheckInvitationDetails, null);
    }

    [HttpGet]
    [Route(PagePath.MemberPartnershipAdd)]
    public async Task<IActionResult> MemberPartnershipAdd()
    {
        DeleteFocusId();

        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        return await SaveSessionAndRedirect(session, nameof(MemberPartnership),
            PagePath.YouAreApprovedPerson, PagePath.MemberPartnership);
    }
}