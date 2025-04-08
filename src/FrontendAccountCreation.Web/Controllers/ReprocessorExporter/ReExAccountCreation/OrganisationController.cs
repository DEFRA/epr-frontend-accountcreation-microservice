using System.Net;
using FrontendAccountCreation.Core.Services;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Errors;
using FrontendAccountCreation.Web.Sessions;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using FrontendAccountCreation.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using System;
using FrontendAccountCreation.Core.Extensions;
using FrontendAccountCreation.Web.Controllers.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter.ReExAccountCreation;

[Route("re-ex/organisation/")]
public class OrganisationController : Controller
{
    private const string PostcodeLookupFailedKey = "PostcodeLookupFailed";
    private const string OrganisationMetaDataKey = "OrganisationMetaData";

    private readonly ISessionManager<AccountCreationSession> _sessionManager;
    private readonly IFacadeService _facadeService;
    private readonly ICompanyService _companyService;
    private readonly IAccountMapper _accountMapper;
    private readonly ILogger<OrganisationController> _logger;
    private readonly ExternalUrlsOptions _urlOptions;
    private readonly DeploymentRoleOptions _deploymentRoleOptions;

    public OrganisationController(
         ISessionManager<AccountCreationSession> sessionManager,
         IFacadeService facadeService,
         ICompanyService companyService,
         IAccountMapper accountMapper,
         IOptions<ExternalUrlsOptions> urlOptions,
         IOptions<DeploymentRoleOptions> deploymentRoleOptions,
         ILogger<OrganisationController> logger)
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

        if (TempData["ModelState"] is not null)
        {
            ModelState.Merge(DeserializeModelState(TempData["ModelState"].ToString()));
            viewModel.CompaniesHouseNumber = TempData["CompaniesHouseNumber"].ToString();
        }

        return View(viewModel);
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

    public IActionResult RedirectToStart()
    {
        return RedirectToAction(nameof(RegisteredAsCharity));
    }

    #region Private Methods 

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

    #endregion

}
