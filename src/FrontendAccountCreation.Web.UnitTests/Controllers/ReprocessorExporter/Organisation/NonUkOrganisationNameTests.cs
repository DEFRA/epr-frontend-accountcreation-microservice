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
                PagePath.TradingName,PagePath.IsUkMainAddress,PagePath.OrganisationName
            ]
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_organisationSession);
    }

    [TestMethod]
    public async Task GET_WhenNonUkOrganisationNameIsNotInSession_ThenViewIsReturnedWithoutNonUkOrganisationName()
    {
        //Act
        var result = await _systemUnderTest.OrganisationName();

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<OrganisationNameViewModel>();
        var viewModel = (OrganisationNameViewModel?)viewResult.Model;
        viewModel!.OrganisationName.Should().BeNull();
    }

    [TestMethod]
    public async Task GET_WhenNonUkOrganisationNameIsInSession_ThenViewIsReturnedWithNonUkOrganisationName()
    {
        //Arrange
        const string nonUkOrganisationName = "NonUk Company";
        _organisationSession.ReExManualInputSession = new ReExManualInputSession
        {
            OrganisationName = nonUkOrganisationName
        };

        //Act
        var result = await _systemUnderTest.OrganisationName();

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<OrganisationNameViewModel>();
        var viewModel = (OrganisationNameViewModel?)viewResult.Model;
        viewModel!.OrganisationName.Should().Be(nonUkOrganisationName);
    }

    [TestMethod]
    public async Task GET_ThenBackLinkIsCorrect()
    {
        //Act
        var result = await _systemUnderTest.OrganisationName();

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        AssertBackLink(viewResult, PagePath.IsUkMainAddress);
    }

    [TestMethod]
    public async Task POST_GivenNonUkOrganisationName_NonCompaniesHouseFlow_ThenRedirectToIsTradingNameDifferent()
    {
        // Arrange
        var request = new OrganisationNameViewModel { OrganisationName = "German Greengrocers" };
        _organisationSession.OrganisationType = OrganisationType.NonCompaniesHouseCompany;

        // Act
        var result = await _systemUnderTest.OrganisationName(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.IsTradingNameDifferent));
    }

    [TestMethod]
    public async Task POST_GivenNonUkOrganisationName_ThenUpdatesSession()
    {
        // Arrange
        var request = new OrganisationNameViewModel { OrganisationName = "German Greengrocers" };

        // Act
        await _systemUnderTest.OrganisationName(request);

        // Assert
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task POST_GivenNoNonUkOrganisationName_ThenSessionNotUpdated()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(OrganisationNameViewModel.OrganisationName), "Organisation name field is required");

        // Act
        await _systemUnderTest.OrganisationName(new OrganisationNameViewModel());

        // Assert
        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<OrganisationSession>>()),
            Times.Never);
    }

    [TestMethod]
    public async Task POST_GivenNoTradingName_ThenReturnView()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(OrganisationNameViewModel.OrganisationName), "Organisation name field is required");
        var viewModel = new OrganisationNameViewModel
        {
            OrganisationName = ""
        };

        // Act
        var result = await _systemUnderTest.OrganisationName(viewModel);

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [TestMethod]
    public async Task POST_GivenNonUkOrganisationNameTooLong_ThenReturnViewWithUsersBadInput()
    {
        // Arrange
        const string badNonUkOrganisationName = "123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789";

        _systemUnderTest.ModelState.AddModelError(nameof(OrganisationNameViewModel.OrganisationName), "Organisation name must be 170 characters or less");
        var viewModel = new OrganisationNameViewModel
        {
            OrganisationName = badNonUkOrganisationName
        };

        // Act
        var result = await _systemUnderTest.OrganisationName(viewModel);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<OrganisationNameViewModel?>();
        var resultViewModel = (OrganisationNameViewModel?)viewResult.Model;
        resultViewModel!.OrganisationName.Should().Be(badNonUkOrganisationName);
    }

    [TestMethod]
    public async Task POST_GivenNoNonUkOrganisationName_ThenViewHasCorrectBackLink()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(OrganisationNameViewModel.OrganisationName), "Organisation name field is required");

        // Act
        var result = await _systemUnderTest.OrganisationName(new OrganisationNameViewModel());

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        AssertBackLink(viewResult, PagePath.IsUkMainAddress);
    }
}
