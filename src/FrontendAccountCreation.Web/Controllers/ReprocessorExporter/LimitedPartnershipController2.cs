using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter;

public partial class LimitedPartnershipController
{
    [HttpGet]
    [Route(PagePath.LimitedPartnershipNamesOfPartners + "/Delete")]
    [OrganisationJourneyAccess(PagePath.LimitedPartnershipNamesOfPartners)]
    public async Task<IActionResult> NamesOfPartnersDelete([FromQuery] Guid id)
    {
        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        session?.ReExCompaniesHouseSession?.Partnership?.LimitedPartnership?.Partners?.RemoveAll(x => x.Id == id);

        return await SaveSessionAndRedirect(session, nameof(NamesOfPartners),
            PagePath.LimitedPartnershipNamesOfPartners, null);
    }

    [HttpGet]
    [Route(PagePath.LimitedPartnershipCheckNamesOfPartners + "/Delete")]
    [OrganisationJourneyAccess(PagePath.LimitedPartnershipCheckNamesOfPartners)]
    public async Task<IActionResult> CheckNamesOfPartnersDelete([FromQuery] Guid id)
    {
        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        session?.ReExCompaniesHouseSession?.Partnership?.LimitedPartnership?.Partners?.RemoveAll(x => x.Id == id);

        return await SaveSessionAndRedirect(session, nameof(CheckNamesOfPartners),
            PagePath.LimitedPartnershipCheckNamesOfPartners, null);
    }
}
