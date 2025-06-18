using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class NonUkOrganisationNameTests : OrganisationTestBase
{
    private OrganisationSession _organisationSession = null!;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _organisationSession = new OrganisationSession
        {
            Journey =
            [
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber,
                PagePath.ConfirmCompanyDetails, PagePath.UkNation, PagePath.IsTradingNameDifferent,
                PagePath.TradingName,PagePath.IsUkMainAddress,PagePath.NonUkOrganisationName
            ]
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_organisationSession);
    }

    [TestMethod]
    public async Task GET_WhenNonUkOrganisationNameIsNotInSession_ThenViewIsReturnedWithoutNonUkOrganisationName()
    {
        //Act
        var result = await _systemUnderTest.NonUkOrganisationName();

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<NonUkOrganisationNameViewModel>();
        var viewModel = (NonUkOrganisationNameViewModel?)viewResult.Model;
        viewModel!.NonUkOrganisationName.Should().BeNull();
    }

    [TestMethod]
    public async Task GET_WhenNonUkOrganisationNameIsInSession_ThenViewIsReturnedWithNonUkOrganisationName()
    {
        //Arrange
        const string nonUkOrganisationName = "NonUk Company";
        _organisationSession.ReExManualInputSession = new ReExManualInputSession
        {
            NonUkOrganisationName = nonUkOrganisationName
        };

        //Act
        var result = await _systemUnderTest.NonUkOrganisationName();

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<NonUkOrganisationNameViewModel>();
        var viewModel = (NonUkOrganisationNameViewModel?)viewResult.Model;
        viewModel!.NonUkOrganisationName.Should().Be(nonUkOrganisationName);
    }

    [TestMethod]
    public async Task GET_ThenBackLinkIsCorrect()
    {
        //Act
        var result = await _systemUnderTest.NonUkOrganisationName();

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        AssertBackLink(viewResult, PagePath.IsUkMainAddress);
    }

    [TestMethod]
    public async Task POST_GivenNonUkOrganisationName_NonCompaniesHouseFlow_ThenRedirectToIsTradingNameDifferent()
    {
        // Arrange
        var request = new NonUkOrganisationNameViewModel { NonUkOrganisationName = "German Greengrocers" };
        _organisationSession.OrganisationType = OrganisationType.NonCompaniesHouseCompany;

        // Act
        var result = await _systemUnderTest.NonUkOrganisationName(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.IsTradingNameDifferent));
    }

    [TestMethod]
    public async Task POST_GivenNonUkOrganisationName_ThenUpdatesSession()
    {
        // Arrange
        var request = new NonUkOrganisationNameViewModel { NonUkOrganisationName = "German Greengrocers" };

        // Act
        await _systemUnderTest.NonUkOrganisationName(request);

        // Assert
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task POST_GivenNoNonUkOrganisationName_ThenSessionNotUpdated()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(NonUkOrganisationNameViewModel.NonUkOrganisationName), "Organisation name field is required");

        // Act
        await _systemUnderTest.NonUkOrganisationName(new NonUkOrganisationNameViewModel());

        // Assert
        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<OrganisationSession>>()),
            Times.Never);
    }

    [TestMethod]
    public async Task POST_GivenNoTradingName_ThenReturnView()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(NonUkOrganisationNameViewModel.NonUkOrganisationName), "Organisation name field is required");
        var viewModel = new NonUkOrganisationNameViewModel
        {
            NonUkOrganisationName = ""
        };

        // Act
        var result = await _systemUnderTest.NonUkOrganisationName(viewModel);

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [TestMethod]
    public async Task POST_GivenNonUkOrganisationNameTooLong_ThenReturnViewWithUsersBadInput()
    {
        // Arrange
        const string badNonUkOrganisationName = "123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789";

        _systemUnderTest.ModelState.AddModelError(nameof(NonUkOrganisationNameViewModel.NonUkOrganisationName), "Organisation name must be 170 characters or less");
        var viewModel = new NonUkOrganisationNameViewModel
        {
            NonUkOrganisationName = badNonUkOrganisationName
        };

        // Act
        var result = await _systemUnderTest.NonUkOrganisationName(viewModel);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<NonUkOrganisationNameViewModel?>();
        var resultViewModel = (NonUkOrganisationNameViewModel?)viewResult.Model;
        resultViewModel!.NonUkOrganisationName.Should().Be(badNonUkOrganisationName);
    }

    [TestMethod]
    public async Task POST_GivenNoNonUkOrganisationName_ThenViewHasCorrectBackLink()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(NonUkOrganisationNameViewModel.NonUkOrganisationName), "Organisation name field is required");

        // Act
        var result = await _systemUnderTest.NonUkOrganisationName(new NonUkOrganisationNameViewModel());

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        AssertBackLink(viewResult, PagePath.IsUkMainAddress);
    }
}
