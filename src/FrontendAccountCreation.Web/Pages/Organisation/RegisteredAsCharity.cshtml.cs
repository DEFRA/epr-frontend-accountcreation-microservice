using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Controllers.Errors;
using FrontendAccountCreation.Web.ErrorNext;
using FrontendAccountCreation.Web.FullPages.Radios;
using FrontendAccountCreation.Web.FullPages.Radios.Common;
using FrontendAccountCreation.Web.Sessions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.Collections.Immutable;
using System.Net;

namespace FrontendAccountCreation.Web.Pages.Organisation;

//public class OrganisationPageModel(ISessionManager<OrganisationSession> sessionManager) : PageModel
//{
//    protected ISessionManager<OrganisationSession> SessionManager = sessionManager;
//}

public class RegisteredAsCharityModel(ISessionManager<OrganisationSession> sessionManager) : PageModel, IRadiosPageModel
{
    public IEnumerable<IRadio> Radios => CommonRadios.YesNo;

    [BindProperty]
    public string? SelectedValue { get; set; }

    public IErrorState Errors { get; set; } = ErrorState.Empty;

    //@Localizer["RegisteredAsCharity.Question"]
    public string? Legend => "RegisteredAsCharity.Question";

    public async Task<IActionResult> OnGet(
        [FromServices] IOptions<DeploymentRoleOptions> deploymentRoleOptions)
    {
        // Do not pre-select radio options as this makes it more likely that users will:
        // * not realise they've missed a question
        // * submit the wrong answer

        // only preselect a radio button after the user has previously selected it
        //SelectedValue = Country.Wales.ToString();

        if (deploymentRoleOptions.Value.IsRegulator())
        {
            return RedirectToAction(nameof(ErrorController.ErrorReEx), nameof(ErrorController).Replace("Controller", ""), new
            {
                statusCode = (int)HttpStatusCode.Forbidden
            });
        }

        var session = await sessionManager.GetSessionAsync(HttpContext.Session);

        //YesNoAnswer? isTheOrganisationCharity = null;

        //if (session?.IsTheOrganisationCharity.HasValue == true)
        //{
        //    isTheOrganisationCharity = session.IsTheOrganisationCharity == true ? YesNoAnswer.Yes : YesNoAnswer.No;
        //}

        if (session?.IsTheOrganisationCharity != null)
        {
            SelectedValue = session.IsTheOrganisationCharity.ToString();
        }

        return Page();
    }

    public void OnPost()
    {
        //todo: add new IError to work with model error state
        
        //if (SelectedValue == null)
        //{
        //    Errors = ErrorState.Create(PossibleErrors, ErrorId.NoCountrySelected);
        //    return;
        //}

        //SelectedCountry = (Country)Enum.Parse(typeof(Country), SelectedValue);
    }

    public enum ErrorId
    {
        NoCountrySelected
    }

    public static readonly ImmutableDictionary<int, PossibleError> PossibleErrors =
        ImmutableDictionary.Create<int, PossibleError>()
            .Add(ErrorId.NoCountrySelected, "Select the country where you live");
}