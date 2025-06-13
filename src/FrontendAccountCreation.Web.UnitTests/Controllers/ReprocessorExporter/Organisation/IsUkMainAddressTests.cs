using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class IsUkMainAddressTests() : YesNoPageTestBase<IsUkMainAddressViewModel>(
    c => c.IsUkMainAddress(),
    (c, vm) => c.IsUkMainAddress(vm),
    (session, val) => session.IsUkMainAddress = val,
    session => session.IsUkMainAddress,
    vm => vm.IsUkMainAddress)
{
    // Page and Journey details
    protected override string CurrentPagePath => PagePath.IsUkMainAddress;
    protected override string ExpectedBacklinkPagePath => PagePath.RegisteredWithCompaniesHouse;

    protected override List<string> JourneyForGetBacklinkTest =>
    [
        PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.IsUkMainAddress
    ];

    // Redirect targets
    protected override string RedirectActionNameOnYes => nameof(OrganisationController.TradingName);
    protected override string RedirectActionNameOnNo => nameof(OrganisationController.NotImplemented);
}