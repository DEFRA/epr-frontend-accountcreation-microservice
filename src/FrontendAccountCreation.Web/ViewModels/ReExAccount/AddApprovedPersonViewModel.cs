using FrontendAccountCreation.Web.ViewModels.Shared.GovUK;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount
{
    public class AddApprovedPersonViewModel
    {
        [Required(ErrorMessage = "AddAnApprovedPerson.OptionError")]
        public string InviteUserOption { get; set; }

        public bool? IsOrganisationAPartnership { get; set; }   

        public bool IsInEligibleToBeApprovedPerson { get; set; }

        public ErrorsViewModel? ErrorsViewModel { get; set; }
    }
}
        