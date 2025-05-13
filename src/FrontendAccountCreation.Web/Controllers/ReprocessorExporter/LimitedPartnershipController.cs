using FrontendAccountCreation.Core.Extensions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Core.Sessions.ReEx.Partnership;
using FrontendAccountCreation.Core.Sessions.ReEx.Partnership.ApprovedPersons;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Sessions;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Mvc;

using FrontendAccountCreation.Core.Extensions;

using FrontendAccountCreation.Web.ViewModels.AccountCreation;

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter;

[Route("re-ex/organisation")]
public partial class LimitedPartnershipController : Controller
{
    private readonly ISessionManager<OrganisationSession> _sessionManager;

    public LimitedPartnershipController(ISessionManager<OrganisationSession> sessionManager)
    {
        _sessionManager = sessionManager;
    }

    [HttpGet]
    [Route(PagePath.LimitedPartnershipNamesOfPartners)]
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
    /// <param name="command">'save' to update partners and continue, 'add' to add new partner, any Guid removes the corresponding item from the model.</param>
    /// <returns></returns>
    [HttpPost]
    [Route(PagePath.LimitedPartnershipNamesOfPartners)]
    public async Task<IActionResult> NamesOfPartners(LimitedPartnershipPartnersViewModel model, string command)
    {
        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        // when command is a Guid its an instruction to remove that item from the model,
        // so do so before validating
        if (Guid.TryParse(command, out Guid removedId))
        {
            model.Partners.RemoveAll(x => x.Id == removedId);
            await SyncSessionWithModel(model.ExpectsCompanyPartners, model.ExpectsIndividualPartners, await GetSessionPartners(model.Partners));
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
            ModelState.Clear();

            SetBackLink(session, PagePath.LimitedPartnershipNamesOfPartners);
            return View(model);
        }

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

        // TODO: there is a missing page between here and LimitedPartnershipRole
        return await SaveSessionAndRedirect(session, nameof(LimitedPartnershipRole),
            PagePath.LimitedPartnershipNamesOfPartners, PagePath.LimitedPartnershipRole);

        async Task SyncSessionWithModel(bool hasCompanyPartners, bool hasIndividualPartners, List<ReExLimitedPartnershipPersonOrCompany> partners)
        {
            // Organisation > Company > Partnership > Limited Partnership
            ReExCompaniesHouseSession companySession = session.ReExCompaniesHouseSession ?? new();
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
    [Route(PagePath.ApprovedPersonPartnershipRole)]
    public async Task<IActionResult> ApprovedPersonPartnershipRole([FromQuery] Guid id)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        // please check this is correct Ehsan
        var approvedPersons = session?.ReExCompaniesHouseSession?.TeamMembers?.ToList();
        var index = approvedPersons?.FindIndex(0, x => x.Id.Equals(id));

        if (index is >= 0)
        {
            var viewModel = new LimitedPartnershipApprovedPersonRoleViewModel
            {
                // please check this is correct Ehsan
                RoleInOrganisation = (ReExLimitedPartnershipRoles?)approvedPersons![index.Value].Role
            };
            return View(viewModel);
        }

        return View();
    }

    [HttpPost]
    [Route(PagePath.ApprovedPersonPartnershipRole)]
    public async Task<IActionResult> ApprovedPersonPartnershipRole(LimitedPartnershipApprovedPersonRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        // please check this is correct Ehsan
        var approvedPersons = session?.ReExCompaniesHouseSession?.TeamMembers;

        var index = approvedPersons?.FindIndex(x => x.Id == model.Id);

        if (index is null or -1)
        {
            return RedirectToAction(nameof(PersonCanNotBeInvited), new { id = model.Id });
        }

        // please check this is correct Ehsan
        approvedPersons![index.Value].Role = (ReExTeamMemberRole?)model.RoleInOrganisation!.Value;

        if (model.RoleInOrganisation == ReExLimitedPartnershipRoles.None)
        {
            await SaveSession(session, PagePath.ApprovedPersonPartnershipRole,
                PagePath.ApprovedPersonPartnershipCanNotBeInvited);

            return RedirectToAction(nameof(PersonCanNotBeInvited), new { id = model.Id });
        }

        await SaveSession(session, PagePath.ApprovedPersonPartnershipRole, PagePath.ApprovedPersonPartnershipDetails);
        return RedirectToAction(nameof(ApprovedPersonDetails), new { id = model.Id });
    }

    [HttpGet]
    [Route(PagePath.ApprovedPersonPartnershipCanNotBeInvited)]
    public IActionResult PersonCanNotBeInvited([FromQuery] Guid id)
    {
        return View(new LimitedPartnershipPersonCanNotBeInvitedViewModel { Id = id });
    }

    [HttpPost]
    [Route(PagePath.ApprovedPersonPartnershipCanNotBeInvited)]
    public IActionResult PersonCanNotBeInvited(LimitedPartnershipPersonCanNotBeInvitedViewModel model)
    {
        return RedirectToAction("CheckYourDetails", "AccountCreation");
    }

    [HttpGet]
    [Route(PagePath.ApprovedPersonPartnershipDetails)]
    public IActionResult ApprovedPersonDetails(Guid id)
    {
        // Placeholder action to satisfy RedirectToAction with id
        return Ok();
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

    [HttpGet]
    [Route(PagePath.PartnershipType)]
    public async Task<IActionResult> PartnershipType()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.PartnershipType);

        return View();
    }

    [HttpPost]
    [Route(PagePath.PartnershipType)]
    public async Task<IActionResult> PartnershipType(PartnershipTypeRequestViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.PartnershipType);
            return View(model);
        }

        session.ReExCompaniesHouseSession.IsPartnership = true;

        if (model.isLimitedPartnership == Core.Sessions.PartnershipType.LimitedPartnership)
        {
            session.ReExCompaniesHouseSession.Partnership = new ReExPartnership
            {
                IsLimitedPartnership = true,
                LimitedPartnership = new ReExLimitedPartnership()
            };
        }

        return await SaveSessionAndRedirect(session, nameof(LimitedPartnershipType), PagePath.PartnershipType, PagePath.LimitedPartnershipType);
    }

    [HttpGet]
    [Route(PagePath.LimitedPartnershipType)]
    public async Task<IActionResult> LimitedPartnershipType()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.LimitedPartnershipType);
        return View();
    }

    [HttpPost]
    [Route(PagePath.LimitedPartnershipType)]
    public async Task<IActionResult> LimitedPartnershipType(LimitedPartnershipTypeRequestViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.LimitedPartnershipType);
            return View(model);
        }

        session.ReExCompaniesHouseSession.IsPartnership = true;

        if (model.isIndividualPartners || model.isCompanyPartners)
        {
            session.ReExCompaniesHouseSession.Partnership = new ReExPartnership
            {
                IsLimitedPartnership = true,
                LimitedPartnership = new ReExLimitedPartnership
                {
                    HasIndividualPartners = model.isIndividualPartners,
                    HasCompanyPartners = model.isCompanyPartners
                }
            };
        }

        return await SaveSessionAndRedirect(session, nameof(NamesOfPartners), PagePath.LimitedPartnershipNamesOfPartners, PagePath.LimitedPartnershipRole);
    }

    [HttpGet]
    [Route(PagePath.LimitedPartnershipRole)]
    public async Task<IActionResult> LimitedPartnershipRole()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.LimitedPartnershipType);
        return View();
    }

    private void SetBackLink(OrganisationSession session, string currentPagePath)
    {
        if (session.IsUserChangingDetails && currentPagePath != PagePath.CheckYourDetails)
        {
            ViewBag.BackLinkToDisplay = PagePath.CheckYourDetails;
        }
        else
        {
            ViewBag.BackLinkToDisplay = session.Journey.PreviousOrDefault(currentPagePath) ?? string.Empty;
        }
    }

    private async Task<RedirectToActionResult> SaveSessionAndRedirect(OrganisationSession session,
        string actionName, string currentPagePath, string? nextPagePath)
    {
        session.IsUserChangingDetails = false;
        await SaveSession(session, currentPagePath, nextPagePath);

        return RedirectToAction(actionName);
    }

    private async Task SaveSession(OrganisationSession session, string currentPagePath, string? nextPagePath)
    {
        ClearRestOfJourney(session, currentPagePath);

        session.Journey.AddIfNotExists(nextPagePath);

        await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
    }

    private static void ClearRestOfJourney(OrganisationSession session, string currentPagePath)
    {
        var index = session.Journey.IndexOf(currentPagePath);

        // this also cover if current page not found (index = -1) then it clears all pages
        session.Journey = session.Journey.Take(index + 1).ToList();
    }
}