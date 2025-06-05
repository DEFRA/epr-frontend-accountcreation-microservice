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

    // ViewModel property access (now a single expression)
    protected override Expression<Func<IsTradingNameDifferentViewModel, YesNoAnswer?>> ViewModelYesNoPropertyExpression =>
        vm => vm.IsTradingNameDifferent;

    // Redirect targets
    protected override string RedirectActionNameOnYes => nameof(OrganisationController.TradingName);
    protected override string RedirectActionNameOnNo => nameof(OrganisationController.IsOrganisationAPartner);
}

//[TestClass]
//public class IsTradingNameDifferentTests : OrganisationTestBase
//{
//    [TestInitialize]
//    public void Setup()
//    {
//        SetupBase();
//    }

//    [TestMethod]
//    public async Task GET_BackLinkIsUkNation()
//    {
//        //Arrange
//        var orgCreationSession = new OrganisationSession
//        {
//            Journey =
//            [
//                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber,
//                PagePath.ConfirmCompanyDetails, PagePath.UkNation, PagePath.IsTradingNameDifferent
//            ]
//        };

//        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(orgCreationSession);

//        //Act
//        var result = await _systemUnderTest.IsTradingNameDifferent();

//        //Assert
//        result.Should().NotBeNull();
//        result.Should().BeOfType<ViewResult>();
//        var viewResult = (ViewResult)result;
//        AssertBackLink(viewResult, PagePath.UkNation);
//    }

//    [TestMethod]
//    [DataRow(true, YesNoAnswer.Yes)]
//    [DataRow(false, YesNoAnswer.No)]
//    [DataRow(null, null)]
//    public async Task GET_CorrectViewModelIsReturnedInTheView(bool? isTradingNameDifferentSession, YesNoAnswer? expectedIsTradingNameDifferentViewModel)
//    {
//        //Arrange
//        var orgCreationSession = new OrganisationSession
//        {
//            Journey =
//            [
//                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber,
//                PagePath.ConfirmCompanyDetails, PagePath.UkNation, PagePath.IsTradingNameDifferent
//            ],
//            IsTradingNameDifferent = isTradingNameDifferentSession
//        };

//        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(orgCreationSession);

//        //Act
//        var result = await _systemUnderTest.IsTradingNameDifferent();

//        //Assert
//        result.Should().NotBeNull();
//        result.Should().BeOfType<ViewResult>();
//        var viewResult = (ViewResult)result;
//        viewResult.Model.Should().BeOfType<IsTradingNameDifferentViewModel>();
//        var viewModel = (IsTradingNameDifferentViewModel?)viewResult.Model;
//        viewModel!.IsTradingNameDifferent.Should().Be(expectedIsTradingNameDifferentViewModel);
//    }

//    [TestMethod]
//    [DataRow(YesNoAnswer.Yes, true)]
//    [DataRow(YesNoAnswer.No, false)]
//    public async Task POST_UserSelectsYesOrNo_SessionUpdatedCorrectly(YesNoAnswer userAnswer, bool expectedIsTradingNameDifferentInSession)
//    {
//        // Arrange
//        var request = new IsTradingNameDifferentViewModel { IsTradingNameDifferent = userAnswer };

//        // Act
//        await _systemUnderTest.IsTradingNameDifferent(request);

//        // Assert
//        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(),
//            It.Is<OrganisationSession>(os => os.IsTradingNameDifferent == expectedIsTradingNameDifferentInSession)),
//            Times.Once);
//    }

//    [TestMethod]
//    public async Task POST_UserSelectsNothing_SessionNotUpdated()
//    {
//        // Arrange
//        var request = new IsTradingNameDifferentViewModel { IsTradingNameDifferent = null };
//        _systemUnderTest.ModelState.AddModelError("IsTradingNameDifferent", "Select if your organisation's trading name is different to its Companies House name");

//        // Act
//        await _systemUnderTest.IsTradingNameDifferent(request);

//        // Assert
//        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(),
//                It.IsAny<OrganisationSession>()),
//            Times.Never);
//    }

//    [TestMethod]
//    public async Task POST_UserSelectsNothing_ViewIsReturnedWithCorrectModel()
//    {
//        // Arrange
//        var request = new IsTradingNameDifferentViewModel { IsTradingNameDifferent = null };
//        _systemUnderTest.ModelState.AddModelError("IsTradingNameDifferent", "Select if your organisation's trading name is different to its Companies House name");

//        // Act
//        var result = await _systemUnderTest.IsTradingNameDifferent(request);

//        // Assert
//        result.Should().NotBeNull();
//        result.Should().BeOfType<ViewResult>();
//        var viewResult = (ViewResult)result;
//        viewResult.Model.Should().BeOfType<IsTradingNameDifferentViewModel>();
//        var viewModel = (IsTradingNameDifferentViewModel?)viewResult.Model;
//        viewModel!.IsTradingNameDifferent.Should().BeNull();
//    }

//    [TestMethod]
//    [DataRow(YesNoAnswer.Yes, nameof(OrganisationController.TradingName))]
//    [DataRow(YesNoAnswer.No, nameof(OrganisationController.IsOrganisationAPartner))]
//    public async Task POST_UserSelectsYesOrNo_UserIsRedirected(YesNoAnswer userAnswer, string expectedRedirect)
//    {
//        // Arrange
//        var request = new IsTradingNameDifferentViewModel { IsTradingNameDifferent = userAnswer };

//        // Act
//        var result = await _systemUnderTest.IsTradingNameDifferent(request);

//        // Assert
//        result.Should().BeOfType<RedirectToActionResult>();

//        ((RedirectToActionResult)result).ActionName.Should().Be(expectedRedirect);
//    }
//}