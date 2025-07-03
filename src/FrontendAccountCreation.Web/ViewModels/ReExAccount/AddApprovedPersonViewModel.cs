using FrontendAccountCreation.Web.ViewModels.Shared.GovUK;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount
{
    [ExcludeFromCodeCoverage]
    public class AddApprovedPersonViewModel
    {
        [Required]
        public string InviteUserOption { get; set; }

        public bool? IsOrganisationAPartnership { get; set; }

        public bool IsInEligibleToBeApprovedPerson { get; set; }

        public ErrorsViewModel? ErrorsViewModel { get; set; }

        public bool IsLimitedPartnership { get; set; }

        public bool IsLimitedLiablePartnership { get; set; }

        public bool IsIndividualInCharge { get; set; }

        public bool IsSoleTrader { get; set; }

        public bool IsNonUk { get; set; }

        public bool IsNonCompaniesHousePartnership { get; set; }

        /// <summary>
        /// Note: This is being added to select default message option for all the flows (as per the story)
        /// if anything changes then you would need to go through else option i.e false
        /// </summary>
        public bool SelectDefaultErrorMessage { get; set; } = true;
    }
}
