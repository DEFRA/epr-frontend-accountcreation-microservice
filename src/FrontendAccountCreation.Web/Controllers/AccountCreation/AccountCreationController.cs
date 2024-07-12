namespace FrontendAccountCreation.Web.Controllers.AccountCreation;

using System.Net;
using System.Security.Claims;
using Attributes;
using Configs;
using Constants;
using Core.Addresses;
using Core.Constants;
using Core.Exceptions;
using Core.Extensions;
using Core.Services;
using Core.Services.Dto.Company;
using Core.Services.FacadeModels;
using Core.Sessions;
using Errors;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Sessions;
using ViewModels;
using ViewModels.AccountCreation;

public class AccountCreationController : Controller
{
    private const string PostcodeLookupFailedKey = "PostcodeLookupFailed";
    private const string OrganisationMetaDataKey = "OrganisationMetaData";

    private readonly ISessionManager<AccountCreationSession> _sessionManager;
    private readonly IFacadeService _facadeService;
    private readonly ICompanyService _companyService;
    private readonly IAccountMapper _accountMapper;
    private readonly ILogger<AccountCreationController> _logger;
    private readonly ExternalUrlsOptions _urlOptions;
    private readonly DeploymentRoleOptions _deploymentRoleOptions;

    public AccountCreationController(
        ISessionManager<AccountCreationSession> sessionManager,
        IFacadeService facadeService,
        ICompanyService companyService,
        IAccountMapper accountMapper,
        IOptions<ExternalUrlsOptions> urlOptions,
        IOptions<DeploymentRoleOptions> deploymentRoleOptions,
        ILogger<AccountCreationController> logger)
    {
        _sessionManager = sessionManager;
        _facadeService = facadeService;
        _companyService = companyService;
        _accountMapper = accountMapper;
        _urlOptions = urlOptions.Value;
        _deploymentRoleOptions = deploymentRoleOptions.Value;
        _logger = logger;
    }

    [HttpGet]
    [AuthorizeForScopes(ScopeKeySection = ConfigKeys.FacadeScope)]
    [Route("")]
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

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new AccountCreationSession()
        {
            Journey = new List<string> { PagePath.RegisteredAsCharity }
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
    [JourneyAccess(PagePath.RegisteredWithCompaniesHouse)]
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
    [JourneyAccess(PagePath.RegisteredWithCompaniesHouse)]
    public async Task<IActionResult> RegisteredWithCompaniesHouse(RegisteredWithCompaniesHouseViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new AccountCreationSession()
        {
            Journey = new List<string> { PagePath.RegisteredWithCompaniesHouse }
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
    [Route(PagePath.TypeOfOrganisation)]
    [JourneyAccess(PagePath.TypeOfOrganisation)]
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
    [JourneyAccess(PagePath.TypeOfOrganisation)]
    public async Task<IActionResult> TypeOfOrganisation(TypeOfOrganisationViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.TypeOfOrganisation);
            return View(model);
        }

        session.ManualInputSession ??= new ManualInputSession();
        session.ManualInputSession.ProducerType = model.ProducerType;
        session.CompaniesHouseSession = null;

        return await SaveSessionAndRedirect(session, nameof(TradingName), PagePath.TypeOfOrganisation,
            PagePath.TradingName);
    }

    [HttpGet]
    [Route(PagePath.CompaniesHouseNumber)]
    [JourneyAccess(PagePath.CompaniesHouseNumber)]
    public async Task<IActionResult> CompaniesHouseNumber()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.CompaniesHouseNumber);

        ViewBag.FindAndUpdateCompanyInformationLink = _urlOptions.FindAndUpdateCompanyInformation;

        var viewModel = new CompaniesHouseNumberViewModel
        {
            CompaniesHouseNumber = session.CompaniesHouseSession?.Company?.CompaniesHouseNumber,
        };

        return View(viewModel);
    }

    [HttpPost]
    [AuthorizeForScopes(ScopeKeySection = ConfigKeys.FacadeScope)]
    [Route(PagePath.CompaniesHouseNumber)]
    [JourneyAccess(PagePath.CompaniesHouseNumber)]
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
            session.CompaniesHouseSession = new CompaniesHouseSession();
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

            return View(model);
        }

        session.CompaniesHouseSession.Company = company;

        return await SaveSessionAndRedirect(session, nameof(ConfirmCompanyDetails), PagePath.CompaniesHouseNumber, PagePath.ConfirmCompanyDetails);
    }

    [HttpGet]
    [Route(PagePath.ConfirmCompanyDetails)]
    [JourneyAccess(PagePath.ConfirmCompanyDetails)]
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
    [JourneyAccess(PagePath.ConfirmCompanyDetails)]
    public async Task<IActionResult> ConfirmDetailsOfTheCompany()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        session.CompaniesHouseSession.IsComplianceScheme = _companyService.IsComplianceScheme(session.CompaniesHouseSession.Company.CompaniesHouseNumber);

        if (!session.CompaniesHouseSession.Company.AccountExists)
        {
            session.Journey.RemoveAll(x => x == PagePath.AccountAlreadyExists);
            TempData.Remove(OrganisationMetaDataKey);

            if (session.CompaniesHouseSession.IsComplianceScheme)
            {
                return await SaveSessionAndRedirect(session, nameof(RoleInOrganisation), PagePath.ConfirmCompanyDetails, PagePath.RoleInOrganisation);
            }
            return await SaveSessionAndRedirect(session, nameof(UkNation), PagePath.ConfirmCompanyDetails, PagePath.UkNation);
        }

        return await SaveSessionAndRedirect(session, nameof(AccountAlreadyExists), PagePath.ConfirmCompanyDetails, PagePath.AccountAlreadyExists);
    }

    [HttpGet]
    [Route(PagePath.UkNation)]
    [JourneyAccess(PagePath.UkNation)]
    public async Task<IActionResult> UkNation()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.UkNation);

        var viewModel = new UkNationViewModel()
        {
            UkNation = session.UkNation,
            IsCompaniesHouseFlow = session.IsCompaniesHouseFlow,
            IsManualInputFlow = session.IsManualInputFlow
        };
        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.UkNation)]
    [JourneyAccess(PagePath.UkNation)]
    public async Task<IActionResult> UkNation(UkNationViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        model.IsCompaniesHouseFlow = session.IsCompaniesHouseFlow;
        model.IsManualInputFlow = session.IsManualInputFlow;

        ProducerType? producerType = null;

        if (session.IsManualInputFlow && session.ManualInputSession.ProducerType is { })
        {
            producerType = session.ManualInputSession.ProducerType.Value;
        }
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
            return await SaveSessionAndRedirect(session, nameof(RoleInOrganisation), PagePath.UkNation, PagePath.RoleInOrganisation);
        }
        if (model.IsManualInputFlow && producerType != ProducerType.SoleTrader)
        {
            return await SaveSessionAndRedirect(session, nameof(ManualInputRoleInOrganisation), PagePath.UkNation, PagePath.ManualInputRoleInOrganisation);
        }
        return await SaveSessionAndRedirect(session, nameof(FullName), PagePath.UkNation, PagePath.FullName);
    }

    [HttpGet]
    [Route(PagePath.CannotVerifyOrganisation)]
    [JourneyAccess(PagePath.CannotVerifyOrganisation)]
    public async Task<IActionResult> CannotVerifyOrganisation()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.CannotVerifyOrganisation);

        return View();
    }

    [HttpGet]
    [Route(PagePath.RoleInOrganisation)]
    public async Task<IActionResult> RoleInOrganisation()
    {
        var inviteTokenTempData = TempData["InviteToken"] as string;
        var invitedOrganisationIdTempData = TempData["InvitedOrganisationId"] as string;
        var invitedCompanyHouseNumber = TempData["InvitedCompanyHouseNumber"] as string;

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new AccountCreationSession();

        if (!string.IsNullOrWhiteSpace(inviteTokenTempData))
        {
            TempData["InvitedOrganisationId"] = null;
            session.IsApprovedUser = true;
            session.InviteToken = inviteTokenTempData;

            var organisationData = await _facadeService.GetOrganisationNameByInviteTokenAsync(inviteTokenTempData);
            session.OrganisationType = OrganisationType.CompaniesHouseCompany;

            session.CompaniesHouseSession = new CompaniesHouseSession
            {
                IsComplianceScheme = true
            };

            session.CompaniesHouseSession.Company = new Company
            {
                Name = organisationData.OrganisationName,
                CompaniesHouseNumber = invitedCompanyHouseNumber,
                OrganisationId = invitedOrganisationIdTempData
            };
            session.CompaniesHouseSession.Company.BusinessAddress = new Address
            {
                SubBuildingName = organisationData.SubBuildingName,
                BuildingName = organisationData.BuildingName,
                BuildingNumber = organisationData.BuildingNumber,
                Postcode = organisationData.Postcode,
                Town = organisationData.Town,
                County = organisationData.County,
                Country = organisationData.Country,
                Street = organisationData.Street
            };
        }

        SetBackLink(session, PagePath.RoleInOrganisation);
        _sessionManager.SaveSessionAsync(HttpContext.Session, session);

        var viewModel = new RoleInOrganisationViewModel()
        {
            RoleInOrganisation = session.CompaniesHouseSession?.RoleInOrganisation
        };

        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.RoleInOrganisation)]
    [AuthorizeForScopes(ScopeKeySection = ConfigKeys.FacadeScope)]
    public async Task<IActionResult> RoleInOrganisation(RoleInOrganisationViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.RoleInOrganisation);

            return View(model);
        }

        if (session.CompaniesHouseSession == null)
        {
            CompaniesHouseSession companiesHouseSession = new CompaniesHouseSession();
            session.CompaniesHouseSession = companiesHouseSession;
        }
        session.CompaniesHouseSession.RoleInOrganisation = model.RoleInOrganisation.Value;

        if (model.RoleInOrganisation == Core.Sessions.RoleInOrganisation.NoneOfTheAbove)
        {
            return await SaveSessionAndRedirect(session, nameof(CannotCreateAccount), PagePath.RoleInOrganisation,
                PagePath.CannotCreateAccount);
        }

        return await SaveSessionAndRedirect(session, nameof(FullName), PagePath.RoleInOrganisation,
                PagePath.FullName);
    }

    [HttpGet]
    [Route($"{PagePath.Invitation}/{{inviteToken}}")]
    public async Task<RedirectToActionResult> Invitation(string inviteToken)
    {
        TempData["InviteToken"] = inviteToken;

        var invitedApprovedUser = await _facadeService.GetServiceRoleIdAsync(inviteToken);

        if (invitedApprovedUser.IsInvitationTokenInvalid) 
        {
            return RedirectToAction(nameof(InvalidToken));
            
        }

        if (invitedApprovedUser.ServiceRoleId == "1")
        {
            TempData["InvitedOrganisationId"] = invitedApprovedUser.OrganisationId;
            if (!string.IsNullOrEmpty(invitedApprovedUser.CompanyHouseNumber))
            {
                TempData["InvitedCompanyHouseNumber"] = invitedApprovedUser.CompanyHouseNumber;
                return RedirectToAction(nameof(RoleInOrganisation));
            }
            else
            {
                return RedirectToAction(nameof(ManualInputRoleInOrganisation));
            }
        }

        return RedirectToAction(nameof(InviteeFullName));
    }
    
    [HttpGet]
    public IActionResult InvalidToken() {
        var callbackUrl = Url.Action(action: "SignedOutInvalidToken", controller: "Home", values: null, protocol: Request.Scheme);
        return SignOut(
             new AuthenticationProperties()
             {
                 RedirectUri = callbackUrl
             }
             ,
             CookieAuthenticationDefaults.AuthenticationScheme,
             OpenIdConnectDefaults.AuthenticationScheme);
    }


    [HttpGet]
    [Route(PagePath.InviteeFullName)]
    public async Task<IActionResult> InviteeFullName()
    {
        var inviteTokenTempData = TempData["InviteToken"] as string;
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new AccountCreationSession();

        if (!string.IsNullOrEmpty(inviteTokenTempData))
        {
            session.InviteToken = inviteTokenTempData;
            _sessionManager.SaveSessionAsync(HttpContext.Session, session);
        }

        var userAccount = await _facadeService.GetUserAccount();

        if (userAccount is not null && userAccount.User.EnrolmentStatus != EnrolmentStatus.Invited)
        {
            _logger.LogInformation("User with ID {UserId} does not have an enrolment status of \"Invited\".", User.GetObjectId());
            return Redirect(_urlOptions.ReportDataRedirectUrl);
        }

        if (string.IsNullOrEmpty(session?.InviteToken))
        {
            return RedirectToAction("Error", "Error");
        }

        return View(nameof(FullName), new FullNameViewModel
        {
            PostAction = nameof(InviteeFullName)
        });
    }

    [HttpPost]
    [AuthorizeForScopes(ScopeKeySection = ConfigKeys.FacadeScope)]
    [Route(PagePath.InviteeFullName)]
    public async Task<IActionResult> InviteeFullName(FullNameViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (string.IsNullOrEmpty(session?.InviteToken))
        {
            ModelState.AddModelError(nameof(InviteeFullName), $"No invite token found in session for user {User.GetObjectId()}");
        }

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.InviteeFullName);

            return View(nameof(FullName), model);
        }

        var inviteeDetails = new EnrolInvitedUserModel
        {
            InviteToken = session?.InviteToken,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        await _facadeService.PostEnrolInvitedUserAsync(inviteeDetails);

        await SaveSession(session, PagePath.InviteeFullName, null);

        return Redirect(_urlOptions.ReportDataRedirectUrl);
    }

    [HttpGet]
    [Route(PagePath.ManualInputRoleInOrganisation)]
    public async Task<IActionResult> ManualInputRoleInOrganisation()
    {
        var inviteTokenTempData = TempData["InviteToken"] as string;
        var invitedOrganisationIdTempData = TempData["InvitedOrganisationId"] as string;
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new AccountCreationSession();

        if (!string.IsNullOrWhiteSpace(inviteTokenTempData))
        {
            TempData["InvitedOrganisationId"] = null;
            session.IsApprovedUser = true;
            session.InviteToken = inviteTokenTempData;
            session.ManualInputSession = new ManualInputSession();
            var organisationData = await _facadeService.GetOrganisationNameByInviteTokenAsync(inviteTokenTempData);
            session.ManualInputSession.TradingName = organisationData.OrganisationName;
            session.ManualInputSession.BusinessAddress = new Address();
            session.ManualInputSession.BusinessAddress.SubBuildingName = organisationData.SubBuildingName;
            session.ManualInputSession.BusinessAddress.BuildingName = organisationData.BuildingName;
            session.ManualInputSession.BusinessAddress.BuildingNumber = organisationData.BuildingNumber;
            session.ManualInputSession.BusinessAddress.Postcode = organisationData.Postcode;
            session.ManualInputSession.BusinessAddress.Town = organisationData.Town;
            session.ManualInputSession.BusinessAddress.County = organisationData.County;
            session.ManualInputSession.BusinessAddress.Country = organisationData.Country;
            session.ManualInputSession.BusinessAddress.Street = organisationData.Street;
            session.OrganisationType = OrganisationType.NonCompaniesHouseCompany;
            session.ManualInputSession.OrganisationId = invitedOrganisationIdTempData;
        }

        SetBackLink(session, PagePath.ManualInputRoleInOrganisation);
        _sessionManager.SaveSessionAsync(HttpContext.Session, session);

        var viewModel = new ManualInputRoleInOrganisationViewModel()
        {
            RoleInOrganisation = session.ManualInputSession?.RoleInOrganisation
        };

        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.ManualInputRoleInOrganisation)]
    [AuthorizeForScopes(ScopeKeySection = ConfigKeys.FacadeScope)]
    public async Task<IActionResult> ManualInputRoleInOrganisation(ManualInputRoleInOrganisationViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.ManualInputRoleInOrganisation);
            return View(model);
        }

        session.ManualInputSession ??= new ManualInputSession();
        session.ManualInputSession.RoleInOrganisation = model.RoleInOrganisation!;

        return await SaveSessionAndRedirect(session, nameof(FullName), PagePath.ManualInputRoleInOrganisation, PagePath.FullName);
    }

    [HttpGet]
    [Route(PagePath.FullName)]
    [JourneyAccess(PagePath.FullName)]
    public async Task<IActionResult> FullName()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.FullName);

        var viewModel = new FullNameViewModel()
        {
            PostAction = nameof(FullName),
            FirstName = session.Contact.FirstName,
            LastName = session.Contact.LastName
        };

        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.FullName)]
    [JourneyAccess(PagePath.FullName)]
    public async Task<IActionResult> FullName(FullNameViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.FullName);

            return View(model);
        }

        session.Contact.FirstName = model.FirstName;
        session.Contact.LastName = model.LastName;

        return await SaveSessionAndRedirect(session, nameof(TelephoneNumber), PagePath.FullName,
            PagePath.TelephoneNumber);
    }

    [HttpGet]
    [Route(PagePath.TelephoneNumber)]
    [JourneyAccess(PagePath.TelephoneNumber)]
    public async Task<IActionResult> TelephoneNumber([FromQuery(Name = "isUserChangingDetails")]
        bool isUserChangingDetails = false)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        session.IsUserChangingDetails = isUserChangingDetails;
        _sessionManager.SaveSessionAsync(HttpContext.Session, session);

        SetBackLink(session, PagePath.TelephoneNumber);

        return View(new TelephoneNumberViewModel()
        {
            EmailAddress = GetUserEmail(),
            TelephoneNumber = session.Contact.TelephoneNumber,
            IsCompaniesHouseFlow = session.IsCompaniesHouseFlow,
            IsManualInputFlow = session.IsManualInputFlow
        });
    }

    [HttpPost]
    [Route(PagePath.TelephoneNumber)]
    [JourneyAccess(PagePath.TelephoneNumber)]
    public async Task<IActionResult> TelephoneNumber(TelephoneNumberViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.TelephoneNumber);

        if (!ModelState.IsValid)
        {
            model.EmailAddress = GetUserEmail();
            model.IsCompaniesHouseFlow = session.IsCompaniesHouseFlow;
            model.IsManualInputFlow = session.IsManualInputFlow;
            return View(model);
        }

        session.Contact.TelephoneNumber = model.TelephoneNumber;

        return await SaveSessionAndRedirect(session, nameof(CheckYourDetails), PagePath.TelephoneNumber,
            PagePath.CheckYourDetails);
    }

    [HttpGet]
    [Route(PagePath.TradingName)]
    [JourneyAccess(PagePath.TradingName)]
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

    [HttpPost]
    [Route(PagePath.TradingName)]
    [JourneyAccess(PagePath.TradingName)]
    public async Task<IActionResult> TradingName(TradingNameViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.TradingName);

            return View(model);
        }

        session.ManualInputSession ??= new ManualInputSession();

        session.ManualInputSession.TradingName = model.TradingName;

        return await SaveSessionAndRedirect(session, nameof(BusinessAddressPostcode), PagePath.TradingName,
            PagePath.BusinessAddressPostcode);
    }

    [HttpGet]
    [Route(PagePath.BusinessAddressPostcode)]
    [JourneyAccess(PagePath.BusinessAddressPostcode)]
    public async Task<IActionResult> BusinessAddressPostcode()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.BusinessAddressPostcode);

        var viewModel = new BusinessAddressPostcodeViewModel()
        {
            Postcode = session?.ManualInputSession?.BusinessAddress?.Postcode,
        };
        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.BusinessAddressPostcode)]
    [JourneyAccess(PagePath.BusinessAddressPostcode)]
    public async Task<IActionResult> BusinessAddressPostcode(BusinessAddressPostcodeViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.BusinessAddressPostcode);

            return View(model);
        }

        session.ManualInputSession ??= new ManualInputSession();
        session.ManualInputSession.BusinessAddress ??= new Address();

        session.ManualInputSession.BusinessAddress.Postcode = model.Postcode;

        return await SaveSessionAndRedirect(session, nameof(SelectBusinessAddress), PagePath.BusinessAddressPostcode,
            PagePath.SelectBusinessAddress);
    }

    [HttpGet]
    [AuthorizeForScopes(ScopeKeySection = ConfigKeys.FacadeScope)]
    [Route(PagePath.SelectBusinessAddress)]
    [JourneyAccess(PagePath.SelectBusinessAddress)]
    public async Task<IActionResult> SelectBusinessAddress()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        var viewModel = new SelectBusinessAddressViewModel()
        {
            Postcode = session?.ManualInputSession?.BusinessAddress?.Postcode,
        };
        SetBackLink(session, PagePath.SelectBusinessAddress);
        await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

        AddressList? addressList = null;
        var addressLookupFailed = false;

        try
        {
            addressList = await _facadeService.GetAddressListByPostcodeAsync(session.ManualInputSession.BusinessAddress.Postcode);
            if (addressList == null)
            {
                addressLookupFailed = true;
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to retrieve addresses for postcode: {BusinessAddressPostcode}", session.ManualInputSession.BusinessAddress.Postcode);
            addressLookupFailed = true;
        }

        if (addressLookupFailed)
        {
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
            TempData[PostcodeLookupFailedKey] = true;
            return RedirectToAction(nameof(BusinessAddress));
        }
        viewModel.SetAddressItems(addressList?.Addresses, viewModel.SelectedListIndex!);
        session.ManualInputSession?.AddressesForPostcode.Clear();
        session.ManualInputSession?.AddressesForPostcode.AddRange(addressList.Addresses);
        await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
        TempData.Remove(PostcodeLookupFailedKey);

        return View(viewModel);
    }

    [HttpPost]
    [AuthorizeForScopes(ScopeKeySection = ConfigKeys.FacadeScope)]
    [Route(PagePath.SelectBusinessAddress)]
    [JourneyAccess(PagePath.SelectBusinessAddress)]
    public async Task<IActionResult> SelectBusinessAddress(SelectBusinessAddressViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        var indexParseResult = int.TryParse(model.SelectedListIndex, out var index);
        if (ModelState.IsValid &&
            (!indexParseResult || index < 0 || index >= session?.ManualInputSession?.AddressesForPostcode.Count))
        {
            ModelState.AddModelError(nameof(model.SelectedListIndex), "SelectBusinessAddress.ErrorMessage");
        }

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.SelectBusinessAddress);
            model.Postcode = session?.ManualInputSession?.BusinessAddress?.Postcode;

            AddressList? addressList = null;
            var addressLookupFailed = false;

            try
            {
                addressList = await _facadeService.GetAddressListByPostcodeAsync(session.ManualInputSession.BusinessAddress.Postcode);
                if (addressList == null)
                {
                    addressLookupFailed = true;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to retrieve addresses for postcode: {BusinessAddressPostcode}", session.ManualInputSession.BusinessAddress.Postcode);
                addressLookupFailed = true;
            }

            if (addressLookupFailed)
            {
                await SaveSession(session, PagePath.SelectBusinessAddress, PagePath.BusinessAddress);
                TempData[PostcodeLookupFailedKey] = true;
                return RedirectToAction(nameof(BusinessAddress));
            }
            model.SetAddressItems(addressList?.Addresses, model.SelectedListIndex);

            return View(model);
        }
        model.SetAddressItems(session.ManualInputSession?.AddressesForPostcode, model.SelectedListIndex!);
        session.ManualInputSession ??= new ManualInputSession();
        session.ManualInputSession.BusinessAddress = session.ManualInputSession?.AddressesForPostcode[index];

        return await SaveSessionAndRedirect(session, nameof(UkNation), PagePath.SelectBusinessAddress, PagePath.UkNation);
    }

    [HttpGet]
    [Route(PagePath.BusinessAddress)]
    [JourneyAccess(PagePath.BusinessAddress)]
    public async Task<IActionResult> BusinessAddress()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        session.Journey.RemoveAll(x => x == PagePath.SelectBusinessAddress);
        SetBackLink(session, PagePath.BusinessAddress);

        var viewModel = new BusinessAddressViewModel
        {
            Postcode = session.ManualInputSession.BusinessAddress?.Postcode
        };

        if (session.ManualInputSession.BusinessAddress?.IsManualAddress == true)
        {
            viewModel.SubBuildingName = session.ManualInputSession.BusinessAddress.SubBuildingName;
            viewModel.BuildingName = session.ManualInputSession.BusinessAddress.BuildingName;
            viewModel.BuildingNumber = session.ManualInputSession.BusinessAddress.BuildingNumber;
            viewModel.Street = session.ManualInputSession.BusinessAddress.Street;
            viewModel.Town = session.ManualInputSession.BusinessAddress.Town;
            viewModel.County = session.ManualInputSession.BusinessAddress.County;
        }

        if (TempData.ContainsKey(PostcodeLookupFailedKey))
        {
            viewModel.ShowWarning = true;
            TempData[PostcodeLookupFailedKey] = true;
        }
        await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.BusinessAddress)]
    [JourneyAccess(PagePath.BusinessAddress)]
    public async Task<IActionResult> BusinessAddress(BusinessAddressViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.BusinessAddress);

            if (TempData.ContainsKey(PostcodeLookupFailedKey))
            {
                model.ShowWarning = true;
                TempData[PostcodeLookupFailedKey] = true;
            }
            return View(model);
        }

        session.ManualInputSession.BusinessAddress = new Address
        {
            SubBuildingName = model.SubBuildingName,
            BuildingName = model.BuildingName,
            BuildingNumber = model.BuildingNumber,
            Street = model.Street,
            Town = model.Town,
            County = model.County,
            Postcode = model.Postcode,
            IsManualAddress = true
        };
        TempData.Remove(PostcodeLookupFailedKey);

        return await SaveSessionAndRedirect(session, nameof(UkNation), PagePath.BusinessAddress, PagePath.UkNation);
    }

    [HttpGet]
    [Route(PagePath.CheckYourDetails)]
    [JourneyAccess(PagePath.CheckYourDetails)]
    public async Task<IActionResult> CheckYourDetails()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.CheckYourDetails);
        ViewBag.MakeChangesToYourLimitedCompanyLink = _urlOptions.MakeChangesToYourLimitedCompany;

        var viewModel = new CheckYourDetailsViewModel
        {
            IsCompaniesHouseFlow = session.IsCompaniesHouseFlow,
            IsManualInputFlow = session.IsManualInputFlow,
            YourFullName = session.Contact?.ToString(),
            TelephoneNumber = session.Contact?.TelephoneNumber,
            Nation = session.UkNation
        };

        if (viewModel.IsCompaniesHouseFlow)
        {
            viewModel.BusinessAddress = session.CompaniesHouseSession?.Company.BusinessAddress;
            viewModel.CompanyName = session.CompaniesHouseSession?.Company.Name;
            viewModel.CompaniesHouseNumber = session.CompaniesHouseSession?.Company.CompaniesHouseNumber;
            viewModel.RoleInOrganisation = session.CompaniesHouseSession?.RoleInOrganisation;
            viewModel.IsComplianceScheme = session.CompaniesHouseSession?.IsComplianceScheme is true;
        }
        else if (viewModel.IsManualInputFlow)
        {
            viewModel.ProducerType = session.ManualInputSession.ProducerType;
            viewModel.BusinessAddress = session.ManualInputSession?.BusinessAddress;
            viewModel.TradingName = session.ManualInputSession?.TradingName;
            viewModel.IsComplianceScheme = false;
            viewModel.JobTitle = session.ManualInputSession?.RoleInOrganisation;
        }

        session.IsUserChangingDetails = true;
        _sessionManager.SaveSessionAsync(HttpContext.Session, session);

        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.CheckYourDetails)]
    [JourneyAccess(PagePath.CheckYourDetails)]
    public async Task<IActionResult> ConfirmYourDetails()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (session.IsApprovedUser)
        {
            return await SaveSessionAndRedirect(session, nameof(DeclarationWithFullName), PagePath.CheckYourDetails,
                PagePath.DeclarationWithFullName);
        }

        return await SaveSessionAndRedirect(session, nameof(Declaration), PagePath.CheckYourDetails,
            PagePath.Declaration);
    }

    [HttpGet]
    [Route(PagePath.Declaration)]
    [JourneyAccess(PagePath.Declaration)]
    public async Task<IActionResult> Declaration()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.Declaration);
        ViewBag.IsAdminUser = false;
        return View();
    }

    [HttpGet]
    [Route(PagePath.DeclarationWithFullName)]
    [JourneyAccess(PagePath.DeclarationWithFullName)]
    public async Task<IActionResult> DeclarationWithFullName()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var model = new DeclarationViewModelWithFullName();
        SetBackLink(session, PagePath.DeclarationWithFullName);
        ViewBag.OrganisationName = session.CompaniesHouseSession?.Company.Name ?? session.ManualInputSession.TradingName;
        ViewBag.IsAdminUser = true;
        return View(model);
    }

    [HttpPost]
    [AuthorizeForScopes(ScopeKeySection = ConfigKeys.FacadeScope)]
    [Route(PagePath.Declaration)]
    [JourneyAccess(PagePath.Declaration)]
    public async Task<IActionResult> Confirm()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        try
        {
            string email = GetUserEmail();
            var account = _accountMapper.CreateAccountModel(session, email);
            await _facadeService.PostAccountDetailsAsync(account);
            _sessionManager.RemoveSession(HttpContext.Session);

            return Redirect(account.Organisation.IsComplianceScheme
                ? _urlOptions.ReportDataRedirectUrl
                : _urlOptions.ReportDataLandingRedirectUrl);
        }
        catch (ProblemResponseException ex)
        {
            switch (ex.ProblemDetails?.Type)
            {
                default:
                    {
                        throw;
                    }
            }
        }
    }

    [HttpPost]
    [AuthorizeForScopes(ScopeKeySection = ConfigKeys.FacadeScope)]
    [Route(PagePath.DeclarationWithFullName)]
    [JourneyAccess(PagePath.DeclarationWithFullName)]
    public async Task<IActionResult> ConfirmWithFullName(DeclarationViewModelWithFullName model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.DeclarationWithFullName);
            return View(nameof(DeclarationWithFullName), model);
        }

        session.DeclarationFullName = model.FullName;
        session.DeclarationTimestamp = DateTime.Now;

        try
        {
            var organisationData = await _facadeService.GetOrganisationNameByInviteTokenAsync(session.InviteToken);
            var account = _accountMapper.CreateAccountModel(session, organisationData.ApprovedUserEmail);
            account.Person.ContactEmail = organisationData.ApprovedUserEmail;
            await _facadeService.PostApprovedUserAccountDetailsAsync(account);
            _sessionManager.RemoveSession(HttpContext.Session);

            return Redirect(_urlOptions.ReportDataNewApprovedUser);
        }
        catch (ProblemResponseException ex)
        {
            switch (ex.ProblemDetails?.Type)
            {
                default:
                    {
                        throw;
                    }
            }
        }
    }

    [HttpGet]
    [Route(PagePath.ReportPackagingData)]
    [JourneyAccess(PagePath.ReportPackagingData)]
    public IActionResult ReportPackagingData()
    {
        return View();
    }

    [HttpGet]
    [Route(PagePath.AccountAlreadyExists)]
    [JourneyAccess(PagePath.AccountAlreadyExists)]
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
    [JourneyAccess(PagePath.NotAffected)]
    public async Task<IActionResult> NotAffected()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new AccountCreationSession();

        SetBackLink(session, PagePath.NotAffected);

        return View();
    }

    [HttpGet]
    [Route(PagePath.CannotCreateAccount)]
    [JourneyAccess(PagePath.CannotCreateAccount)]
    public async Task<IActionResult> CannotCreateAccount()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.CannotCreateAccount);
        ViewBag.CreateAccountToReportPackagingData = _urlOptions.CreateAccountToReportPackagingData;

        return View();
    }

    public IActionResult RedirectToStart()
    {
        return RedirectToAction(nameof(RegisteredAsCharity));
    }

    private async Task<RedirectToActionResult> SaveSessionAndRedirect(AccountCreationSession session,
        string actionName, string currentPagePath, string? nextPagePath)
    {
        session.IsUserChangingDetails = false;
        await SaveSession(session, currentPagePath, nextPagePath);

        return RedirectToAction(actionName);
    }

    private async Task SaveSession(AccountCreationSession session, string currentPagePath, string? nextPagePath)
    {
        ClearRestOfJourney(session, currentPagePath);

        session.Journey.AddIfNotExists(nextPagePath);

        await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
    }

    private static void ClearRestOfJourney(AccountCreationSession session, string currentPagePath)
    {
        var index = session.Journey.IndexOf(currentPagePath);

        // this also cover if current page not found (index = -1) then it clears all pages
        session.Journey = session.Journey.Take(index + 1).ToList();
    }

    private void SetBackLink(AccountCreationSession session, string currentPagePath)
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

    private string? GetUserEmail() => User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value ??
                                      // Remove when we migrate all environments to custom policy
                                      User.Claims.FirstOrDefault(claim => claim.Type == "emails")?.Value;

}
