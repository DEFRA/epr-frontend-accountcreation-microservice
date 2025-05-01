using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using FrontendAccountCreation;
using FrontendAccountCreation.Core.Extensions;
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
using Microsoft.Identity.Web;

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter;

[Feature(FeatureFlags.AddOrganisationCompanyHouseDirectorJourney)]
[Route("re-ex/organisation")]
public class OrganisationController : Controller
{
    private readonly ISessionManager<OrganisationSession> _sessionManager;
    private readonly IFacadeService _facadeService;
    private readonly ICompanyService _companyService;
    private readonly IOrganisationMapper _organisationMapper;
    private readonly ILogger<OrganisationController> _logger;
    private readonly ExternalUrlsOptions _urlOptions;
    private readonly DeploymentRoleOptions _deploymentRoleOptions;

    public OrganisationController(
         ISessionManager<OrganisationSession> sessionManager,
         IFacadeService facadeService,
         ICompanyService companyService,
         IOrganisationMapper organisationMapper,
         IOptions<ExternalUrlsOptions> urlOptions,
         IOptions<DeploymentRoleOptions> deploymentRoleOptions,
         ILogger<OrganisationController> logger)
    {
        _sessionManager = sessionManager;
        _facadeService = facadeService;
        _companyService = companyService;
        _organisationMapper = organisationMapper;
        _urlOptions = urlOptions.Value;
        _deploymentRoleOptions = deploymentRoleOptions.Value;
        _logger = logger;
    }

    //todo: how do we handle feature flag for first page? manually?

    [HttpGet]
    [Route("")]
    [AuthorizeForScopes(ScopeKeySection = ConfigKeys.FacadeScope)]
    [Route(PagePath.RegisteredAsCharity)]
    public async Task<IActionResult> RegisteredAsCharity()
    {
        if (_deploymentRoleOptions.IsRegulator())
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new
            {
                statusCode = (int)HttpStatusCode.Forbidden
            });
        }

        //todo: the account will already exist, so I don't think this check is wanted
        // in fact, we probably want to check that the account *does* already exist
        var userExists = await _facadeService.DoesAccountAlreadyExistAsync();
        if (userExists)
        {
            if (string.IsNullOrEmpty(_urlOptions.ExistingUserRedirectUrl))
            {
                return RedirectToAction("UserAlreadyExists", "Home");
            }
            else
            {
                return Redirect(_urlOptions.ExistingUserRedirectUrl);
            }
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new OrganisationSession()
        {
            Journey = [PagePath.RegisteredAsCharity]
        };

        YesNoAnswer? isTheOrganisationCharity = null;

        if (session.IsTheOrganisationCharity.HasValue)
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

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new OrganisationSession()
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
    public async Task<IActionResult> RegisteredWithCompaniesHouse(RegisteredWithCompaniesHouseViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new OrganisationSession()
        {
            Journey = [PagePath.RegisteredWithCompaniesHouse]
        };
        
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
            return await SaveSessionAndRedirect(session, nameof(CompaniesHouseNumber), PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber);
        }
        else
        {
            return await SaveSessionAndRedirect(session, nameof(TypeOfOrganisation), PagePath.RegisteredWithCompaniesHouse, PagePath.TypeOfOrganisation);
        }
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
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

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

        return await SaveSessionAndRedirect(session, nameof(PartnerOrganisation), PagePath.TradingName,
            PagePath.PartnerOrganisation);
    }

    [ExcludeFromCodeCoverage]
    [HttpGet]
    [Route(PagePath.PartnerOrganisation)]
    [OrganisationJourneyAccess(PagePath.PartnerOrganisation)]
    public Task<IActionResult> PartnerOrganisation()
    {
        throw new NotImplementedException(
            "The 'partner organisation' page hasn't been built. It will be built in a future story.");
    }

    [HttpGet]
    [Route(PagePath.TypeOfOrganisation)]
    [OrganisationJourneyAccess(PagePath.TypeOfOrganisation)]
    public async Task<IActionResult> TypeOfOrganisation()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.TypeOfOrganisation);

        var viewModel = new TypeOfOrganisationViewModel()
        {
            ProducerType = session.ReExManualInputSession?.ProducerType
        };

        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.TypeOfOrganisation)]
    [OrganisationJourneyAccess(PagePath.TypeOfOrganisation)]
    public async Task<IActionResult> TypeOfOrganisation(TypeOfOrganisationViewModel model)
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

        return await SaveSessionAndRedirect(session, nameof(TradingName), PagePath.TypeOfOrganisation,
            PagePath.RoleInOrganisation);
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
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);            
        session.IsOrganisationAPartnership = model.IsOrganisationAPartner == YesNoAnswer.Yes;

        if (session.IsOrganisationAPartnership == true)
        {
            // TODO: Yes or No ending up same pagePath - to be confirmed
            return await SaveSessionAndRedirect(session, nameof(RoleInOrganisation), PagePath.IsPartnership, PagePath.RoleInOrganisation);
        }
        return await SaveSessionAndRedirect(session, nameof(RoleInOrganisation), PagePath.IsPartnership, PagePath.RoleInOrganisation);
    }

    [HttpGet]
    [Route(PagePath.RoleInOrganisation)]
    [OrganisationJourneyAccess(PagePath.RoleInOrganisation)]
    public async Task<IActionResult> RoleInOrganisation()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.TypeOfOrganisation);

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

        if (model.RoleInOrganisation == Core.Sessions.RoleInOrganisation.NoneOfTheAbove)
        {
            return await SaveSessionAndRedirect(session, "CannotCreateAccount", PagePath.RoleInOrganisation,
                PagePath.CannotCreateAccount);
        }

        return await SaveSessionAndRedirect(session, nameof(AddApprovedPerson), PagePath.RoleInOrganisation,
                PagePath.ManageAccountPerson);
    }

    [ExcludeFromCodeCoverage(Justification = "The 'Manage Account Person' page hasn't been built. It will be built in a future story.")]
    [HttpGet]
    [Route(PagePath.ManageAccountPerson)]
    [OrganisationJourneyAccess(PagePath.ManageAccountPerson)]
    public async Task<IActionResult> AddApprovedPerson()
    {
        throw new NotImplementedException(
            "The 'Manage Account Person' page hasn't been built. It will be built in a future story.");
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
            _logger.LogError(exception, "Companies House Lookup failed for {RegistrationNumber}", model.CompaniesHouseNumber);

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

        if (session.ReExCompaniesHouseSession.Company.AccountExists)
        {
            return await SaveSessionAndRedirect(session, nameof(AccountAlreadyExists), PagePath.ConfirmCompanyDetails,
                PagePath.AccountAlreadyExists);
        }

        session.Journey.RemoveAll(x => x == PagePath.AccountAlreadyExists);

        return await SaveSessionAndRedirect(session, nameof(UkNation), PagePath.ConfirmCompanyDetails, PagePath.UkNation);
    }

    [ExcludeFromCodeCoverage]
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
            IsCompaniesHouseFlow = session.IsCompaniesHouseFlow
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
        return await SaveSessionAndRedirect(session, nameof(IsTradingNameDifferent), PagePath.UkNation, PagePath.IsTradingNameDifferent);
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
    [Route(PagePath.YouAreApprovedPerson)]
    [OrganisationJourneyAccess(PagePath.YouAreApprovedPerson)]
    public async Task<IActionResult> YouAreApprovedPerson()
    {
        return View();
    }

    [HttpGet]
    [Route(PagePath.ApprovedPersonContinue)]
    public async Task<IActionResult> ApprovedConfirmationContinue()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        await SaveSessionAndRedirect(session, nameof(NotImplementedMethod), PagePath.YouAreApprovedPerson, "ToBeAdded");
        return Ok();
    }

    public void NotImplementedMethod()
    {
        // TO DO following & modify - once Tungsten has merged
        throw new NotImplementedException("not been implemented yet...as no related user-story has been confirmed!");
    }

    [HttpGet]
    [Route(PagePath.AddApprovedPerson)]
    public IActionResult InviteOtherApprovedPerson()
    {
        // TO DO following & modify - once Tungsten has merged
        return Ok("not been implemented yet...WIP by Tungsten team.");
    }

    public IActionResult RedirectToStart()
    {
        return RedirectToAction(nameof(RegisteredAsCharity));
    }

    #region Private Methods 

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

    #endregion

}
