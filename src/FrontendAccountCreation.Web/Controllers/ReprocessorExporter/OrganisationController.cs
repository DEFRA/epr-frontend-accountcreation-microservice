﻿using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Models;
using FrontendAccountCreation.Core.Services;
using FrontendAccountCreation.Core.Services.Dto.Company;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Pages.Re_Ex.Organisation;
using FrontendAccountCreation.Web.Sessions;
using FrontendAccountCreation.Web.ViewModels;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using FrontendAccountCreation.Web.ViewModels.ReExAccount.Unincorporated;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.FeatureManagement;
using Microsoft.Identity.Web;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter;

[Feature(FeatureFlags.AddOrganisationCompanyHouseDirectorJourney)]
[Route("re-ex/organisation")]
[AuthorizeForScopes(ScopeKeySection = ConfigKeys.FacadeScope)]
public class OrganisationController : ControllerBase<OrganisationSession>
{
    private readonly ISessionManager<OrganisationSession> _sessionManager;
    private readonly IFacadeService _facadeService;
    //to-do: this is only used by one method, so we should inject it directly into the method
    private readonly IReExAccountMapper _reExAccountMapper;
    private readonly ILogger<OrganisationController> _logger;
    private readonly ExternalUrlsOptions _urlOptions;
    private readonly ServiceKeysOptions _serviceKeyOptions;

    public OrganisationController(
         ISessionManager<OrganisationSession> sessionManager,
         IFacadeService facadeService,
         IReExAccountMapper reExAccountMapper,
         IMultipleOptions multipleOptions,
         ILogger<OrganisationController> logger) : base(sessionManager)
    {
        _sessionManager = sessionManager;
        _facadeService = facadeService;
        _reExAccountMapper = reExAccountMapper;
        _urlOptions = multipleOptions.UrlOptions;
        _serviceKeyOptions = multipleOptions.ServiceKeysOptions;
        _logger = logger;
    }

    [HttpGet]
    [Route("inject-error")]
    public IActionResult InjectError()
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route(PagePath.RegisteredWithCompaniesHouse)]
    [OrganisationJourneyAccess(PagePath.RegisteredWithCompaniesHouse)]
    public async Task<IActionResult> RegisteredWithCompaniesHouse()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.RegisteredWithCompaniesHouse);

        YesNoAnswer? isTheOrganisationRegistered = session.OrganisationType switch
        {
            null => null,
            OrganisationType.NotSet => null,
            OrganisationType.CompaniesHouseCompany => YesNoAnswer.Yes,
            OrganisationType.NonCompaniesHouseCompany => YesNoAnswer.No
        };

        return View(new RegisteredWithCompaniesHouseViewModel
        {
            IsTheOrganisationRegistered = isTheOrganisationRegistered
        });
    }

    [HttpPost]
    [Route(PagePath.RegisteredWithCompaniesHouse)]
    [OrganisationJourneyAccess(PagePath.RegisteredWithCompaniesHouse)]
    public async Task<IActionResult> RegisteredWithCompaniesHouse(
        [FromServices] IFeatureManager featureManager,
        RegisteredWithCompaniesHouseViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.RegisteredWithCompaniesHouse);
            return View(model);
        }

        session.OrganisationType = model.IsTheOrganisationRegistered switch
        {
            YesNoAnswer.Yes => OrganisationType.CompaniesHouseCompany,
            YesNoAnswer.No => OrganisationType.NonCompaniesHouseCompany,
            _ => session.OrganisationType
        };

        if (model.IsTheOrganisationRegistered == YesNoAnswer.Yes)
        {
            if (session.ReExManualInputSession != null)
            {
                session.ReExManualInputSession = null;
            }
            return await SaveSessionAndRedirect(session, nameof(CompaniesHouseNumber),
                PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber);
        }

        if (await featureManager.IsEnabledAsync(FeatureFlags.AddOrganisationSoleTraderJourney))
        {
            if (session.ReExCompaniesHouseSession != null)
            {
                session.ReExCompaniesHouseSession = null;
            }
            return await SaveSessionAndRedirect(session, nameof(IsUkMainAddress), PagePath.RegisteredWithCompaniesHouse,
                PagePath.IsUkMainAddress);
        }

        return Redirect(PagePath.PageNotFoundReEx);
    }

    [HttpGet]
    [Route(PagePath.IsUkMainAddress)]
    [OrganisationJourneyAccess(PagePath.IsUkMainAddress)]
    public async Task<IActionResult> IsUkMainAddress()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.IsUkMainAddress);

        YesNoAnswer? isUkMainAddress = null;
        if (session.IsUkMainAddress != null)
        {
            isUkMainAddress = session.IsUkMainAddress.Value ? YesNoAnswer.Yes : YesNoAnswer.No;
        }

        return View(new IsUkMainAddressViewModel
        {
            IsUkMainAddress = isUkMainAddress
        });
    }

    [HttpPost]
    [Route(PagePath.IsUkMainAddress)]
    [OrganisationJourneyAccess(PagePath.IsUkMainAddress)]
    public async Task<IActionResult> IsUkMainAddress(IsUkMainAddressViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.IsUkMainAddress);
            return View(model);
        }

        session.IsUkMainAddress = model.IsUkMainAddress == YesNoAnswer.Yes;

        // reset some values - if flow has been changed
        session.AreTheyIndividualInCharge = null;
        session.TheyManageOrControlOrganisation = null;

        if (!string.IsNullOrWhiteSpace(session.TradingName))
        {
            session.TradingName = null;
        }

        // reset values - in case user coming from back button to change the flow
        session.UkNation = null;
        session.IsIndividualInCharge = null;
        session.AreTheyIndividualInCharge = null;
        session.UserManagesOrControls = null;
        session.TheyManageOrControlOrganisation = null;

        session.ReExManualInputSession = new ReExManualInputSession()
        {
            OrganisationName = null,
            ProducerType = model.IsUkMainAddress == YesNoAnswer.No ? ProducerType.NonUkOrganisation : null
        };

        return await SaveSessionAndRedirect(session, nameof(OrganisationName),
            PagePath.IsUkMainAddress, PagePath.OrganisationName);
    }

    [HttpGet]
    [Route(PagePath.IsTradingNameDifferent)]
    [OrganisationJourneyAccess(PagePath.IsTradingNameDifferent)]
    public async Task<IActionResult> IsTradingNameDifferent()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.IsTradingNameDifferent);

        YesNoAnswer? isTradingNameDifferent = null;
        if (session.IsTradingNameDifferent != null)
        {
            isTradingNameDifferent = session.IsTradingNameDifferent.Value ? YesNoAnswer.Yes : YesNoAnswer.No;
        }

        return View(new IsTradingNameDifferentViewModel
        {
            IsTradingNameDifferent = isTradingNameDifferent,
            IsNonUk = session.IsUkMainAddress == false,
            IsCompaniesHouseFlow = session.IsCompaniesHouseFlow
        });
    }

    [HttpPost]
    [Route(PagePath.IsTradingNameDifferent)]
    [OrganisationJourneyAccess(PagePath.IsTradingNameDifferent)]
    public async Task<IActionResult> IsTradingNameDifferent(IsTradingNameDifferentViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.IsTradingNameDifferent);
            return View(model);
        }

        session.IsTradingNameDifferent = model.IsTradingNameDifferent == YesNoAnswer.Yes;

        // remove any prev trading name values
        session.TradingName = null;

        if (model.IsTradingNameDifferent == YesNoAnswer.Yes)
        {
            return await SaveSessionAndRedirectToPage(
                session,
                nameof(TradingName),
                PagePath.IsTradingNameDifferent,
                PagePath.TradingName);
        }

        string nextAction, nextPagePath;

        if (session.IsCompaniesHouseFlow)
        {
            nextAction = nameof(IsOrganisationAPartner);
            nextPagePath = PagePath.IsPartnership;
        }
        else if (session.IsUkMainAddress == false)
        {
            nextAction = nameof(AddressOverseas);
            nextPagePath = PagePath.AddressOverseas;
        }
        else
        {
            nextAction = nameof(TypeOfOrganisation);
            nextPagePath = PagePath.TypeOfOrganisation;
        }

        return await SaveSessionAndRedirect(session, nextAction, PagePath.IsTradingNameDifferent, nextPagePath);
    }

    [HttpGet]
    [Route(PagePath.ManageControl)]
    [OrganisationJourneyAccess(PagePath.ManageControl)]
    public async Task<IActionResult> ManageControl()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.ManageControl);

        return View(new ManageControlViewModel
        {
            UserManagesOrControls = session.UserManagesOrControls,
            IsUnincorporatedFlow = session.ReExManualInputSession.ProducerType == ProducerType.UnincorporatedBody
        });
    }

    [HttpPost]
    [Route(PagePath.ManageControl)]
    [OrganisationJourneyAccess(PagePath.ManageControl)]
    public async Task<IActionResult> ManageControl(ManageControlViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.ManageControl);
            return View(model);
        }

        session.UserManagesOrControls = model.UserManagesOrControls;
        session.ReExManualInputSession!.IsEligibleToBeApprovedPerson = model.UserManagesOrControls == YesNoNotSure.Yes;

        return await SaveSessionAndRedirect(session,
            nameof(ApprovedPersonController),
            nameof(ApprovedPersonController.AddApprovedPerson),
            PagePath.ManageControl,
            PagePath.AddAnApprovedPerson);
    }

    [HttpGet]
    [Route(PagePath.AddressOverseas)]
    [OrganisationJourneyAccess(PagePath.AddressOverseas)]
    public async Task<IActionResult> AddressOverseas()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.AddressOverseas);

        var viewModel = new BusinessAddressOverseasViewModel();

        if (session.ReExManualInputSession?.BusinessAddress?.IsManualAddress == true)
        {
            viewModel.Country = session.ReExManualInputSession.BusinessAddress.Country;
            viewModel.AddressLine1 = session.ReExManualInputSession.BusinessAddress.BuildingName;
            viewModel.AddressLine2 = session.ReExManualInputSession.BusinessAddress.Street;
            viewModel.TownOrCity = session.ReExManualInputSession.BusinessAddress.Town;
            viewModel.StateProvinceRegion = session.ReExManualInputSession.BusinessAddress.County;
            viewModel.Postcode = session.ReExManualInputSession.BusinessAddress.Postcode;
        }

        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.AddressOverseas)]
    [OrganisationJourneyAccess(PagePath.AddressOverseas)]
    public async Task<IActionResult> AddressOverseas(BusinessAddressOverseasViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.AddressOverseas);
            return View(model);
        }

        var address = session!.ReExManualInputSession!.BusinessAddress ??= new Address();

        address.Country = model.Country;
        address.BuildingName = model.AddressLine1;
        address.Street = model.AddressLine2;
        address.Town = model.TownOrCity;
        address.County = model.StateProvinceRegion;
        address.Postcode = model.Postcode;
        address.IsManualAddress = true;

        return await SaveSessionAndRedirectToPage(session, nameof(UkRegulator),
            PagePath.AddressOverseas, PagePath.UkRegulator);
    }

    [HttpGet]
    [Route(PagePath.TypeOfOrganisation)]
    [OrganisationJourneyAccess(PagePath.TypeOfOrganisation)]
    public async Task<IActionResult> TypeOfOrganisation()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.TypeOfOrganisation);

        var viewModel = new ReExTypeOfOrganisationViewModel()
        {
            ProducerType = session.ReExManualInputSession?.ProducerType
        };

        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.TypeOfOrganisation)]
    [OrganisationJourneyAccess(PagePath.TypeOfOrganisation)]
    public async Task<IActionResult> TypeOfOrganisation(ReExTypeOfOrganisationViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.TypeOfOrganisation);
            return View(model);
        }

        session.ReExManualInputSession ??= new ReExManualInputSession();
        session.ReExManualInputSession.ProducerType = model.ProducerType;
        session.ReExCompaniesHouseSession = null;

        return await SaveSessionAndRedirect(session, nameof(UkNation), PagePath.TypeOfOrganisation,
            PagePath.UkNation);
    }

    [HttpGet]
    [Route(PagePath.IsPartnership)]
    [OrganisationJourneyAccess(PagePath.IsPartnership)]
    public async Task<IActionResult> IsOrganisationAPartner()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.IsPartnership);

        YesNoAnswer? isOrganisationAPartnership = null;
        if (session.IsOrganisationAPartnership != null)
        {
            isOrganisationAPartnership = session.IsOrganisationAPartnership.Value ? YesNoAnswer.Yes : YesNoAnswer.No;
        }

        return View(new IsOrganisationAPartnerViewModel
        {
            IsOrganisationAPartner = isOrganisationAPartnership
        });
    }

    [HttpPost]
    [Route(PagePath.IsPartnership)]
    public async Task<IActionResult> IsOrganisationAPartner(IsOrganisationAPartnerViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.IsPartnership);
            return View(model);
        }

        var wasOrganisationAPartnership = session.IsOrganisationAPartnership;
        var isOrganisationAPartnership = model.IsOrganisationAPartner == YesNoAnswer.Yes;

        // clear existing session values when the user changes their original decision
        if (wasOrganisationAPartnership.HasValue && (wasOrganisationAPartnership != isOrganisationAPartnership))
        {
            session.ReExCompaniesHouseSession.Partnership = null; // partnership details
            session.ReExCompaniesHouseSession.TeamMembers = null; // invitee details
            session.ReExCompaniesHouseSession.ProducerType = null; // setting producer to null as from here it can go to non paternship flow
            session.ReExCompaniesHouseSession.RoleInOrganisation = null;
            session.ReExCompaniesHouseSession.IsInEligibleToBeApprovedPerson = false;
            session.InviteUserOption = null;
        }
        session.IsOrganisationAPartnership = isOrganisationAPartnership;

        if (session.IsOrganisationAPartnership == true)
        {
            // both options 'Yes/No' ending up same pagePath
            return await SaveSessionAndRedirect(session, nameof(LimitedPartnershipController), nameof(LimitedPartnershipController.PartnershipType), PagePath.IsPartnership,
                PagePath.PartnershipType);
        }
        return await SaveSessionAndRedirect(session, nameof(RoleInOrganisation), PagePath.IsPartnership, PagePath.RoleInOrganisation);
    }

    [HttpGet]
    [Route(PagePath.RoleInOrganisation)]
    [OrganisationJourneyAccess(PagePath.RoleInOrganisation)]
    public async Task<IActionResult> RoleInOrganisation()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.RoleInOrganisation);

        var viewModel = new RoleInOrganisationViewModel()
        {
            RoleInOrganisation = session.ReExCompaniesHouseSession?.RoleInOrganisation
        };

        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.RoleInOrganisation)]
    [OrganisationJourneyAccess(PagePath.RoleInOrganisation)]
    public async Task<IActionResult> RoleInOrganisation(RoleInOrganisationViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.RoleInOrganisation);
            return View(model);
        }

        if (session.ReExCompaniesHouseSession == null)
        {
            ReExCompaniesHouseSession companiesHouseSession = new ReExCompaniesHouseSession();
            session.ReExCompaniesHouseSession = companiesHouseSession;
        }
        session.ReExCompaniesHouseSession.RoleInOrganisation = model.RoleInOrganisation.Value;
        session.ReExCompaniesHouseSession.IsInEligibleToBeApprovedPerson = model.RoleInOrganisation == Core.Sessions.RoleInOrganisation.NoneOfTheAbove;

        return await SaveSessionAndRedirect(session, nameof(ApprovedPersonController), nameof(ApprovedPersonController.AddApprovedPerson), PagePath.RoleInOrganisation,
                PagePath.AddAnApprovedPerson);
    }

    [HttpGet]
    [Route(PagePath.UnincorporatedRoleInOrganisation)]
    public async Task<IActionResult> UnincorporatedRoleInOrganisation()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.UnincorporatedRoleInOrganisation);
        return View(new ReExRoleInOrganisationViewModel { Role = session.ReExManualInputSession.RoleInUnincorporatedOrganisation });
    }

    [HttpPost]
    [Route(PagePath.UnincorporatedRoleInOrganisation)]
    public async Task<IActionResult> UnincorporatedRoleInOrganisation(ReExRoleInOrganisationViewModel viewModel)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.UnincorporatedRoleInOrganisation);
            return View(viewModel);
        }
        session.ReExManualInputSession ??= new ReExManualInputSession();
        session.ReExManualInputSession.RoleInUnincorporatedOrganisation = viewModel.Role;

        return await SaveSessionAndRedirect(session, nameof(ManageControl), PagePath.UnincorporatedRoleInOrganisation, PagePath.ManageControl);
    }

    [HttpGet]
    [Route(PagePath.CompaniesHouseNumber)]
    [OrganisationJourneyAccess(PagePath.CompaniesHouseNumber)]
    public async Task<IActionResult> CompaniesHouseNumber()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.CompaniesHouseNumber);

        ViewBag.FindAndUpdateCompanyInformationLink = _urlOptions.FindAndUpdateCompanyInformation;

        var viewModel = new ReExCompaniesHouseNumberViewModel
        {
            CompaniesHouseNumber = session.ReExCompaniesHouseSession?.Company?.CompaniesHouseNumber,
        };

        if (TempData["ModelState"] is not null)
        {
            ModelState.Merge(DeserializeModelState(TempData["ModelState"].ToString()));
            viewModel.CompaniesHouseNumber = TempData["CompaniesHouseNumber"].ToString();
        }

        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.CompaniesHouseNumber)]
    public async Task<IActionResult> CompaniesHouseNumber(ReExCompaniesHouseNumberViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.CompaniesHouseNumber);

            ViewBag.FindAndUpdateCompanyInformationLink = _urlOptions.FindAndUpdateCompanyInformation;

            return View(model);
        }

        session.ReExCompaniesHouseSession = new ReExCompaniesHouseSession();

        // reset data - if coming from different flow i.e. Sole-trader
        session.TradingName = null;
        session.IsTradingNameDifferent = null;
        session.IsUkMainAddress = null;
        if (session.ReExManualInputSession != null)
        {
            session.ReExManualInputSession = null;
        }

        Company? company;

        try
        {
            company = await _facadeService.GetCompanyByCompaniesHouseNumberAsync(model.CompaniesHouseNumber);
        }
        catch (Exception exception)
        {
            LogCompaniesHouseLookupFailed(exception, model.CompaniesHouseNumber);

            return await SaveSessionAndRedirect(session, nameof(CannotVerifyOrganisation), PagePath.CompaniesHouseNumber, PagePath.CannotVerifyOrganisation);
        }

        if (company == null)
        {
            ModelState.AddModelError(nameof(ReExCompaniesHouseNumberViewModel.CompaniesHouseNumber), "CompaniesHouseNumber.NotFoundError");

            SetBackLink(session, PagePath.CompaniesHouseNumber);

            ViewBag.FindAndUpdateCompanyInformationLink = _urlOptions.FindAndUpdateCompanyInformation;
            TempData["ModelState"] = SerializeModelState(ModelState);
            TempData["CompaniesHouseNumber"] = model.CompaniesHouseNumber;

            return RedirectToAction(nameof(CompaniesHouseNumber));
        }

        session.ReExCompaniesHouseSession.Company = company;

        return await SaveSessionAndRedirect(session, nameof(ConfirmCompanyDetails), PagePath.CompaniesHouseNumber, PagePath.ConfirmCompanyDetails);
    }

    // this monstrosity is required because Sonar complains that _logger.LogError isn't covered by a test.
    // it is covered, but Moq doesn't support verifying extension methods, so we have to verify the first
    // non-extension method in the LogError call stack, which Sonar isn't smart enough to interpret as covering LogError
    [ExcludeFromCodeCoverage]
    private void LogCompaniesHouseLookupFailed(Exception exception, string? companiesHouseNumber)
    {
        _logger.LogError(exception, "Companies House Lookup failed for {RegistrationNumber}",
            companiesHouseNumber?.Replace(Environment.NewLine, ""));
    }

    [HttpGet]
    [Route(PagePath.CannotVerifyOrganisation)]
    [OrganisationJourneyAccess(PagePath.CannotVerifyOrganisation)]
    public async Task<IActionResult> CannotVerifyOrganisation()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.CannotVerifyOrganisation);

        return View();
    }

    [HttpGet]
    [Route(PagePath.ConfirmCompanyDetails)]
    [OrganisationJourneyAccess(PagePath.ConfirmCompanyDetails)]
    public async Task<IActionResult> ConfirmCompanyDetails()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.ConfirmCompanyDetails);
        ViewBag.MakeChangesToYourLimitedCompanyLink = _urlOptions.MakeChangesToYourLimitedCompany;

        var viewModel = new ConfirmCompanyDetailsViewModel
        {
            CompanyName = session.ReExCompaniesHouseSession.Company.Name,
            CompaniesHouseNumber = session.ReExCompaniesHouseSession.Company.CompaniesHouseNumber,
            BusinessAddress = session.ReExCompaniesHouseSession.Company.BusinessAddress
        };

        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.ConfirmCompanyDetails)]
    [OrganisationJourneyAccess(PagePath.ConfirmCompanyDetails)]
    public async Task<IActionResult> ConfirmDetailsOfTheCompany()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var company = session.ReExCompaniesHouseSession.Company;

        if (company.AccountExists)
        {
            return await SaveSessionAndRedirect(session, nameof(AccountAlreadyExists), PagePath.ConfirmCompanyDetails,
                PagePath.AccountAlreadyExists);
        }

        session.Journey.RemoveAll(x => x == PagePath.AccountAlreadyExists);

        if (session.IsCompaniesHouseFlow && NationMapper.TryMapToNation(company.BusinessAddress.Country, out Nation nation) && nation != Nation.NotSet)
        {
            session!.UkNation = nation;
            if (!session.WhiteList.Contains(PagePath.UkNation))
            {
                session.WhiteList.Add(PagePath.UkNation);
            }
            return await SaveSessionAndRedirect(session, nameof(IsTradingNameDifferent), PagePath.ConfirmCompanyDetails, PagePath.IsTradingNameDifferent);
        }
        return await SaveSessionAndRedirect(session, nameof(UkNation), PagePath.ConfirmCompanyDetails, PagePath.UkNation);
    }

    [HttpGet]
    [Route(PagePath.AccountAlreadyExists)]
    [OrganisationJourneyAccess(PagePath.AccountAlreadyExists)]
    public async Task<IActionResult> AccountAlreadyExists()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.AccountAlreadyExists);

        return View(new AccountAlreadyExistsViewModel
        {
            DateCreated = session.ReExCompaniesHouseSession.Company.AccountCreatedOn.Value.Date
        });
    }

    [HttpGet]
    [Route(PagePath.UkNation)]
    [OrganisationJourneyAccess(PagePath.UkNation)]
    public async Task<IActionResult> UkNation()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.UkNation);

        var viewModel = new UkNationViewModel()
        {
            UkNation = session.UkNation,
            IsCompaniesHouseFlow = session.IsCompaniesHouseFlow,
            IsManualInputFlow = !session.IsCompaniesHouseFlow
        };
        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.UkNation)]
    [OrganisationJourneyAccess(PagePath.UkNation)]
    public async Task<IActionResult> UkNation(UkNationViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        model.IsCompaniesHouseFlow = session.IsCompaniesHouseFlow;

        if (!ModelState.IsValid)
        {
            if (model.UkNation == null)
            {
                var errorMessage = model.IsCompaniesHouseFlow ? "UkNation.LimitedCompany.ErrorMessage" : "UkNation.SoleTrader.ErrorMessage";
                ModelState.ClearValidationState(nameof(model.UkNation));
                ModelState.AddModelError(nameof(model.UkNation), errorMessage);
            }
            SetBackLink(session, PagePath.UkNation);
            return View(model);
        }
        session!.UkNation = model.UkNation;
        if (model.IsCompaniesHouseFlow)
        {
            return await SaveSessionAndRedirect(session, nameof(IsTradingNameDifferent), PagePath.UkNation, PagePath.IsTradingNameDifferent);
        }
        return await SaveSessionAndRedirect(session, nameof(BusinessAddress), PagePath.UkNation, PagePath.BusinessAddress);
    }

    [HttpGet]
    [Route(PagePath.NotAffected)]
    [OrganisationJourneyAccess(PagePath.NotAffected)]
    public async Task<IActionResult> NotAffected()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.NotAffected);

        return View();
    }

    [HttpGet]
    [Route(PagePath.BusinessAddress)]
    [OrganisationJourneyAccess(PagePath.BusinessAddress)]
    public async Task<IActionResult> BusinessAddress()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.BusinessAddress);

        var viewModel = new ReExBusinessAddressViewModel();

        if (session.ReExManualInputSession?.BusinessAddress?.IsManualAddress == true)
        {
            viewModel.BuildingNumber = session.ReExManualInputSession.BusinessAddress.BuildingNumber;
            viewModel.BuildingName = session.ReExManualInputSession.BusinessAddress.BuildingName;
            viewModel.Street = session.ReExManualInputSession.BusinessAddress.Street;
            viewModel.Town = session.ReExManualInputSession.BusinessAddress.Town;
            viewModel.County = session.ReExManualInputSession.BusinessAddress.County;
            viewModel.Postcode = session.ReExManualInputSession.BusinessAddress.Postcode;
        }

        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.BusinessAddress)]
    [OrganisationJourneyAccess(PagePath.BusinessAddress)]
    public async Task<IActionResult> BusinessAddress(ReExBusinessAddressViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.BusinessAddress);
            return View(model);
        }

        var address = session!.ReExManualInputSession!.BusinessAddress ??= new Address();

        address.BuildingNumber = model.BuildingNumber;
        address.BuildingName = model.BuildingName;
        address.Street = model.Street;
        address.Town = model.Town;
        address.County = model.County;
        address.Postcode = model.Postcode;
        address.IsManualAddress = true;

        if (session.ReExManualInputSession?.ProducerType == ProducerType.Partnership)
        {
            return await SaveSessionAndRedirect(session, nameof(LimitedPartnershipController), nameof(LimitedPartnershipController.NonCompaniesHousePartnershipType),
                PagePath.BusinessAddress, PagePath.NonCompaniesHousePartnershipType);
        }

        session.ReExManualInputSession.TypesOfPartner = null;
        if (session.OrganisationType == OrganisationType.NonCompaniesHouseCompany && session.ReExManualInputSession?.ProducerType == ProducerType.UnincorporatedBody)
        {
            return await SaveSessionAndRedirect(session, nameof(UnincorporatedRoleInOrganisation), PagePath.BusinessAddress, PagePath.UnincorporatedRoleInOrganisation);
        }
        return await SaveSessionAndRedirect(session, nameof(SoleTrader),
            PagePath.BusinessAddress, PagePath.SoleTrader);
    }

    [HttpGet]
    [Route(PagePath.SoleTrader)]
    [OrganisationJourneyAccess(PagePath.SoleTrader)]
    public async Task<IActionResult> SoleTrader()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.SoleTrader);

        var viewModel = new SoleTraderViewModel();

        //to-do: helper for this common code
        if (session.IsIndividualInCharge.HasValue)
        {
            viewModel.IsIndividualInCharge = session.IsIndividualInCharge == true ? YesNoAnswer.Yes : YesNoAnswer.No;
        }

        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.SoleTrader)]
    [OrganisationJourneyAccess(PagePath.SoleTrader)]
    public async Task<IActionResult> SoleTrader(SoleTraderViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.SoleTrader);
            return View(model);
        }

        session.IsIndividualInCharge = model.IsIndividualInCharge == YesNoAnswer.Yes;

        if (session.IsIndividualInCharge == true)
        {
            session.ReExManualInputSession.TeamMembers = null;
            session.InviteUserOption = InviteUserOptions.BeAnApprovedPerson;
            return await SaveSessionAndRedirect(session,
                controllerName: nameof(ApprovedPersonController),
                actionName: nameof(ApprovedPersonController.YouAreApprovedPersonSoleTrader),
                currentPagePath: PagePath.SoleTrader,
                nextPagePath: PagePath.YouAreApprovedPersonSoleTrader);
        }
        else
        {
            session.InviteUserOption = null;
        }

        //to-do: we skip to a later page here to handle out-of-order build, it will probably go to NotApprovedPerson
        return await SaveSessionAndRedirect(session, nameof(ApprovedPersonController), nameof(ApprovedPersonController.AddApprovedPerson),
            PagePath.SoleTrader, PagePath.AddAnApprovedPerson);
    }

    [HttpGet]
    [Route(PagePath.OrganisationName)]
    [OrganisationJourneyAccess(PagePath.OrganisationName)]
    public async Task<IActionResult> OrganisationName()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.OrganisationName);

        var viewModel = new OrganisationNameViewModel()
        {
            OrganisationName = session?.ReExManualInputSession?.OrganisationName,
        };
        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.OrganisationName)]
    [OrganisationJourneyAccess(PagePath.OrganisationName)]
    public async Task<IActionResult> OrganisationName(OrganisationNameViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.OrganisationName);

            return View(model);
        }

        session.ReExManualInputSession ??= new ReExManualInputSession();

        session.ReExManualInputSession.OrganisationName = model.OrganisationName!;

        return await SaveSessionAndRedirect(session, nameof(IsTradingNameDifferent), PagePath.OrganisationName,
            PagePath.IsTradingNameDifferent);
    }

    [HttpGet]
    [Route(PagePath.NonUkRoleInOrganisation)]
    [OrganisationJourneyAccess(PagePath.NonUkRoleInOrganisation)]
    public async Task<IActionResult> NonUkRoleInOrganisation()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.NonUkRoleInOrganisation);

        var viewModel = new NonUkRoleInOrganisationViewModel()
        {
            NonUkRoleInOrganisation = session.ReExManualInputSession?.NonUkRoleInOrganisation,
        };
        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.NonUkRoleInOrganisation)]
    [OrganisationJourneyAccess(PagePath.NonUkRoleInOrganisation)]
    public async Task<IActionResult> NonUkRoleInOrganisation(NonUkRoleInOrganisationViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.NonUkRoleInOrganisation);

            return View(model);
        }

        session.ReExManualInputSession ??= new ReExManualInputSession();

        session.ReExManualInputSession.NonUkRoleInOrganisation = model.NonUkRoleInOrganisation!;

        return await SaveSessionAndRedirect(session, nameof(ManageControl), PagePath.NonUkRoleInOrganisation,
            PagePath.ManageControl);
    }

    [HttpGet]
    [Route(PagePath.Declaration)]
    [OrganisationJourneyAccess(PagePath.Declaration)]
    public async Task<IActionResult> Declaration()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.Declaration);
        return View();
    }

    [HttpGet]
    [Route(PagePath.DeclarationContinue)]
    public async Task<IActionResult> DeclarationContinue()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        // Post related data
        var reExOrganisation = _reExAccountMapper.CreateReExOrganisationModel(session);
        await _facadeService.PostReprocessorExporterCreateOrganisationAsync(reExOrganisation, _serviceKeyOptions.ReprocessorExporter);

        return await SaveSessionAndRedirect(session, nameof(Success), PagePath.DeclarationContinue, PagePath.Success);
    }

    [HttpGet]
    [Route(PagePath.Success)]
    [OrganisationJourneyAccess(PagePath.Success)]
    public async Task<IActionResult> Success()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        var viewModel = new ReExOrganisationSuccessViewModel
        {
            IsCompaniesHouseFlow = session.IsCompaniesHouseFlow
        };

        if (viewModel.IsCompaniesHouseFlow)
        {
            var company = session.ReExCompaniesHouseSession?.Company;
            viewModel.CompanyName = company?.Name;
            viewModel.ReExCompanyTeamMembers = session.ReExCompaniesHouseSession?.TeamMembers;
        }
        else
        {
            var manualInput = session.ReExManualInputSession;
            viewModel.IsSoleTrader = manualInput?.ProducerType == ProducerType.SoleTrader;
            viewModel.CompanyName = manualInput?.OrganisationName;
            viewModel.ReExCompanyTeamMembers = manualInput?.TeamMembers;
            viewModel.IsAnApprovedPerson = session.InviteUserOption == InviteUserOptions.BeAnApprovedPerson;
        }

        return View(viewModel);
    }

    [HttpGet]
    [Route(PagePath.NotImplemented)]
    [OrganisationJourneyAccess(PagePath.NotImplemented)]
    [ExcludeFromCodeCoverage]
    public IActionResult NotImplemented()
    {
        return View();
    }

    #region Private Methods

    private static ModelStateDictionary DeserializeModelState(string serializedModelState)
    {
        var errorList = JsonSerializer.Deserialize<Dictionary<string, string[]>>(serializedModelState);
        var modelState = new ModelStateDictionary();

        foreach (var kvp in errorList)
        {
            foreach (var error in kvp.Value)
            {
                modelState.AddModelError(kvp.Key, error);
            }
        }

        return modelState;
    }

    private static string SerializeModelState(ModelStateDictionary modelState)
    {
        var errorList = modelState.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
        );

        return JsonSerializer.Serialize(errorList);
    }

    #endregion Private Methods
}