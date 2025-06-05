using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class IsTradingNameDifferentTests : YesNoPageTestBase<IsTradingNameDifferentViewModel>
{
    // Controller and ViewModel actions
    protected override Func<OrganisationController, Task<IActionResult>> GetPageAction => ctrl => ctrl.IsTradingNameDifferent();
    protected override Func<OrganisationController, IsTradingNameDifferentViewModel, Task<IActionResult>> PostPageAction => (ctrl, vm) => ctrl.IsTradingNameDifferent(vm);

    // Session property access
    protected override Action<OrganisationSession, bool?> SetSessionValueForGetTest => (session, val) => session.IsTradingNameDifferent = val;
    protected override Func<OrganisationSession, bool?> GetSessionValueForPostTest => session => session.IsTradingNameDifferent;

    // Page and Journey details
    protected override string CurrentPagePath => PagePath.IsTradingNameDifferent;
    protected override string ExpectedBacklinkPagePath => PagePath.UkNation;

    protected override List<string> JourneyForGetBacklinkTest =>
    [
        PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber,
        PagePath.ConfirmCompanyDetails, PagePath.UkNation, PagePath.IsTradingNameDifferent
    ];

    // ViewModel property access
    protected override Expression<Func<IsTradingNameDifferentViewModel, YesNoAnswer?>> ViewModelYesNoPropertyExpression =>
        vm => vm.IsTradingNameDifferent;

    // Redirect targets
    protected override string RedirectActionNameOnYes => nameof(OrganisationController.TradingName);
    protected override string RedirectActionNameOnNo => nameof(OrganisationController.IsOrganisationAPartner);
}
