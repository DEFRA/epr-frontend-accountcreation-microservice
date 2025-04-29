
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount
{
    public class AddApprovedPersonViewModel
    {
        [Required(ErrorMessage = "Please select an option to proceed.")]
        public string InviteUserOption { get; set; }
    }
}
