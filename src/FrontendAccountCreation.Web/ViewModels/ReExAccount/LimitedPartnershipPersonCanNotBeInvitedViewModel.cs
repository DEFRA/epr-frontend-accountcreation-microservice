using FrontendAccountCreation.Core.Models;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class LimitedPartnershipPersonCanNotBeInvitedViewModel
{
    public Guid Id { get; set; }

    public YesNoNotSure? TheyManageOrControlOrganisation { get; set; }
}