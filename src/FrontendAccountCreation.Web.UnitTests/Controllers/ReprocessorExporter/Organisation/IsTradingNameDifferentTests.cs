using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class IsTradingNameDifferentTests() : YesNoPageTestBase<IsTradingNameDifferentViewModel>(
    c => c.IsTradingNameDifferent(),
    (c, vm) => c.IsTradingNameDifferent(vm),
    (session, val) => session.IsTradingNameDifferent = val,
    session => session.IsTradingNameDifferent,
    vm => vm.IsTradingNameDifferent)
{
    // Page and Journey details
    protected override string CurrentPagePath => PagePath.IsTradingNameDifferent;
    protected override string ExpectedBacklinkPagePath => PagePath.UkNation;

    protected override List<string> JourneyForGetBacklinkTest =>
    [
        PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber,
        PagePath.ConfirmCompanyDetails, PagePath.UkNation, PagePath.IsTradingNameDifferent
    ];

    // Redirect targets
    protected override string RedirectActionNameOnYes => nameof(OrganisationController.TradingName);
    protected override string RedirectActionNameOnNo => nameof(OrganisationController.IsOrganisationAPartner);
}
