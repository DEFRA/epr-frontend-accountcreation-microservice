using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount.Unincorporated;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class UnincorporatedRoleInOrganisationTests : OrganisationTestBase
{
    private OrganisationSession _organisationSession = null!;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _organisationSession = new OrganisationSession
        {
            Journey = new List<string> { PagePath.BusinessAddress, PagePath.UnincorporatedRoleInOrganisation },
            ReExManualInputSession = new ReExManualInputSession()
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_organisationSession);
    }

    [TestMethod]
    public async Task UnincorporatedRoleInOrganisation_Get_ReturnsViewWithPrepopulatedModel()
    {
        // Arrange
        var role = "test";
        _organisationSession.ReExManualInputSession = new ReExManualInputSession
        {
            RoleInUnincorporatedOrganisation = role,
        };

        // Act
        var result = await _systemUnderTest.UnincorporatedRoleInOrganisation();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ReExRoleInOrganisationViewModel>().Subject;

        AssertBackLink(viewResult, PagePath.BusinessAddress);
        model.Role.Should().Be(role);
    }

    [TestMethod]
    public async Task UnincorporatedRoleInOrganisation_Post_WithInvalidInput_ReturnView()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError("Role", "Test");

        var viewModel = new ReExRoleInOrganisationViewModel();

        // Act
        var result = await _systemUnderTest.UnincorporatedRoleInOrganisation(viewModel);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        AssertBackLink(viewResult, PagePath.BusinessAddress);
        viewResult.Model.Should().Be(viewModel);
    }

    [TestMethod]
    public async Task RoleInOrganisation_Post_ReturnRedirect()
    {
        // Arrange
        var viewModel = new ReExRoleInOrganisationViewModel { Role = "test" };

        // Act
        var result = await _systemUnderTest.UnincorporatedRoleInOrganisation(viewModel);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(OrganisationController.ManageControl));

        _organisationSession.ReExManualInputSession.RoleInUnincorporatedOrganisation.Should().Be(viewModel.Role);
    }
    
    [TestMethod]
    public async Task RoleInOrganisation_PostWithNullReExManualInputSession_ReturnRedirect()
    {
        // Arrange
        var viewModel = new ReExRoleInOrganisationViewModel { Role = "test" };
        var organisationSession = new OrganisationSession
        {
            Journey = new List<string> { PagePath.BusinessAddress, PagePath.UnincorporatedRoleInOrganisation },
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(organisationSession);

        // Act
        var result = await _systemUnderTest.UnincorporatedRoleInOrganisation(viewModel);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(OrganisationController.ManageControl));

        organisationSession.ReExManualInputSession.RoleInUnincorporatedOrganisation.Should().Be(viewModel.Role);
    }
}