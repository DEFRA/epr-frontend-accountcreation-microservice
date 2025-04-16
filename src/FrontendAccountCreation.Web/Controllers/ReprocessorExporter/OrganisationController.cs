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
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter;

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

    [HttpGet]
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

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);


        YesNoAnswer? isTheOrganisationCharity = null;

        if (session != null)
        {
            isTheOrganisationCharity = session.IsTheOrganisationCharity ? YesNoAnswer.Yes : YesNoAnswer.No;

            if (session.IsUserChangingDetails)
            {
                SetBackLink(session, string.Empty);
            }
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

        if (session.IsTheOrganisationCharity)
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
            return await SaveSessionAndRedirect(session, nameof(TradingName), PagePath.IsTradingNameDifferent, PagePath.TradingName2);
        }
        return await SaveSessionAndRedirect(session, nameof(IsPartnership), PagePath.IsTradingNameDifferent, PagePath.IsPartnership);
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
            ProducerType = session.ManualInputSession?.ProducerType
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

        session.ManualInputSession ??= new ReExManualInputSession();
        session.ManualInputSession.ProducerType = model.ProducerType;
        session.CompaniesHouseSession = null;

        return await SaveSessionAndRedirect(session, nameof(TradingName), PagePath.TypeOfOrganisation,
            PagePath.TradingName);
    }

    [HttpGet]
    [Route(PagePath.TradingName2)]
    [OrganisationJourneyAccess(PagePath.TradingName2)]
    public async Task<IActionResult> TradingName()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.TradingName);

        var viewModel = new TradingNameViewModel()
        {
            TradingName = session?.ManualInputSession?.TradingName,
        };
        return View(viewModel);
    }

    [HttpGet]
    [Route(PagePath.IsPartnership)]
    [OrganisationJourneyAccess(PagePath.IsPartnership)]
    [ExcludeFromCodeCoverage]
    public Task<IActionResult> IsPartnership()
    {
        throw new NotImplementedException(
            "The 'Is your organisation a partnership' page hasn't been built. It will be built in a future story.");
    }

    [HttpGet]
    [Route(PagePath.CompaniesHouseNumber)]
    [OrganisationJourneyAccess(PagePath.CompaniesHouseNumber)]
    public async Task<IActionResult> CompaniesHouseNumber()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.CompaniesHouseNumber);

        ViewBag.FindAndUpdateCompanyInformationLink = _urlOptions.FindAndUpdateCompanyInformation;

        var viewModel = new CompaniesHouseNumberViewModel
        {
            CompaniesHouseNumber = session.CompaniesHouseSession?.Company?.CompaniesHouseNumber,
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
    [OrganisationJourneyAccess(PagePath.CompaniesHouseNumber)]
    public async Task<IActionResult> CompaniesHouseNumber(CompaniesHouseNumberViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.CompaniesHouseNumber);

            ViewBag.FindAndUpdateCompanyInformationLink = _urlOptions.FindAndUpdateCompanyInformation;

            return View(model);
        }

        if (session.CompaniesHouseSession == null)
        {
            session.CompaniesHouseSession = new ReExCompaniesHouseSession();
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
            ModelState.AddModelError(nameof(CompaniesHouseNumberViewModel.CompaniesHouseNumber), "CompaniesHouseNumber.NotFoundError");

            SetBackLink(session, PagePath.CompaniesHouseNumber);

            ViewBag.FindAndUpdateCompanyInformationLink = _urlOptions.FindAndUpdateCompanyInformation;
            TempData["ModelState"] = SerializeModelState(ModelState);
            TempData["CompaniesHouseNumber"] = model.CompaniesHouseNumber;

            return RedirectToAction(nameof(CompaniesHouseNumber));
        }

        session.CompaniesHouseSession.Company = company;

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
            CompanyName = session.CompaniesHouseSession.Company.Name,
            CompaniesHouseNumber = session.CompaniesHouseSession.Company.CompaniesHouseNumber,
            BusinessAddress = session.CompaniesHouseSession.Company.BusinessAddress
        };

        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.ConfirmCompanyDetails)]
    [OrganisationJourneyAccess(PagePath.ConfirmCompanyDetails)]
    public async Task<IActionResult> ConfirmDetailsOfTheCompany()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (session.CompaniesHouseSession.Company.AccountExists)
        {
            return await SaveSessionAndRedirect(session, nameof(AccountAlreadyExists), PagePath.ConfirmCompanyDetails,
                PagePath.AccountAlreadyExists);
        }

        session.Journey.RemoveAll(x => x == PagePath.AccountAlreadyExists);

        return await SaveSessionAndRedirect(session, nameof(UkNation), PagePath.ConfirmCompanyDetails, PagePath.UkNation);
    }

    [ExcludeFromCodeCoverage]
    [HttpGet]
    [Route(PagePath.UkNation)]
    [OrganisationJourneyAccess(PagePath.UkNation)]
    public IActionResult UkNation()
    {
        return RedirectToAction(nameof(IsTradingNameDifferent));

        //throw new NotImplementedException(
        //    "The nation page isn't implemented yet and will be implemented in a later story");
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
            DateCreated = session.CompaniesHouseSession.Company.AccountCreatedOn.Value.Date
        });
    }

    [HttpGet]
    [Route(PagePath.NotAffected)]
    [OrganisationJourneyAccess(PagePath.NotAffected)]
    public async Task<IActionResult> NotAffected()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new OrganisationSession();

        SetBackLink(session, PagePath.NotAffected);

        return View();
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
