using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class NonUkRoleInOrganisationTests : OrganisationTestBase
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
                PagePath.TradingName,PagePath.IsUkMainAddress,PagePath.OrganisationName,PagePath.NonUkRoleInOrganisation,
                PagePath.NotImplemented
            ]
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_organisationSession);
    }

    [TestMethod]
    public async Task GET_WhenNonUkRoleInOrganisationIsNotInSession_ThenViewIsReturnedWithoutNonUkRoleInOrganisation()
    {
        //Act
        var result = await _systemUnderTest.NonUkRoleInOrganisation();

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<NonUkRoleInOrganisationViewModel>();
        var viewModel = (NonUkRoleInOrganisationViewModel?)viewResult.Model;
        viewModel!.NonUkRoleInOrganisation.Should().BeNull();
    }

    [TestMethod]
    public async Task GET_WhenNonUkRoleInOrganisationIsInSession_ThenViewIsReturnedWithNonUkRoleInOrganisation()
    {
        //Arrange
        const string nonUkRoleInOrganisation = "Employee";
        _organisationSession.ReExManualInputSession = new ReExManualInputSession
        {
            NonUkRoleInOrganisation = nonUkRoleInOrganisation
        };

        //Act
        var result = await _systemUnderTest.NonUkRoleInOrganisation();

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<NonUkRoleInOrganisationViewModel>();
        var viewModel = (NonUkRoleInOrganisationViewModel?)viewResult.Model;
        viewModel!.NonUkRoleInOrganisation.Should().Be(nonUkRoleInOrganisation);
    }

    [TestMethod]
    public async Task POST_GivenNonUkRoleInOrganisation_NonCompaniesHouseFlow_ThenRedirectToIsTradingNameDifferent()
    {
        // Arrange
        var request = new NonUkRoleInOrganisationViewModel { NonUkRoleInOrganisation = "Employee" };
        _organisationSession.OrganisationType = OrganisationType.NonCompaniesHouseCompany;

        // Act
        var result = await _systemUnderTest.NonUkRoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.ManageControl));
    }

    [TestMethod]
    public async Task POST_GivenNonUkRoleInOrganisation_ThenUpdatesSession()
    {
        // Arrange
        var request = new NonUkRoleInOrganisationViewModel { NonUkRoleInOrganisation = "Employee" };

        // Act
        await _systemUnderTest.NonUkRoleInOrganisation(request);

        // Assert
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task POST_GivenNoNonUkRoleInOrganisation_ThenSessionNotUpdated()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(NonUkRoleInOrganisationViewModel.NonUkRoleInOrganisation), "Role in organisation field is required");

        // Act
        await _systemUnderTest.NonUkRoleInOrganisation(new NonUkRoleInOrganisationViewModel());

        // Assert
        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<OrganisationSession>>()),
            Times.Never);
    }

    [TestMethod]
    public async Task POST_GivenNoNonUkRoleInOrganisation_ThenViewHasCorrectBackLink()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(NonUkRoleInOrganisationViewModel.NonUkRoleInOrganisation), "Role in organisation field is required");

        // Act
        var result = await _systemUnderTest.NonUkRoleInOrganisation(new NonUkRoleInOrganisationViewModel());

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        AssertBackLink(viewResult, PagePath.OrganisationName);
    }

    [TestMethod]
    public async Task GET_WhenNonUkRoleInOrganisationIsPresent_ThenViewModelIsPopulated()
    {
        // Arrange
        const string expectedRole = "CEO";
        _organisationSession.ReExManualInputSession = new ReExManualInputSession
        {
            NonUkRoleInOrganisation = expectedRole
        };

        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(_organisationSession);

        // Act
        var result = await _systemUnderTest.NonUkRoleInOrganisation();

        // Assert
        var viewResult = (ViewResult)result;
        var viewModel = (NonUkRoleInOrganisationViewModel)viewResult.Model!;
        viewModel.NonUkRoleInOrganisation.Should().Be(expectedRole);
    }

    [TestMethod]
    public async Task GET_WhenReExManualInputSessionIsNull_ThenViewModelHasNullRole()
    {
        // Arrange
        _organisationSession.ReExManualInputSession = null;

        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(_organisationSession);

        // Act
        var result = await _systemUnderTest.NonUkRoleInOrganisation();

        // Assert
        var viewResult = (ViewResult)result;
        var viewModel = (NonUkRoleInOrganisationViewModel)viewResult.Model!;
        viewModel.NonUkRoleInOrganisation.Should().BeNull();
    }
}
