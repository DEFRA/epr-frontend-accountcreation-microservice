using System.Diagnostics.CodeAnalysis;
using FrontendAccountCreation.Core.Sessions;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount
{
    [ExcludeFromCodeCoverage]
    public class ApprovedPersonViewModel
    {
        public bool IsLimitedLiabilityPartnership { get; set; }
        public bool IsLimitedPartnership { get; set; }
        public bool IsApprovedUser { get; set; }
        public ProducerType? ProducerType { get; set; }
    }
}
