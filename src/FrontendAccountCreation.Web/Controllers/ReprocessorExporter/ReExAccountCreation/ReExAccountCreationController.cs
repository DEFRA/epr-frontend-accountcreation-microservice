using System.Net;
using FrontendAccountCreation.Core.Services;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReExAccounts;
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

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter.ReExAccountCreation;

[Route("[controller]/{action=Index}")]
public class ReExAccountCreationController : Controller
{
    private const string PostcodeLookupFailedKey = "PostcodeLookupFailed";
    private const string OrganisationMetaDataKey = "OrganisationMetaData";

    private readonly ISessionManager<AccountCreationSession> _sessionManager;
    private readonly IFacadeService _facadeService;
    private readonly ICompanyService _companyService;
    private readonly IAccountMapper _accountMapper;
    private readonly ILogger<ReExAccountCreationController> _logger;
    private readonly ExternalUrlsOptions _urlOptions;
    private readonly DeploymentRoleOptions _deploymentRoleOptions;

    public ReExAccountCreationController(
         ISessionManager<AccountCreationSession> sessionManager,
         IFacadeService facadeService,
         ICompanyService companyService,
         IAccountMapper accountMapper,
         IOptions<ExternalUrlsOptions> urlOptions,
         IOptions<DeploymentRoleOptions> deploymentRoleOptions,
         ILogger<ReExAccountCreationController> logger)
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
    [Route("/re-ex")]
    [Route(ReExPagePath.RegisteredAsCharity)]
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

    #endregion

}
