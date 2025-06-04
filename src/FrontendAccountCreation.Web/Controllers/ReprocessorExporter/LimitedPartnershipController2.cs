using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter;

public partial class LimitedPartnershipController
{
    [HttpGet]
    [Route(PagePath.LimitedPartnershipNamesOfPartners + "/Delete")]
    public async Task<IActionResult> NamesOfPartnersDelete([FromQuery] Guid id)
    {
        DeleteFocusId();

        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        session?.ReExCompaniesHouseSession?.Partnership?.LimitedPartnership?.Partners?.RemoveAll(x => x.Id == id);

        return await SaveSessionAndRedirect(session, nameof(NamesOfPartners),
            PagePath.LimitedPartnershipNamesOfPartners, null);
    }

    [HttpGet]
    [Route(PagePath.LimitedPartnershipCheckNamesOfPartners + "/Delete")]
    public async Task<IActionResult> CheckNamesOfPartnersDelete([FromQuery] Guid id)
    {
        DeleteFocusId();

        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        session?.ReExCompaniesHouseSession?.Partnership?.LimitedPartnership?.Partners?.RemoveAll(x => x.Id == id);

        return await SaveSessionAndRedirect(session, nameof(CheckNamesOfPartners),
            PagePath.LimitedPartnershipCheckNamesOfPartners, null);
    }
}