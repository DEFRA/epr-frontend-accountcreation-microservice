using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.Controllers.ReExAccounts
{
    /// <summary>
    /// Reprocessor & Exporter Account creation controller.
    /// </summary>
    public class ReExAccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
