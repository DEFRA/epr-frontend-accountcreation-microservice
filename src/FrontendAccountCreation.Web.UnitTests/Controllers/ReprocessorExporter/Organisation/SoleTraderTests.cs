using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

//todo: could have new base for yes/no page tests
[TestClass]
public class SoleTraderTests : OrganisationTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task GET_BackLinkIsTodo()
    {
        //Arrange
        var orgCreationSession = new OrganisationSession
        {
            Journey =
            [
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.IsUkMainAddress,
                PagePath.TradingName, PagePath.TypeOfOrganisation, PagePath.UkNation, PagePath.BusinessAddress,
                PagePath.SoleTrader
            ]
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(orgCreationSession);

        //Act
        var result = await _systemUnderTest.SoleTrader();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        AssertBackLink(viewResult, PagePath.BusinessAddress);
    }

    [TestMethod]
    [DataRow(true, YesNoAnswer.Yes)]
    [DataRow(false, YesNoAnswer.No)]
    [DataRow(null, null)]
    public async Task GET_CorrectViewModelIsReturnedInTheView(
        bool? isIndividualInCharge,
        YesNoAnswer? expectedIsIndividualInChargeViewModel)
    {
        //Arrange
        var orgCreationSession = new OrganisationSession
        {
            Journey =
            [
                PagePath.RegisteredAsCharity,
                PagePath.RegisteredWithCompaniesHouse,
                PagePath.IsUkMainAddress,
                PagePath.TradingName
            ],
            IsIndividualInCharge = isIndividualInCharge
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(orgCreationSession);

        //Act
        var result = await _systemUnderTest.SoleTrader();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<SoleTraderViewModel>();
        var viewModel = (SoleTraderViewModel?)viewResult.Model;
        viewModel!.IsIndividualInCharge.Should().Be(expectedIsIndividualInChargeViewModel);
    }

    [TestMethod]
    [DataRow(YesNoAnswer.Yes, true)]
    [DataRow(YesNoAnswer.No, false)]
    public async Task POST_UserSelectsYesOrNo_SessionUpdatedCorrectly(YesNoAnswer userAnswer, bool expectedIsIndividualInChargeInSession)
    {
        // Arrange
        var request = new SoleTraderViewModel { IsIndividualInCharge = userAnswer };

        // Act
        await _systemUnderTest.SoleTrader(request);

        // Assert
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(),
            It.Is<OrganisationSession>(os => os.IsIndividualInCharge == expectedIsIndividualInChargeInSession)),
            Times.Once);
    }

    [TestMethod]
    public async Task POST_UserSelectsNothing_SessionNotUpdated()
    {
        // Arrange
        var request = new SoleTraderViewModel { IsIndividualInCharge = null };
        _systemUnderTest.ModelState.AddModelError("IsIndividualInCharge", "Select yes if you are the individual in charge of your business");

        // Act
        await _systemUnderTest.SoleTrader(request);

        // Assert
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(),
                It.IsAny<OrganisationSession>()),
            Times.Never);
    }

    [TestMethod]
    public async Task POST_UserSelectsNothing_ViewIsReturnedWithCorrectModel()
    {
        // Arrange
        var request = new SoleTraderViewModel { IsIndividualInCharge = null };
        _systemUnderTest.ModelState.AddModelError("IsIndividualInCharge", "Select yes if you are the individual in charge of your business");

        // Act
        var result = await _systemUnderTest.SoleTrader(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<SoleTraderViewModel>();
        var viewModel = (SoleTraderViewModel?)viewResult.Model;
        viewModel!.IsIndividualInCharge.Should().BeNull();
    }

    //todo:
    //[TestMethod]
    //[DataRow(YesNoAnswer.Yes, nameof(OrganisationController.TradingName))]
    //[DataRow(YesNoAnswer.No, nameof(OrganisationController.NotImplemented))]
    //public async Task POST_UserSelectsYesOrNo_UserIsRedirected(
    //    YesNoAnswer userAnswer, string expectedRedirect)
    //{
    //    // Arrange
    //    var request = new IsUkMainAddressViewModel { IsUkMainAddress = userAnswer };

    //    // Act
    //    var result = await _systemUnderTest.IsUkMainAddress(request);

    //    // Assert
    //    result.Should().BeOfType<RedirectToActionResult>();

    //    ((RedirectToActionResult)result).ActionName.Should().Be(expectedRedirect);
    //}
}