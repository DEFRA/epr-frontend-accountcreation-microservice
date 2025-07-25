﻿using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Core.Sessions.ReEx.Partnership;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Extensions;
using FrontendAccountCreation.Web.Sessions;
using FrontendAccountCreation.Web.ViewModels;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter;

[Feature(FeatureFlags.AddOrganisationCompanyHouseDirectorJourney)]
[Route("re-ex/organisation")]
public class LimitedPartnershipController : ControllerBase<OrganisationSession>
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

        ReExTypesOfPartner ltdPartnershipSession = session?.ReExCompaniesHouseSession?.Partnership?.LimitedPartnership;
        bool hasIndividualPartners = ltdPartnershipSession?.HasIndividualPartners ?? true;
        bool hasCompanyPartners = ltdPartnershipSession?.HasCompanyPartners ?? true;

        List<PartnershipPersonOrCompanyViewModel> partnerList = GetExistingPartners(ltdPartnershipSession?.Partners, hasIndividualPartners, hasCompanyPartners);

        PartnershipPartnersViewModel model = new()
        {
            ExpectsIndividualPartners = hasIndividualPartners,
            ExpectsCompanyPartners = hasCompanyPartners,
            Partners = partnerList
        };
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
    public async Task<IActionResult> NamesOfPartners(PartnershipPartnersViewModel model, string command)
    {
        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            AddCustomValidationErrors("NamesOfPartners.", model);
            SetBackLink(session, PagePath.LimitedPartnershipNamesOfPartners);
            return View(model);
        }

        if (command == "add")
        {
            PartnershipPersonOrCompanyViewModel newPartner = new()
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

        // synchronise Companies House session
        async Task SyncSessionWithModel(
            bool hasCompanys,
            bool hasIndividuals,
            List<ReExPersonOrCompanyPartner> partners)
        {
            ReExCompaniesHouseSession companySession = session.ReExCompaniesHouseSession;
            ReExPartnership partnershipSession = companySession.Partnership ?? new();

            ReExTypesOfPartner ltdPartnershipSession = new()
            {
                Partners = partners,
                HasCompanyPartners = hasCompanys,
                HasIndividualPartners = hasIndividuals
            };

            partnershipSession.LimitedPartnership = ltdPartnershipSession;
            companySession.Partnership = partnershipSession;
            session.ReExCompaniesHouseSession = companySession;
        }
    }

    [HttpGet]
    [Route(PagePath.LimitedPartnershipNamesOfPartnersDelete)]
    public async Task<IActionResult> NamesOfPartnersDelete([FromQuery] Guid id)
    {
        DeleteFocusId();

        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        DeleteCompaniesHousePartnerFromSession(session, id);

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
        List<ReExPersonOrCompanyPartner> model = session.ReExCompaniesHouseSession?.Partnership?.LimitedPartnership?.Partners ?? new();

        return View(model);
    }

    /// <summary>
    /// Alternatively one might just get the next page, but that would not update session navigation.
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [Route(PagePath.LimitedPartnershipCheckNamesOfPartners)]
    [OrganisationJourneyAccess(PagePath.LimitedPartnershipCheckNamesOfPartners)]
    public async Task<IActionResult> CheckNamesOfPartners(List<ReExPersonOrCompanyPartner> modelNotUsed)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        return await SaveSessionAndRedirect(session!, nameof(LimitedPartnershipController.LimitedPartnershipRole), PagePath.LimitedPartnershipCheckNamesOfPartners, PagePath.LimitedPartnershipRole);
    }

    [HttpGet]
    [Route(PagePath.LimitedPartnershipCheckNamesOfPartnersDelete)]
    public async Task<IActionResult> CheckNamesOfPartnersDelete([FromQuery] Guid id)
    {
        DeleteFocusId();

        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        DeleteCompaniesHousePartnerFromSession(session, id);

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

        var partnershipSession = session.ReExCompaniesHouseSession.Partnership ?? new ReExPartnership();

        var wasLp = partnershipSession.IsLimitedPartnership;
        var wasLlp = partnershipSession.IsLimitedLiabilityPartnership;

        var isLp = model.TypeOfPartnership == Core.Sessions.PartnershipType.LimitedPartnership;
        var isLlp = model.TypeOfPartnership == Core.Sessions.PartnershipType.LimitedLiabilityPartnership;

        // clear existing session values when the user changes their original decision
        if ((wasLp && !isLp) || (wasLlp && !isLlp))
        {
            partnershipSession.LimitedPartnership = null;
            partnershipSession.LimitedLiabilityPartnership = null;
            session.ReExCompaniesHouseSession.TeamMembers = null;
            session.ReExCompaniesHouseSession.RoleInOrganisation = null;
            session.ReExCompaniesHouseSession.IsInEligibleToBeApprovedPerson = false;
            session.InviteUserOption = null;
        }

        partnershipSession.IsLimitedPartnership = isLp;
        partnershipSession.IsLimitedLiabilityPartnership = isLlp;
        session.ReExCompaniesHouseSession.Partnership = partnershipSession;

        //Set producer type to LP or LLP
        session.ReExCompaniesHouseSession.ProducerType = partnershipSession.IsLimitedPartnership ?
                                                                ProducerType.LimitedPartnership :
                                                                ProducerType.LimitedLiabilityPartnership;

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
        if (session != null
            && session.ReExCompaniesHouseSession != null
            && session.ReExCompaniesHouseSession.Partnership != null
            && session.ReExCompaniesHouseSession.Partnership.LimitedPartnership != null)
        {
            hasIndividualPartners = session.ReExCompaniesHouseSession.Partnership.LimitedPartnership.HasIndividualPartners;
            hasCompanyPartners = session.ReExCompaniesHouseSession.Partnership.LimitedPartnership.HasCompanyPartners;
        }

        return View(new WhatSortOfPartnerRequestViewModel
        {
            HasCompanyPartners = hasCompanyPartners,
            HasIndividualPartners = hasIndividualPartners
        });
    }

    [HttpPost]
    [Route(PagePath.LimitedPartnershipType)]
    [OrganisationJourneyAccess(PagePath.LimitedPartnershipType)]
    public async Task<IActionResult> LimitedPartnershipType(WhatSortOfPartnerRequestViewModel model)
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
                LimitedPartnership = new ReExTypesOfPartner
                {
                    HasIndividualPartners = model.HasIndividualPartners,
                    HasCompanyPartners = model.HasCompanyPartners
                }
            };
        }
        else
        {
            var partnership = session.ReExCompaniesHouseSession.Partnership;

            if (partnership.LimitedPartnership == null)
            {
                partnership.LimitedPartnership = new ReExTypesOfPartner();
            }

            partnership.LimitedPartnership.HasIndividualPartners = model.HasIndividualPartners;
            partnership.LimitedPartnership.HasCompanyPartners = model.HasCompanyPartners;
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
            RoleInOrganisation = limitedPartnershipRole
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

        session.ReExCompaniesHouseSession.RoleInOrganisation = model.RoleInOrganisation;
        session.ReExCompaniesHouseSession.IsInEligibleToBeApprovedPerson = model.RoleInOrganisation == Core.Sessions.RoleInOrganisation.NoneOfTheAbove;

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

    //Non company house User Role in Partnership
    [HttpGet]
    [Route(PagePath.NonCompaniesHousePartnershipYourRole)]
    [OrganisationJourneyAccess(PagePath.NonCompaniesHousePartnershipYourRole)]
    public async Task<IActionResult> NonCompaniesHousePartnershipRole()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.NonCompaniesHousePartnershipYourRole);

        return View(new NonCompaniesHousePartnershipRoleModel
        {
            RoleInOrganisation = session.ReExManualInputSession.RoleInOrganisation
        });
    }

    [HttpPost]
    [Route(PagePath.NonCompaniesHousePartnershipYourRole)]
    [OrganisationJourneyAccess(PagePath.NonCompaniesHousePartnershipYourRole)]
    public async Task<IActionResult> NonCompaniesHousePartnershipRole(NonCompaniesHousePartnershipRoleModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.NonCompaniesHousePartnershipYourRole);
            return View(model);
        }

        session.ReExManualInputSession.RoleInOrganisation = model.RoleInOrganisation;
        session.ReExManualInputSession.IsEligibleToBeApprovedPerson = model.RoleInOrganisation != RoleInOrganisation.NoneOfTheAbove;

        if (model.RoleInOrganisation == RoleInOrganisation.NoneOfTheAbove)
        {
            return await SaveSessionAndRedirect(session, nameof(ApprovedPersonController), nameof(ApprovedPersonController.NonCompaniesHousePartnershipInviteApprovedPerson), PagePath.NonCompaniesHousePartnershipYourRole, PagePath.NonCompaniesHousePartnershipInviteApprovedPerson);
        }

        return await SaveSessionAndRedirect(session, nameof(ApprovedPersonController), nameof(ApprovedPersonController.NonCompaniesHousePartnershipAddApprovedPerson),
                    PagePath.NonCompaniesHousePartnershipYourRole, PagePath.NonCompaniesHousePartnershipAddApprovedPerson);
    }

    [HttpGet]
    [Route(PagePath.NonCompaniesHousePartnershipType)]
    [OrganisationJourneyAccess(PagePath.NonCompaniesHousePartnershipType)]
    public async Task<IActionResult> NonCompaniesHousePartnershipType()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.NonCompaniesHousePartnershipType);

        bool hasIndividualPartners = false;
        bool hasCompanyPartners = false;
        if (session.ReExManualInputSession != null && session.ReExManualInputSession.TypesOfPartner != null)
        {
            hasIndividualPartners = session.ReExManualInputSession.TypesOfPartner.HasIndividualPartners;
            hasCompanyPartners = session.ReExManualInputSession.TypesOfPartner.HasCompanyPartners;
        }

        return View(new WhatSortOfPartnerRequestViewModel
        {
            HasCompanyPartners = hasCompanyPartners,
            HasIndividualPartners = hasIndividualPartners
        });
    }

    [HttpPost]
    [Route(PagePath.NonCompaniesHousePartnershipType)]
    [OrganisationJourneyAccess(PagePath.NonCompaniesHousePartnershipType)]
    public async Task<IActionResult> NonCompaniesHousePartnershipType(WhatSortOfPartnerRequestViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.NonCompaniesHousePartnershipType);
            return View(model);
        }

        if (session != null && session.ReExManualInputSession != null)
        {
            if (session.ReExManualInputSession.TypesOfPartner != null)
            {
                session.ReExManualInputSession.TypesOfPartner.HasIndividualPartners = model.HasIndividualPartners;
                session.ReExManualInputSession.TypesOfPartner.HasCompanyPartners = model.HasCompanyPartners;
            }
            else
            {
                session.ReExManualInputSession.TypesOfPartner = new ReExTypesOfPartner
                {
                    HasIndividualPartners = model.HasIndividualPartners,
                    HasCompanyPartners = model.HasCompanyPartners
                };
            }
        }

        return await SaveSessionAndRedirect(session, nameof(NonCompaniesHousePartnershipNamesOfPartners), PagePath.NonCompaniesHousePartnershipType, PagePath.NonCompaniesHousePartnershipNamesOfPartners);
    }

    [HttpGet]
    [Route(PagePath.NonCompaniesHousePartnershipNamesOfPartners)]
    [OrganisationJourneyAccess(PagePath.NonCompaniesHousePartnershipNamesOfPartners)]
    public async Task<IActionResult> NonCompaniesHousePartnershipNamesOfPartners()
    {
        OrganisationSession session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.NonCompaniesHousePartnershipNamesOfPartners);

        ReExTypesOfPartner typesOfPartnersSession = session!.ReExManualInputSession?.TypesOfPartner;
        bool hasIndividualPartners = typesOfPartnersSession?.HasIndividualPartners ?? true;
        bool hasCompanyPartners = typesOfPartnersSession?.HasCompanyPartners ?? true;

        List<PartnershipPersonOrCompanyViewModel> partnerList = GetExistingPartners(typesOfPartnersSession?.Partners, hasIndividualPartners, hasCompanyPartners);

        PartnershipPartnersViewModel model = new()
        {
            ExpectsIndividualPartners = hasIndividualPartners,
            ExpectsCompanyPartners = hasCompanyPartners,
            Partners = partnerList
        };
        return View(model);
    }

    /// <summary>
    /// Save partner details to session
    /// </summary>
    /// <param name="model">View model</param>
    /// <param name="command">'save' to update partners and continue, 'add' to add new partner.</param>
    /// <returns></returns>
    [HttpPost]
    [Route(PagePath.NonCompaniesHousePartnershipNamesOfPartners)]
    [OrganisationJourneyAccess(PagePath.NonCompaniesHousePartnershipNamesOfPartners)]
    public async Task<IActionResult> NonCompaniesHousePartnershipNamesOfPartners(PartnershipPartnersViewModel model, string command)
    {
        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            AddCustomValidationErrors("NonCompaniesHousePartnershipNamesOfPartners.", model);
            SetBackLink(session, PagePath.NonCompaniesHousePartnershipNamesOfPartners);
            return View(model);
        }

        if (command == "add")
        {
            PartnershipPersonOrCompanyViewModel newPartner = new()
            {
                Id = Guid.NewGuid()
            };
            model.Partners.Add(newPartner);
            await SyncSessionWithModel(model.ExpectsCompanyPartners, model.ExpectsIndividualPartners, await GetSessionPartners(model.Partners));
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            SetBackLink(session, PagePath.NonCompaniesHousePartnershipNamesOfPartners);
            return View(model);
        }

        await SyncSessionWithModel(model.ExpectsCompanyPartners, model.ExpectsIndividualPartners, await GetSessionPartners(model.Partners));

        return await SaveSessionAndRedirect(session, nameof(NonCompaniesHousePartnershipCheckNamesOfPartners),
            PagePath.NonCompaniesHousePartnershipNamesOfPartners, PagePath.NonCompaniesHousePartnershipCheckNamesOfPartners);

        // synchronise Non Companies House session
        async Task SyncSessionWithModel(
            bool hasCompanys,
            bool hasIndividuals,
            List<ReExPersonOrCompanyPartner> partners)
        {
            ReExManualInputSession nonCompaniesHouseSession = session.ReExManualInputSession;

            ReExTypesOfPartner typesOfPartner = new()
            {
                Partners = partners,
                HasCompanyPartners = hasCompanys,
                HasIndividualPartners = hasIndividuals
            };

            nonCompaniesHouseSession.TypesOfPartner = typesOfPartner;
            session.ReExManualInputSession = nonCompaniesHouseSession;
        }
    }

    [HttpGet]
    [Route(PagePath.NonCompaniesHousePartnershipNamesOfPartnersDelete)]
    public async Task<IActionResult> NonCompaniesHousePartnershipNamesOfPartnersDelete([FromQuery] Guid id)
    {
        DeleteFocusId();

        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        DeleteNonCompaniesHousePartnerFromSession(session, id);

        return await SaveSessionAndRedirect(session, nameof(NonCompaniesHousePartnershipNamesOfPartners),
            PagePath.NonCompaniesHousePartnershipNamesOfPartnersDelete, null);
    }

    [HttpGet]
    [Route(PagePath.NonCompaniesHousePartnershipCheckNamesOfPartners)]
    [OrganisationJourneyAccess(PagePath.NonCompaniesHousePartnershipCheckNamesOfPartners)]
    public async Task<IActionResult> NonCompaniesHousePartnershipCheckNamesOfPartners()
    {
        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.NonCompaniesHousePartnershipCheckNamesOfPartners);

        List<ReExPersonOrCompanyPartner> model = session.ReExManualInputSession?.TypesOfPartner?.Partners ?? new();

        return View(model);
    }

    [HttpPost]
    [Route(PagePath.NonCompaniesHousePartnershipCheckNamesOfPartners)]
    [OrganisationJourneyAccess(PagePath.NonCompaniesHousePartnershipCheckNamesOfPartners)]
    public async Task<IActionResult> NonCompaniesHousePartnershipCheckNamesOfPartners(List<ReExPersonOrCompanyPartner> modelNotUsed)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        return await SaveSessionAndRedirect(session, nameof(LimitedPartnershipController.NonCompaniesHousePartnershipRole), PagePath.NonCompaniesHousePartnershipCheckNamesOfPartners, PagePath.NonCompaniesHousePartnershipYourRole);
    }

    [HttpGet]
    [Route(PagePath.NonCompaniesHousePartnershipCheckNamesOfPartnersDelete)]
    public async Task<IActionResult> NonCompaniesHousePartnershipCheckNamesOfPartnersDelete([FromQuery] Guid id)
    {
        DeleteFocusId();

        OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        DeleteNonCompaniesHousePartnerFromSession(session, id);

        return await SaveSessionAndRedirect(session, nameof(NonCompaniesHousePartnershipCheckNamesOfPartners),
            PagePath.NonCompaniesHousePartnershipCheckNamesOfPartners, null);
    }

    private static async Task<List<ReExPersonOrCompanyPartner>> GetSessionPartners(List<PartnershipPersonOrCompanyViewModel> partners)
    {
        List<ReExPersonOrCompanyPartner> partnersSession = [];
        foreach (var partner in partners)
        {
            ReExPersonOrCompanyPartner sessionPartner = new()
            {
                Id = partner.Id,
                IsPerson = partner.IsPerson,
                Name = partner.IsPerson ? partner.PersonName : partner.CompanyName
            };
            partnersSession.Add(sessionPartner);
        }

        return partnersSession;
    }

    private void AddCustomValidationErrors(string localizerPrefix, PartnershipPartnersViewModel model)
    {
        var keysToUpdate = ModelState.Keys
            .Where(k => k.StartsWith("Partners[") && k.EndsWith("].IsPersonOrCompanyButNotBoth"))
            .ToList();

        foreach (var key in keysToUpdate)
        {
            if (ModelState.GetFieldValidationState(key) == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
            {
                int? index = ExtractIndexFromKey(key);
                if (index.HasValue)
                {
                    ModelState.Remove(key);
                    AddValidationError(localizerPrefix, model, index.Value);
                }
            }
        }
    }

    private static int? ExtractIndexFromKey(string key)
    {
        int start = "Partners[".Length;
        int end = key.IndexOf(']', start);
        if (end > start && int.TryParse(key.AsSpan(start, end - start), out int index))
        {
            return index;
        }
        return null;
    }

    private void AddValidationError(string localizerPrefix, PartnershipPartnersViewModel model, int index)
    {
        if (model.ExpectsCompanyPartners && model.ExpectsIndividualPartners)
        {
            ModelState.AddModelError($"Partners[{index}].PersonName", $"{localizerPrefix}ValidationError_Both");
        }
        else if (model.ExpectsCompanyPartners)
        {
            ModelState.AddModelError($"Partners[{index}].CompanyName", $"{localizerPrefix}ValidationError_Company");
        }
        else if (model.ExpectsIndividualPartners)
        {
            ModelState.AddModelError($"Partners[{index}].PersonName", $"{localizerPrefix}ValidationError_Individual");
        }
        else
        {
            ModelState.AddModelError($"Partners[{index}].PersonName", $"{localizerPrefix}ValidationError_Both");
        }
    }

    private static List<PartnershipPersonOrCompanyViewModel> GetExistingPartners(List<ReExPersonOrCompanyPartner>? partnersSession,
        bool expectsPersons, bool expectsCompanys)
    {
        List<PartnershipPersonOrCompanyViewModel> partnerList = [];
        if (partnersSession != null)
        {
            partnerList = partnersSession.Select(item => (PartnershipPersonOrCompanyViewModel)item)
                .Where(x => (
                        (!x.IsPersonOrCompanyButNotBoth) ||
                        (x.IsPerson && expectsPersons) ||
                        (x.IsCompany && expectsCompanys)
                            )).ToList();
        }

        if (partnerList.Count.Equals(0))
        {
            PartnershipPersonOrCompanyViewModel newPartner = new()
            {
                Id = Guid.NewGuid()
            };
            partnerList.Add(newPartner);
        }

        return partnerList;
    }

    // Move into private method to keep SonarQube happy
    private static void DeleteCompaniesHousePartnerFromSession(OrganisationSession? session, Guid id)
    {
        var partners = session?.ReExCompaniesHouseSession?.Partnership?.LimitedPartnership?.Partners;
        if (partners != null)
        {
            partners.RemoveAll(x => x.Id == id);
        }
    }

    private static void DeleteNonCompaniesHousePartnerFromSession(OrganisationSession? session, Guid id)
    {
        var partners = session?.ReExManualInputSession?.TypesOfPartner?.Partners;
        if (partners != null)
        {
            partners.RemoveAll(x => x.Id == id);
        }
    }
}