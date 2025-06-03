using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter;

[ExcludeFromCodeCoverage(Justification = "Code needed for other developers, will write unit tests after merge")]
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
        return RedirectToAction(nameof(ApprovedPersonController.TeamMemberDetails));
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

    [HttpGet]
    [Route(PagePath.MemberPartnership + "/Add")]
    public async Task<IActionResult> MemberPartnershipAdd()
    {
        DeleteFocusId();
        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        AddPageToWhiteList(session, PagePath.MemberPartnership);
        await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
        return RedirectToAction(nameof(ApprovedPersonController.MemberPartnership));
    }
}