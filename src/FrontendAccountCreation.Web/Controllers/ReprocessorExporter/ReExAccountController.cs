using System.Security.Claims;
using FrontendAccountCreation.Core.Extensions;
using FrontendAccountCreation.Core.Services;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Sessions;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter;

/// <summary>
/// Reprocessor & Exporter Account creation controller.
/// </summary>
[Route("reprocessorexporter")]
public class ReExAccountController : Controller
{
    private readonly ISessionManager<ReExAccountCreationSession> _sessionManager;
    private readonly IFacadeService _facadeService;
    private readonly IAccountMapper _accountMapper;
    private readonly ILogger<ReExAccountController> _logger;
    private readonly ExternalUrlsOptions _urlOptions;
    private readonly DeploymentRoleOptions _deploymentRoleOptions;

    public ReExAccountController(
        ISessionManager<ReExAccountCreationSession> sessionManager,
        IFacadeService facadeService,
        IAccountMapper accountMapper,
        IOptions<ExternalUrlsOptions> urlOptions,
        IOptions<DeploymentRoleOptions> deploymentRoleOptions,
        ILogger<ReExAccountController> logger)
    {
        _sessionManager = sessionManager;
        _facadeService = facadeService;
        _accountMapper = accountMapper;
        _urlOptions = urlOptions.Value;
        _deploymentRoleOptions = deploymentRoleOptions.Value;
        _logger = logger;
    }

    //todo: we'll have to handle user already exists. probably best to handle it at the start of the journey

    [HttpGet]
    [Route(ReExPagePath.ReExAccountFullName)]
    [ReprocessorExporterJourneyAccess(ReExPagePath.ReExAccountFullName)]
    public async Task<IActionResult> ReExAccountFullName()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, ReExPagePath.ReExAccountFullName);

        var viewModel = new ReExAccountFullNameViewModel()
        {
            PostAction = nameof(ReExAccountFullName),
            FirstName = session.Contact.FirstName,
            LastName = session.Contact.LastName
        };

        return View(viewModel);
    }

    [HttpPost]
    [Route("")]
    [Route(ReExPagePath.ReExAccountFullName)]
    [ReprocessorExporterJourneyAccess(ReExPagePath.ReExAccountFullName)]
    public async Task<IActionResult> ReExAccountFullName(ReExAccountFullNameViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, ReExPagePath.ReExAccountFullName);

            return View(model);
        }

        session.Contact.FirstName = model.FirstName;
        session.Contact.LastName = model.LastName;

        //return await SaveSessionAndRedirect(session, nameof(TelephoneNumber), ReExPagePath.FullName,
        //    PagePath.TelephoneNumber);

        return await SaveSessionAndRedirect(session, "TelephoneNumber", ReExPagePath.ReExAccountFullName,
            PagePath.TelephoneNumber);
    }

    [HttpGet]
    [Route(ReExPagePath.ReExAccountTelephoneNumber)]
    [JourneyAccess(ReExPagePath.ReExAccountTelephoneNumber)]
    public async Task<IActionResult> ReExAccountTelephoneNumber()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        _sessionManager.SaveSessionAsync(HttpContext.Session, session);

        SetBackLink(session, ReExPagePath.ReExAccountTelephoneNumber);

        return View(new ReExAccountTelephoneNumberViewModel()
        {
            TelephoneNumber = session.Contact.TelephoneNumber,
        });
    }

    [HttpPost]
    [Route(ReExPagePath.ReExAccountTelephoneNumber)]
    [JourneyAccess(ReExPagePath.ReExAccountTelephoneNumber)]
    public async Task<IActionResult> ReExAccountTelephoneNumber(ReExAccountTelephoneNumberViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, ReExPagePath.ReExAccountTelephoneNumber);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        session.Contact.TelephoneNumber = model.TelephoneNumber;

        //return await SaveSessionAndRedirect(session, nameof(CheckYourDetails), PagePath.TelephoneNumber,
        //    PagePath.CheckYourDetails);

        return await SaveSessionAndRedirect(session, "CheckYourDetails", ReExPagePath.ReExAccountTelephoneNumber,
            PagePath.CheckYourDetails); //TODO : Change Check your Details to Success Page
    }

    [HttpGet]
    [AuthorizeForScopes(ScopeKeySection = ConfigKeys.FacadeScope)]
    [Route(ReExPagePath.Success)]
    [ReprocessorExporterJourneyAccess(ReExPagePath.Success)]
    public async Task<IActionResult> Success()
    {
        //todo: will do this once earlier stories are done
        //var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        var session = new ReExAccountCreationSession
        {
            Contact = new ReExContact
            {
                FirstName = "bob",
                LastName = "smith",
                TelephoneNumber = "01234567890"
            },
            Journey = [ReExPagePath.ReExAccountFullName]
        };


        //todo: person and user email always the same, so only need to pass userid down the stack and pick email from person
        // unless we send down an user just in case they could ever end up being different

        string? email = GetUserEmail();
        //todo: handle null email
        var account = _accountMapper.CreateReExAccountModel(session, email);
        await _facadeService.PostReprocessorExporterAccountAsync(account);
        _sessionManager.RemoveSession(HttpContext.Session);

        SetBackLink(session, ReExPagePath.Success);

        var viewModel = new SuccessViewModel
        {
            //todo: could just add contact to viewmodel
            UserName = $"{session.Contact.FirstName} {session.Contact.LastName}"
        };

        return View(viewModel);
    }

    //todo: move this (these?) somewhere common?
    private string? GetUserEmail() => User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value ??
                                      // Remove when we migrate all environments to custom policy
                                      User.Claims.FirstOrDefault(claim => claim.Type == "emails")?.Value;

    private async Task<RedirectToActionResult> SaveSessionAndRedirect(ReExAccountCreationSession session,
        string actionName, string currentPagePath, string? nextPagePath)
    {
        await SaveSession(session, currentPagePath, nextPagePath);

        return RedirectToAction(actionName);
    }

    private async Task SaveSession(ReExAccountCreationSession session, string currentPagePath, string? nextPagePath)
    {
        ClearRestOfJourney(session, currentPagePath);

        session.Journey.AddIfNotExists(nextPagePath);

        await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
    }

    private static void ClearRestOfJourney(ReExAccountCreationSession session, string currentPagePath)
    {
        var index = session.Journey.IndexOf(currentPagePath);

        // this also cover if current page not found (index = -1) then it clears all pages
        session.Journey = session.Journey.Take(index + 1).ToList();
    }

    private void SetBackLink(ReExAccountCreationSession session, string currentPagePath)
    {
        ViewBag.BackLinkToDisplay = session.Journey.PreviousOrDefault(currentPagePath) ?? string.Empty;
    }
}