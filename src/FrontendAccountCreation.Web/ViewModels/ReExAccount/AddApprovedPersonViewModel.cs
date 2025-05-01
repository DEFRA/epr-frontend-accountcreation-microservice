
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount
{
    public class AddApprovedPersonViewModel
    {
        [Required(ErrorMessage = "AddAnApprovedPerson.OptionError")]
        public string InviteUserOption { get; set; }
    }
}
