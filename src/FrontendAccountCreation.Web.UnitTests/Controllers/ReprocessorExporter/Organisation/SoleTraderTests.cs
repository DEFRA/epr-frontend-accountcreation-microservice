using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class SoleTraderTests() : YesNoPageTestBase<SoleTraderViewModel>(
    c => c.SoleTrader(),
    (c, vm) => c.SoleTrader(vm),
    (session, val) => session.IsIndividualInCharge = val,
    session => session.IsIndividualInCharge,
    vm => vm.IsIndividualInCharge)
{
    // Page and Journey details
    protected override string CurrentPagePath => PagePath.SoleTrader;
    protected override string ExpectedBacklinkPagePath => PagePath.BusinessAddress;

    protected override List<string> JourneyForGetBacklinkTest =>
    [
        PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.IsUkMainAddress,
        PagePath.TradingName, PagePath.TypeOfOrganisation, PagePath.UkNation,
        PagePath.BusinessAddress, // Expected backlink
        PagePath.SoleTrader // Current page
    ];

    // Redirect targets
    protected override string RedirectActionNameOnYes =>
        nameof(ApprovedPersonController.YouAreApprovedPersonSoleTrader);

    //todo: revisit once 'Add an approved person' page is in
    //protected override string RedirectActionNameOnNo => nameof(OrganisationController.NotApprovedPerson);
    protected override string RedirectActionNameOnNo => nameof(ApprovedPersonController.SoleTraderTeamMemberDetails);
}