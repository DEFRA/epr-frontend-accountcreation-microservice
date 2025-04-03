using FrontendAccountCreation.Core.Extensions;
using FrontendAccountCreation.Core.Services;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Sessions;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace FrontendAccountCreation.Web.Controllers.ReExAccounts
{
    /// <summary>
    /// Reprocessor & Exporter Account creation controller.
    /// </summary>
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

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route(PagePath.ReExAccountFullName)]
        [JourneyAccess(PagePath.ReExAccountFullName)]
        public async Task<IActionResult> ReExAccountFullName()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            SetBackLink(session, PagePath.ReExAccountFullName);

            var viewModel = new ReExAccountFullNameViewModel()
            {
                PostAction = nameof(ReExAccountFullName),
                FirstName = session.Contact.FirstName,
                LastName = session.Contact.LastName
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route(PagePath.ReExAccountFullName)]
        [JourneyAccess(PagePath.ReExAccountFullName)]
        public async Task<IActionResult> ReExAccountFullName(ReExAccountFullNameViewModel model)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            if (!ModelState.IsValid)
            {
                SetBackLink(session, PagePath.ReExAccountFullName);

                return View(model);
            }

            session.Contact.FirstName = model.FirstName;
            session.Contact.LastName = model.LastName;

            //return await SaveSessionAndRedirect(session, nameof(TelephoneNumber), PagePath.FullName,
            //    PagePath.TelephoneNumber);

            return await SaveSessionAndRedirect(session, "TelephoneNumber", PagePath.ReExAccountFullName,
                PagePath.TelephoneNumber);
        }

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
            if (currentPagePath != PagePath.CheckYourDetails)
            {
                ViewBag.BackLinkToDisplay = PagePath.CheckYourDetails;
            }
            else
            {
                ViewBag.BackLinkToDisplay = session.Journey.PreviousOrDefault(currentPagePath) ?? string.Empty;
            }
        }
    }
}
