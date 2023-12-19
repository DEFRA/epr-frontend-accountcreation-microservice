using FrontendAccountCreation.Core.Services;
using FrontendAccountCreation.Web.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace FrontendAccountCreation.Web.Controllers.Test;

[AllowAnonymous]
public class TestController : Controller
{
    private readonly IFacadeService _facadeService;

    public TestController(IFacadeService facadeService)
    {
        _facadeService = facadeService;
    }

    [AuthorizeForScopes(ScopeKeySection = ConfigKeys.FacadeScope)]
    [Route("test")]
    public async Task<IActionResult> Test(string returnUrl)
    {
        var result = await _facadeService.GetTestMessageAsync();
        return View("Test", result);
    }
}
