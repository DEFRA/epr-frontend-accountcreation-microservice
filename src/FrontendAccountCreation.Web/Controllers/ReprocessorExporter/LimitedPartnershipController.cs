using FrontendAccountCreation.Core.Extensions;
using FrontendAccountCreation.Core.Sessions;
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
        ReExLimitedPartnershipSummary? ltdPartnershipSummarySession = ltdPartnershipSession?.PartnershipSummary;

        model.ExpectsIndividualPartners = ltdPartnershipSummarySession?.HasIndividualPartners ?? true;
        model.ExpectsCompanyPartners = ltdPartnershipSummarySession?.HasCompanyPartners ?? true;

        List<ReExLimitedPartnershipPersonOrCompany>? partnersSession = ltdPartnershipSummarySession?.Partners;
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
        // when command is a Guid its an instruction to remove that item from the model, so do so before validating
        if (Guid.TryParse(command, out Guid removedId))
        {
            model.Partners.RemoveAll(x => x.Id == removedId);
            ModelState.Clear();
            if (!TryValidateModel(model, nameof(LimitedPartnershipPartnersViewModel)))
            {
                // TODO: set back link when page becomes available
                return View(model);
            }
        }
        else if (!ModelState.IsValid)
        {
            // TODO: set back link when page becomes available
            return View(model);
        }

        // Organisation > Company > Partnership > Limited Partnership > Partnership Summary
        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        ReExCompaniesHouseSession companySession = session.ReExCompaniesHouseSession ?? new();
        ReExPartnership partnershipSession = companySession.Partnership ?? new();
        ReExLimitedPartnership ltdPartnershipSession = partnershipSession.LimitedPartnership ?? new();

        // obtain partners from the view model
        List<ReExLimitedPartnershipPersonOrCompany> partners = await GetPartners(model);
        if (command == "add")
        {
            ReExLimitedPartnershipPersonOrCompany newPartner = new()
            {
                Id = Guid.NewGuid()
            };

            partners.Add(newPartner);
        }

        // refresh limited partnership session from the view model
        ReExLimitedPartnershipSummary ltdPartnershipSummarySession = new()
        {
            Partners = partners,
            HasCompanyPartners = model.ExpectsCompanyPartners,
            HasIndividualPartners = model.ExpectsIndividualPartners
        };

        ltdPartnershipSession.PartnershipSummary = ltdPartnershipSummarySession;
        partnershipSession.LimitedPartnership = ltdPartnershipSession;
        companySession.Partnership = partnershipSession;
        session.ReExCompaniesHouseSession = companySession;

        if (command == "save")
        {
            // TODO: there is a missing page between here and LimitedPartnershipRole
            return await SaveSessionAndRedirect(session, nameof(LimitedPartnershipRole),
                PagePath.LimitedPartnershipNamesOfPartners, PagePath.LimitedPartnershipRole);
        }
        else
        {
            return await SaveSessionAndRedirect(session, nameof(NamesOfPartners),
                PagePath.LimitedPartnershipNamesOfPartners, PagePath.LimitedPartnershipNamesOfPartners);
        }
    }

    [HttpGet]
    [Route(PagePath.ApprovedPersonPartnershipRole)]
    public async Task<IActionResult> ApprovedPersonPartnershipRole([FromQuery] Guid id)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var approvedPersons = session?.ReExCompaniesHouseSession?.Partnership!.LimitedPartnership!
            .PartnershipApprovedPersons;
        var index = approvedPersons?.FindIndex(0, x => x.Id.Equals(id));

        if (index is >= 0)
        {
            var viewModel = new LimitedPartnershipApprovedPersonRoleViewModel
            {
                RoleInOrganisation = approvedPersons![index.Value].Role
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

        var approvedPersons = session?.ReExCompaniesHouseSession?.Partnership!.LimitedPartnership!
            .PartnershipApprovedPersons;

        var index = approvedPersons?.FindIndex(x => x.Id == model.Id);

        if (index is null or -1)
        {
            return RedirectToAction(nameof(PersonCanNotBeInvited), new { id = model.Id });
        }

        approvedPersons![index.Value].Role = model.RoleInOrganisation!.Value;

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

    [HttpGet]
    [Route(PagePath.LimitedPartnershipAddApprovedPerson)]
    public async Task<IActionResult> LimitedPartnershipAddApprovedPerson(Guid id)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.PartnershipType);
        return View();
    }

    [HttpPost]
    [Route(PagePath.LimitedPartnershipAddApprovedPerson)]
    public async Task<IActionResult> LimitedPartnershipAddApprovedPerson(LimitedPartnershipAddApprovedPersonViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.PartnershipType);
            return View(model);
        }

        return model.InviteUserOption switch
        {    
            "BeAnApprovedPerson" => RedirectToAction("YouAreApprovedPerson"),
            "InviteAnotherPerson" => RedirectToAction("TeamMemberRoleInOrganisation"),
            _ => RedirectToAction("CheckYourDetails", "AccountCreation") // "InviteLater"
        };
    }

    private static async Task<List<ReExLimitedPartnershipPersonOrCompany>> GetPartners(
        LimitedPartnershipPartnersViewModel model)
    {
        List<ReExLimitedPartnershipPersonOrCompany> partnersSession = new();
        foreach (var partner in model.Partners)
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
                    PartnershipSummary = new ReExLimitedPartnershipSummary
                    {
                        HasIndividualPartners = model.isIndividualPartners,
                        HasCompanyPartners = model.isCompanyPartners
                    }
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