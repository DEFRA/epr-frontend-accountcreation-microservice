using FrontendAccountCreation.Core.Services;
using FrontendAccountCreation.Core.Services.Dto.Company;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Controllers.Errors;
using FrontendAccountCreation.Web.Sessions;
using FrontendAccountCreation.Web.ViewModels;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.Identity.Web;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter;

[Feature(FeatureFlags.AddOrganisationCompanyHouseDirectorJourney)]
[Route("re-ex/organisation")]
public class OrganisationController : ControllerBase<OrganisationSession>
{
    private readonly ISessionManager<OrganisationSession> _sessionManager;
    private readonly IFacadeService _facadeService;
    private readonly IReExAccountMapper _reExAccountMapper;
    private readonly ILogger<OrganisationController> _logger;
    private readonly ExternalUrlsOptions _urlOptions;
    private readonly DeploymentRoleOptions _deploymentRoleOptions;
    private readonly ServiceKeysOptions _serviceKeyOptions;

    public OrganisationController(
         ISessionManager<OrganisationSession> sessionManager,
         IFacadeService facadeService,
         IReExAccountMapper reExAccountMapper,
         IMultipleOptions multipleOptions,
         IOptions<DeploymentRoleOptions> deploymentRoleOptions,
         ILogger<OrganisationController> logger) : base(sessionManager)
    {
        _sessionManager = sessionManager;
        _facadeService = facadeService;
        _reExAccountMapper = reExAccountMapper;
        _deploymentRoleOptions = deploymentRoleOptions.Value;
        _urlOptions = multipleOptions.UrlOptions;
        _serviceKeyOptions = multipleOptions.ServiceKeysOptions;
        _logger = logger;
    }

    [HttpGet]
    [Route("")]
    [AuthorizeForScopes(ScopeKeySection = ConfigKeys.FacadeScope)]
    [Route(PagePath.RegisteredAsCharity)]
    public async Task<IActionResult> RegisteredAsCharity()
    {
        if (_deploymentRoleOptions.IsRegulator())
        {
            return RedirectToAction(nameof(ErrorController.ErrorReEx), nameof(ErrorController).Replace("Controller", ""), new
            {
                statusCode = (int)HttpStatusCode.Forbidden
            });
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        YesNoAnswer? isTheOrganisationCharity = null;

        if (session?.IsTheOrganisationCharity.HasValue == true)
        {
            isTheOrganisationCharity = session.IsTheOrganisationCharity == true ? YesNoAnswer.Yes : YesNoAnswer.No;
        }

        return View(new RegisteredAsCharityRequestViewModel
        {
            isTheOrganisationCharity = isTheOrganisationCharity
        });
    }

    [HttpPost]
    [Route(PagePath.RegisteredAsCharity)]
    public async Task<IActionResult> RegisteredAsCharity(RegisteredAsCharityRequestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session)
            ?? new OrganisationSession()
            {
                Journey = [PagePath.RegisteredAsCharity]
            };

        session.IsTheOrganisationCharity = model.isTheOrganisationCharity == YesNoAnswer.Yes;

        if (session.IsTheOrganisationCharity.Value)
        {
            return await SaveSessionAndRedirect(session, nameof(NotAffected), PagePath.RegisteredAsCharity, PagePath.NotAffected);
        }
        else
        {
            return await SaveSessionAndRedirect(session, nameof(RegisteredWithCompaniesHouse), PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse);
        }
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
            return await SaveSessionAndRedirect(session, nameof(CompaniesHouseNumber),
                PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber);
        }

        if (await featureManager.IsEnabledAsync(FeatureFlags.AddOrganisationSoleTraderJourney))
        {
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

        if (session.IsUkMainAddress != true)
        {
            return await SaveSessionAndRedirect(session, nameof(NotImplemented),
                PagePath.IsUkMainAddress, PagePath.NotImplemented);
        }

        return await SaveSessionAndRedirect(session, nameof(TradingName), PagePath.IsUkMainAddress,
            PagePath.TradingName);
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
            IsTradingNameDifferent = isTradingNameDifferent
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

        if (session.IsTradingNameDifferent == true)
        {
            return await SaveSessionAndRedirect(session, nameof(TradingName), PagePath.IsTradingNameDifferent, PagePath.TradingName);
        }
        return await SaveSessionAndRedirect(session, nameof(IsOrganisationAPartner), PagePath.IsTradingNameDifferent, PagePath.IsPartnership);
    }

    [HttpGet]
    [Route(PagePath.TradingName)]
    [OrganisationJourneyAccess(PagePath.TradingName)]
    public async Task<IActionResult> TradingName()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.TradingName);

        var viewModel = new TradingNameViewModel()
        {
            TradingName = session?.ReExManualInputSession?.TradingName,
        };
        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.TradingName)]
    [OrganisationJourneyAccess(PagePath.TradingName)]
    public async Task<IActionResult> TradingName(TradingNameViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.TradingName);

            return View(model);
        }

        session.ReExManualInputSession ??= new ReExManualInputSession();

        session.ReExManualInputSession.TradingName = model.TradingName!;

        if (session.IsCompaniesHouseFlow)
        {
            return await SaveSessionAndRedirect(session, nameof(IsOrganisationAPartner), PagePath.TradingName,
                PagePath.IsPartnership);
        }
        return await SaveSessionAndRedirect(session, nameof(TypeOfOrganisation), PagePath.TradingName,
            PagePath.TypeOfOrganisation);
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

        session.IsOrganisationAPartnership = model.IsOrganisationAPartner == YesNoAnswer.Yes;

        if (session.IsOrganisationAPartnership == true)
        {
            // TODO: No option ending up same YES pagePath - to be confirmed
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
    [AuthorizeForScopes(ScopeKeySection = ConfigKeys.FacadeScope)]
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

        if (session.ReExCompaniesHouseSession == null)
        {
            session.ReExCompaniesHouseSession = new ReExCompaniesHouseSession();
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
        else
        {
            return await SaveSessionAndRedirect(session, nameof(BusinessAddress), PagePath.UkNation, PagePath.BusinessAddress);
        }
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

    [ExcludeFromCodeCoverage]
    [HttpGet]
    [Route(PagePath.BusinessAddress)]
    [OrganisationJourneyAccess(PagePath.BusinessAddress)]
    public Task<IActionResult> BusinessAddress()
    {
        return PlaceholderPageGet(PagePath.BusinessAddress, true);
    }

    [ExcludeFromCodeCoverage]
    [HttpPost]
    [Route(PagePath.BusinessAddress)]
    [OrganisationJourneyAccess(PagePath.BusinessAddress)]
    public Task<IActionResult> BusinessAddressPost()
    {
        return PlaceholderPagePost(nameof(SoleTrader), PagePath.BusinessAddress, PagePath.SoleTrader);
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
            return await SaveSessionAndRedirect(session,
                controllerName: nameof(ApprovedPersonController),
                actionName: nameof(ApprovedPersonController.YouAreApprovedPersonSoleTrader),
                currentPagePath: PagePath.SoleTrader, 
                nextPagePath: PagePath.YouAreApprovedPersonSoleTrader);
        }

        //to-do: we skip to a later page here to handle out-of-order build, it will probably go to NotApprovedPerson
        return await SaveSessionAndRedirect(session, nameof(ApprovedPersonController), nameof(ApprovedPersonController.SoleTraderTeamMemberDetails),
            PagePath.SoleTrader, PagePath.SoleTraderTeamMemberDetails);
    }

    //to-do: is not-approved-person page actually a form of the approved person page?

    [ExcludeFromCodeCoverage]
    [HttpGet]
    [Route(PagePath.NotApprovedPerson)]
    [OrganisationJourneyAccess(PagePath.NotApprovedPerson)]
    public Task<IActionResult> NotApprovedPerson()
    {
        return PlaceholderPageGet(PagePath.NotApprovedPerson);
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
            CompanyName = session.ReExCompaniesHouseSession.Company.Name,
            reExCompanyTeamMembers = session.ReExCompaniesHouseSession?.TeamMembers
        };

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

    public IActionResult RedirectToStart()
    {
        return RedirectToAction(nameof(RegisteredAsCharity));
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