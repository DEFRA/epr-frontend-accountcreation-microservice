using FluentAssertions;
using FluentAssertions;
using FrontendAccountCreation;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using global::FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class RoleInOrganisationTests : OrganisationTestBase
{

    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task RoleInOrganisation_RoleSavedAsNone_RedirectsToCannotCreateAccount_AndUpdateSession()
    {
        // Arrange
        var request = new RoleInOrganisationViewModel() { RoleInOrganisation = RoleInOrganisation.NoneOfTheAbove};

        // Act
        var result = await _systemUnderTest.RoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        //TODO: Uncomment when Cannot create Account is Scoped.
        //((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.CannotCreateAccount));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task RoleInOrganisation_RoleSavedAsDirector_RedirectsToManageAccount_AndUpdateSession()
    {
        // Arrange
        var request = new RoleInOrganisationViewModel() { RoleInOrganisation = RoleInOrganisation.Director };

        // Act
        var result = await _systemUnderTest.RoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.AddApprovedPerson));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task RoleInOrganisation_RoleSavedAsPartner_RedirectsToManageAccount_AndUpdateSession()
    {
        // Arrange
        var request = new RoleInOrganisationViewModel() { RoleInOrganisation = RoleInOrganisation.Partner };

        // Act
        var result = await _systemUnderTest.RoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.AddApprovedPerson));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task RoleInOrganisation_RoleSavedAsMember_RedirectsToManageAccount_AndUpdateSession()
    {
        // Arrange
        var request = new RoleInOrganisationViewModel() { RoleInOrganisation = RoleInOrganisation.Member };

        // Act
        var result = await _systemUnderTest.RoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.AddApprovedPerson));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task RoleInOrganisation_RoleSavedAsCompanySecretary_RedirectsToManageAccount_AndUpdateSession()
    {
        // Arrange
        var request = new RoleInOrganisationViewModel() { RoleInOrganisation = RoleInOrganisation.CompanySecretary };

        // Act
        var result = await _systemUnderTest.RoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.AddApprovedPerson));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task RoleInOrganisation_OrganisationRoleSavedWithNoAnswer_ReturnsViewWithError()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(RoleInOrganisationViewModel.RoleInOrganisation), "Field is required");

        // Act
        var result = await _systemUnderTest.RoleInOrganisation(new RoleInOrganisationViewModel());

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<RoleInOrganisationViewModel>();

        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<OrganisationSession>>()), Times.Never);
    }
}
