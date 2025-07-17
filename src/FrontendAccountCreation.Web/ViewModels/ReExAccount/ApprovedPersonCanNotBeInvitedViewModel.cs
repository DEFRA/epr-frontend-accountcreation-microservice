using FrontendAccountCreation.Core.Models;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class ApprovedPersonCanNotBeInvitedViewModel
{
    public Guid Id { get; set; }

    public YesNoNotSure? TheyManageOrControlOrganisation { get; set; }

    public bool? AreTheyIndividualInCharge { get; set; }

    public bool IsNonCompanyHousePartnership { get; set; }
}