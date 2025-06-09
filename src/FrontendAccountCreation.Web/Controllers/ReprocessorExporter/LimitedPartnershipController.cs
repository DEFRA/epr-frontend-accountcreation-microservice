using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Core.Sessions.ReEx.Partnership;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Sessions;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter;

[Feature(FeatureFlags.AddOrganisationCompanyHouseDirectorJourney)]
[Route("re-ex/organisation")]
public partial class LimitedPartnershipController : ControllerBase<OrganisationSession>
{
    private readonly ISessionManager<OrganisationSession> _sessionManager;

    public LimitedPartnershipController(ISessionManager<OrganisationSession> sessionManager) : base(sessionManager)
    {
        _sessionManager = sessionManager;
    }

    [HttpGet]
    [Route(PagePath.LimitedPartnershipNamesOfPartners)]
    [OrganisationJourneyAccess(PagePath.LimitedPartnershipNamesOfPartners)]
    public async Task<IActionResult> NamesOfPartners()
    {
        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.LimitedPartnershipNamesOfPartners);

        LimitedPartnershipPartnersViewModel model = new();

        ReExLimitedPartnership ltdPartnershipSession = session?.ReExCompaniesHouseSession?.Partnership?.LimitedPartnership;
        model.ExpectsIndividualPartners = ltdPartnershipSession?.HasIndividualPartners ?? true;
        model.ExpectsCompanyPartners = ltdPartnershipSession?.HasCompanyPartners ?? true;

        List<ReExLimitedPartnershipPersonOrCompany>? partnersSession = ltdPartnershipSession?.Partners;
        List<LimitedPartnershipPersonOrCompanyViewModel> partnerList = [];
        if (partnersSession != null)
        {
            partnerList = partnersSession.Select(item => (LimitedPartnershipPersonOrCompanyViewModel)item)
                .Where(x => (
                        (!x.IsPersonOrCompanyButNotBoth) ||
                        (x.IsPerson && model.ExpectsIndividualPartners) ||
                        (x.IsCompany && model.ExpectsCompanyPartners)
                            )).ToList();
        }

        if (partnerList.Count.Equals(0))
        {
            LimitedPartnershipPersonOrCompanyViewModel newPartner = new()
            {
                Id = Guid.NewGuid()
            };
            partnerList.Add(newPartner);
        }

        model.Partners = partnerList;
        return View(model);
    }

    /// <summary>
    /// Save partner details to session
    /// </summary>
    /// <param name="model">View model</param>
    /// <param name="command">'save' to update partners and continue, 'add' to add new partner.</param>
    /// <returns></returns>
    [HttpPost]
    [Route(PagePath.LimitedPartnershipNamesOfPartners)]
    [OrganisationJourneyAccess(PagePath.LimitedPartnershipNamesOfPartners)]
    public async Task<IActionResult> NamesOfPartners(LimitedPartnershipPartnersViewModel model, string command)
    {
        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            ModelState.Clear();
            string errorMessage = OverrideValidationErrorMessage(model);
            ModelState.AddModelError(nameof(model.Partners), errorMessage);

            SetBackLink(session, PagePath.LimitedPartnershipNamesOfPartners);
            return View(model);
        }

        if (command == "add")
        {
            LimitedPartnershipPersonOrCompanyViewModel newPartner = new()
            {
                Id = Guid.NewGuid()
            };
            model.Partners.Add(newPartner);
            await SyncSessionWithModel(model.ExpectsCompanyPartners, model.ExpectsIndividualPartners, await GetSessionPartners(model.Partners));
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            SetBackLink(session, PagePath.LimitedPartnershipNamesOfPartners);
            return View(model);
        }

        await SyncSessionWithModel(model.ExpectsCompanyPartners, model.ExpectsIndividualPartners, await GetSessionPartners(model.Partners));

        return await SaveSessionAndRedirect(session, nameof(CheckNamesOfPartners),
            PagePath.LimitedPartnershipNamesOfPartners, PagePath.LimitedPartnershipCheckNamesOfPartners);

        async Task SyncSessionWithModel(
            bool hasCompanyPartners,
            bool hasIndividualPartners,
            List<ReExLimitedPartnershipPersonOrCompany> partners)
        {
            // Organisation > Company > Partnership > Limited Partnership
            ReExCompaniesHouseSession companySession = session.ReExCompaniesHouseSession;
            ReExPartnership partnershipSession = companySession.Partnership ?? new();

            // refresh limited partnership session from the view model
            ReExLimitedPartnership ltdPartnershipSession = new()
            {
                Partners = partners,
                HasCompanyPartners = hasCompanyPartners,
                HasIndividualPartners = hasIndividualPartners
            };

            partnershipSession.LimitedPartnership = ltdPartnershipSession;
            companySession.Partnership = partnershipSession;
            session.ReExCompaniesHouseSession = companySession;
        }

        static string OverrideValidationErrorMessage(LimitedPartnershipPartnersViewModel model)
        {
            string errorMessage = "ValidationError_Both";
            if (model.ExpectsCompanyPartners && model.ExpectsIndividualPartners)
            {
                errorMessage = "ValidationError_Both";
            }
            else if (model.ExpectsCompanyPartners)
            {
                errorMessage = "ValidationError_Company";
            }
            else if (model.ExpectsIndividualPartners)
            {
                errorMessage = "ValidationError_Individual";
            }

            return string.Concat("NamesOfPartners.", errorMessage);
        }
    }

    [HttpGet]
    [Route(PagePath.LimitedPartnershipNamesOfPartnersDelete)]
    public async Task<IActionResult> NamesOfPartnersDelete([FromQuery] Guid id)
    {
        DeleteFocusId();

        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        session?.ReExCompaniesHouseSession?.Partnership?.LimitedPartnership?.Partners?.RemoveAll(x => x.Id == id);

        return await SaveSessionAndRedirect(session, nameof(NamesOfPartners),
            PagePath.LimitedPartnershipNamesOfPartners, null);
    }

    [HttpGet]
    [Route(PagePath.LimitedPartnershipCheckNamesOfPartners)]
    [OrganisationJourneyAccess(PagePath.LimitedPartnershipCheckNamesOfPartners)]
    public async Task<IActionResult> CheckNamesOfPartners()
    {
        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.LimitedPartnershipCheckNamesOfPartners);

        // there is no validation on this page, so work directly on the session rather than a separate view model
        List<ReExLimitedPartnershipPersonOrCompany> model = session.ReExCompaniesHouseSession?.Partnership?.LimitedPartnership?.Partners ?? new();

        return View(model);
    }

    /// <summary>
    /// Alternatively one might just get the next page, but that would not update session navigation.
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [Route(PagePath.LimitedPartnershipCheckNamesOfPartners)]
    [OrganisationJourneyAccess(PagePath.LimitedPartnershipCheckNamesOfPartners)]
    public async Task<IActionResult> CheckNamesOfPartners(List<ReExLimitedPartnershipPersonOrCompany> modelNotUsed)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new OrganisationSession();
        return await SaveSessionAndRedirect(session, nameof(LimitedPartnershipController.LimitedPartnershipRole), PagePath.LimitedPartnershipCheckNamesOfPartners, PagePath.LimitedPartnershipRole);
    }

    [HttpGet]
    [Route(PagePath.LimitedPartnershipCheckNamesOfPartnersDelete)]
    public async Task<IActionResult> CheckNamesOfPartnersDelete([FromQuery] Guid id)
    {
        DeleteFocusId();

        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        session?.ReExCompaniesHouseSession?.Partnership?.LimitedPartnership?.Partners?.RemoveAll(x => x.Id == id);

        return await SaveSessionAndRedirect(session, nameof(CheckNamesOfPartners),
            PagePath.LimitedPartnershipCheckNamesOfPartners, null);
    }

    [HttpGet]
    [Route(PagePath.PartnershipType)]
    [OrganisationJourneyAccess(PagePath.PartnershipType)]
    public async Task<IActionResult> PartnershipType()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.PartnershipType);

        bool? isLimitedPartnership = session?.ReExCompaniesHouseSession?.Partnership?.IsLimitedPartnership;
        bool? isLimitedLiabilityPartnership = session?.ReExCompaniesHouseSession?.Partnership?.IsLimitedLiabilityPartnership;

        PartnershipTypeRequestViewModel model = new();
        if (isLimitedPartnership.GetValueOrDefault(false))
        {
            model.TypeOfPartnership = Core.Sessions.PartnershipType.LimitedPartnership;
        }
        else if (isLimitedLiabilityPartnership.GetValueOrDefault(false))
        {
            model.TypeOfPartnership = Core.Sessions.PartnershipType.LimitedLiabilityPartnership;
        }

        return View(model);
    }

    [HttpPost]
    [Route(PagePath.PartnershipType)]
    [OrganisationJourneyAccess(PagePath.PartnershipType)]
    public async Task<IActionResult> PartnershipType(PartnershipTypeRequestViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.PartnershipType);
            return View(model);
        }

        var partnershipSession = session.ReExCompaniesHouseSession.Partnership ?? new();
        partnershipSession.IsLimitedPartnership = model.TypeOfPartnership == Core.Sessions.PartnershipType.LimitedPartnership;
        partnershipSession.IsLimitedLiabilityPartnership = model.TypeOfPartnership == Core.Sessions.PartnershipType.LimitedLiabilityPartnership;
        session.ReExCompaniesHouseSession.Partnership = partnershipSession;

        return model.TypeOfPartnership == Core.Sessions.PartnershipType.LimitedPartnership ?
            await SaveSessionAndRedirect(session, nameof(LimitedPartnershipType), PagePath.PartnershipType, PagePath.LimitedPartnershipType) :
            await SaveSessionAndRedirect(session, nameof(LimitedLiabilityPartnership), PagePath.PartnershipType, PagePath.LimitedLiabilityPartnership);
    }

    [HttpGet]
    [Route(PagePath.LimitedPartnershipType)]
    [OrganisationJourneyAccess(PagePath.LimitedPartnershipType)]
    public async Task<IActionResult> LimitedPartnershipType()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.LimitedPartnershipType);

        bool hasIndividualPartners = false;
        bool hasCompanyPartners = false;
        if (session.ReExCompaniesHouseSession.Partnership != null && session.ReExCompaniesHouseSession.Partnership.LimitedPartnership != null)
        {
            hasIndividualPartners = session.ReExCompaniesHouseSession.Partnership.LimitedPartnership.HasIndividualPartners;
            hasCompanyPartners = session.ReExCompaniesHouseSession.Partnership.LimitedPartnership.HasCompanyPartners;
        }

        return View(new LimitedPartnershipTypeRequestViewModel
        {
            hasCompanyPartners = hasCompanyPartners,
            hasIndividualPartners = hasIndividualPartners
        });
    }

    [HttpPost]
    [Route(PagePath.LimitedPartnershipType)]
    [OrganisationJourneyAccess(PagePath.LimitedPartnershipType)]
    public async Task<IActionResult> LimitedPartnershipType(LimitedPartnershipTypeRequestViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.LimitedPartnershipType);
            return View(model);
        }

        if (session.ReExCompaniesHouseSession.Partnership == null)
        {
            session.ReExCompaniesHouseSession.Partnership = new ReExPartnership
            {
                IsLimitedPartnership = true,
                LimitedPartnership = new ReExLimitedPartnership
                {
                    HasIndividualPartners = model.hasIndividualPartners,
                    HasCompanyPartners = model.hasCompanyPartners
                }
            };
        }
        else
        {
            var partnership = session.ReExCompaniesHouseSession.Partnership;

            if (partnership.LimitedPartnership == null)
            {
                partnership.LimitedPartnership = new ReExLimitedPartnership();
            }

            partnership.LimitedPartnership.HasIndividualPartners = model.hasIndividualPartners;
            partnership.LimitedPartnership.HasCompanyPartners = model.hasCompanyPartners;
        }

        return await SaveSessionAndRedirect(session, nameof(NamesOfPartners), PagePath.LimitedPartnershipType, PagePath.LimitedPartnershipNamesOfPartners);
    }

    [HttpGet]
    [Route(PagePath.LimitedPartnershipRole)]
    [OrganisationJourneyAccess(PagePath.LimitedPartnershipRole)]
    public async Task<IActionResult> LimitedPartnershipRole()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.LimitedPartnershipRole);

        dynamic limitedPartnershipRole = null;
        if (session.ReExCompaniesHouseSession.RoleInOrganisation != null)
        {
            limitedPartnershipRole = session.ReExCompaniesHouseSession.RoleInOrganisation;
        }

        return View(new LimitedPartnershipRoleViewModel
        {
            LimitedPartnershipRole = limitedPartnershipRole
        });
    }

    [HttpPost]
    [Route(PagePath.LimitedPartnershipRole)]
    [OrganisationJourneyAccess(PagePath.LimitedPartnershipRole)]
    public async Task<IActionResult> LimitedPartnershipRole(LimitedPartnershipRoleViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.LimitedPartnershipRole);
            return View(model);
        }

        session.ReExCompaniesHouseSession.RoleInOrganisation = model.LimitedPartnershipRole;
        session.ReExCompaniesHouseSession.IsInEligibleToBeApprovedPerson = model.LimitedPartnershipRole == Core.Sessions.RoleInOrganisation.NoneOfTheAbove;

        return await SaveSessionAndRedirect(session, nameof(ApprovedPersonController), nameof(ApprovedPersonController.AddApprovedPerson),
                    PagePath.LimitedPartnershipRole, PagePath.AddAnApprovedPerson);
    }

    [HttpGet]
    [Route(PagePath.LimitedLiabilityPartnership)]
    [OrganisationJourneyAccess(PagePath.LimitedLiabilityPartnership)]
    public async Task<IActionResult> LimitedLiabilityPartnership()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.LimitedLiabilityPartnership);
        LimitedLiabilityPartnershipViewModel model = new()
        {
            IsMemberOfLimitedLiabilityPartnership = session.ReExCompaniesHouseSession?.Partnership
                ?.LimitedLiabilityPartnership?.IsMemberOfLimitedLiabilityPartnership
        };

        return View(model);
    }

    [HttpPost]
    [Route(PagePath.LimitedLiabilityPartnership)]
    [OrganisationJourneyAccess(PagePath.LimitedLiabilityPartnership)]
    public async Task<IActionResult> LimitedLiabilityPartnership(LimitedLiabilityPartnershipViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.LimitedLiabilityPartnership);
            return View(model);
        }

        session.ReExCompaniesHouseSession.Partnership ??= new();
        session.ReExCompaniesHouseSession.Partnership.LimitedLiabilityPartnership ??= new();
        session.ReExCompaniesHouseSession.Partnership.LimitedLiabilityPartnership
            .IsMemberOfLimitedLiabilityPartnership = model.IsMemberOfLimitedLiabilityPartnership!.Value;

        session.ReExCompaniesHouseSession.RoleInOrganisation = model.IsMemberOfLimitedLiabilityPartnership == true
            ? RoleInOrganisation.Member
            : null;

        session.ReExCompaniesHouseSession.IsInEligibleToBeApprovedPerson = !model.IsMemberOfLimitedLiabilityPartnership!.Value;

        return await SaveSessionAndRedirect(session, nameof(ApprovedPersonController), nameof(ApprovedPersonController.AddApprovedPerson),
            PagePath.LimitedLiabilityPartnership, PagePath.AddAnApprovedPerson);
    }

    private static async Task<List<ReExLimitedPartnershipPersonOrCompany>> GetSessionPartners(
    List<LimitedPartnershipPersonOrCompanyViewModel> partners)
    {
        List<ReExLimitedPartnershipPersonOrCompany> partnersSession = [];
        foreach (var partner in partners)
        {
            ReExLimitedPartnershipPersonOrCompany sessionPartner = new()
            {
                Id = partner.Id,
                IsPerson = partner.IsPerson,
                Name = partner.IsPerson ? partner.PersonName : partner.CompanyName
            };
            partnersSession.Add(sessionPartner);
        }

        return partnersSession;
    }
}