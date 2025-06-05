using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using System.Linq.Expressions;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class SoleTraderTests() : YesNoPageTestBase<SoleTraderViewModel>(
    c => c.SoleTrader(),
    (c, vm) => c.SoleTrader(vm))
{
    // Session property access
    protected override Action<OrganisationSession, bool?> SetSessionValueForGetTest => (session, val) => session.IsIndividualInCharge = val;
    protected override Func<OrganisationSession, bool?> GetSessionValueForPostTest => session => session.IsIndividualInCharge;

    // Page and Journey details
    protected override string CurrentPagePath => PagePath.SoleTrader;
    protected override string ExpectedBacklinkPagePath => PagePath.BusinessAddress;
    //todo: default to last, current in base if not supplied?
    // how much value are we adding here?
    protected override List<string> JourneyForGetBacklinkTest =>
    [
        PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.IsUkMainAddress,
        PagePath.TradingName, PagePath.TypeOfOrganisation, PagePath.UkNation, PagePath.BusinessAddress, // Expected backlink
        PagePath.SoleTrader // Current page
    ];

    // ViewModel property access (now a single expression)
    protected override Expression<Func<SoleTraderViewModel, YesNoAnswer?>> ViewModelYesNoPropertyExpression =>
        vm => vm.IsIndividualInCharge;

    // Redirect targets
    protected override string RedirectActionNameOnYes => nameof(OrganisationController.ManageAccountPerson);
    protected override string RedirectActionNameOnNo => nameof(OrganisationController.NotApprovedPerson);
}