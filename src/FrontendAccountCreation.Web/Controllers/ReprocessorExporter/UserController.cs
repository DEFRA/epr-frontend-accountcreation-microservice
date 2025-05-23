using System.Security.Claims;
using FrontendAccountCreation.Core.Extensions;
using FrontendAccountCreation.Core.Services;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
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
[Route("re-ex/user")]
[Feature(FeatureFlags.ReprocessorExporter)]
public class UserController : ControllerBase<ReExAccountCreationSession>
{
    private readonly ISessionManager<ReExAccountCreationSession> _sessionManager;
    private readonly IFacadeService _facadeService;
    private readonly IReExAccountMapper _reExAccountMapper;
    private readonly ILogger<UserController> _logger;
    private readonly ServiceKeysOptions _serviceKeyOptions;
    private readonly ExternalUrlsOptions _urlOptions;

    public UserController(
        ISessionManager<ReExAccountCreationSession> sessionManager,
        IFacadeService facadeService,
        IReExAccountMapper reExAccountMapper,
        IOptions<ExternalUrlsOptions> urlOptions,
        IOptions<ServiceKeysOptions> serviceKeyOptions,
        ILogger<UserController> logger) : base(sessionManager)
    {
        _sessionManager = sessionManager;
        _facadeService = facadeService;
        _reExAccountMapper = reExAccountMapper;
        _urlOptions = urlOptions.Value;
        _serviceKeyOptions = serviceKeyOptions.Value;
        _logger = logger;
    }

    [HttpGet]
    [Route("inject-error")]
    public IActionResult InjectError()
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route("")]
    [Route(PagePath.FullName)]
    public async Task<IActionResult> ReExAccountFullName()
    {
        var userExists = await _facadeService.DoesAccountAlreadyExistAsync();
        if (userExists)
        {
          return RedirectToAction("ReExUserAlreadyExists");
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new ReExAccountCreationSession
        {
            Journey = [PagePath.FullName]
        };

        var viewModel = new ReExAccountFullNameViewModel
        {
            FirstName = session.Contact?.FirstName,
            LastName = session.Contact?.LastName
        };

        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.FullName)]
    public async Task<IActionResult> ReExAccountFullName(ReExAccountFullNameViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new ReExAccountCreationSession()
        {
            Journey = [PagePath.FullName]
        };

        session.Contact.FirstName = model.FirstName;
        session.Contact.LastName = model.LastName;

        return await SaveSessionAndRedirect(session, nameof(ReExAccountTelephoneNumber), PagePath.FullName,
            PagePath.TelephoneNumber);
    }

    [HttpGet]
    [Route(PagePath.TelephoneNumber)]
    [ReprocessorExporterJourneyAccess(PagePath.TelephoneNumber)]
    public async Task<IActionResult> ReExAccountTelephoneNumber()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new ReExAccountCreationSession();
        _sessionManager.SaveSessionAsync(HttpContext.Session, session);

        SetBackLink(session, PagePath.TelephoneNumber);

        return View(new ReExAccountTelephoneNumberViewModel()
        {
            TelephoneNumber = session.Contact.TelephoneNumber,
            EmailAddress = session.Contact.Email,
        });
    }

    [HttpPost]
    [Route(PagePath.TelephoneNumber)]
    [ReprocessorExporterJourneyAccess(PagePath.TelephoneNumber)]
    public async Task<IActionResult> ReExAccountTelephoneNumber(ReExAccountTelephoneNumberViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new ReExAccountCreationSession();

        SetBackLink(session, PagePath.TelephoneNumber);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        session.Contact.TelephoneNumber = model.TelephoneNumber;
        session.Contact.Email = model.EmailAddress;

        string? email = GetUserEmail();

        var account = _reExAccountMapper.CreateReprocessorExporterAccountModel(session, email);

        await _facadeService.PostReprocessorExporterAccountAsync(account, _serviceKeyOptions.ReprocessorExporter);

        return await SaveSessionAndRedirect(session, nameof(Success), PagePath.TelephoneNumber,
            PagePath.Success);
    }

    [HttpGet]
    [AuthorizeForScopes(ScopeKeySection = ConfigKeys.FacadeScope)]
    [Route(PagePath.Success)]
    [ReprocessorExporterJourneyAccess(PagePath.Success)]
    public async Task<IActionResult> Success()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new ReExAccountCreationSession();

        var viewModel = new SuccessViewModel
        {
            UserName = $"{session.Contact.FirstName} {session.Contact.LastName}"
        };

        _sessionManager.RemoveSession(HttpContext.Session);

        return View(viewModel);
    }

    [HttpGet]
    [Route(PagePath.UserAlreadyExists)]
    public IActionResult ReExUserAlreadyExists()
    {
        return View();
    }

    private string? GetUserEmail() => User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value ??
                                      // Remove when we migrate all environments to custom policy
                                      User.Claims.FirstOrDefault(claim => claim.Type == "emails")?.Value;
}