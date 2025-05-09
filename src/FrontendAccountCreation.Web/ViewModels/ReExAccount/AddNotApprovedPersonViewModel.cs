
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount
{
    public class AddNotApprovedPersonViewModel
    {
        [Required(ErrorMessage = "AddNotApprovedPerson.OptionError")]
        public string InviteUserOption { get; set; }
    }
}
