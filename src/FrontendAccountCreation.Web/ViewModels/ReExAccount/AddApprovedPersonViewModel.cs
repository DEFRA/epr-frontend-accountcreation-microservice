using FrontendAccountCreation.Web.ViewModels.Shared.GovUK;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount
{
    public class AddApprovedPersonViewModel
    {
        [Required(ErrorMessage = "AddNotApprovedPerson.SoleTrader.ErrorMessage")]
        public string InviteUserOption { get; set; }

        public bool? IsOrganisationAPartnership { get; set; }   

        public bool IsInEligibleToBeApprovedPerson { get; set; }

        public ErrorsViewModel? ErrorsViewModel { get; set; }

        public bool IsLimitedPartnership { get; set; }

        public bool IsLimitedLiablePartnership { get; set; }

        public bool IsIndividualInCharge { get; set; }

        public bool IsSoleTrader { get; set; }
    }
}
        