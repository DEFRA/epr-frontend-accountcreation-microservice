using System.Threading.Tasks;
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
public class UkRegulatorTests : OrganisationTestBase
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
                PagePath.UkRegulator, PagePath.NonUkRoleInOrganisation
            ],
            ReExManualInputSession = new ReExManualInputSession()
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_organisationSession);
    }

    [TestMethod]
    public async Task UkRegulator_Get_ReturnsViewWithViewModelData()
    {
        // Arrange
        _organisationSession.ReExManualInputSession.UkRegulatorNation = Nation.England;

        // Act
        var result = await _systemUnderTest.UkRegulator();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<UkRegulatorForNonUKViewModel>().Subject;
        model.UkRegulatorNation.Should().Be(Nation.England);

        _sessionManagerMock.Verify(x => x.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task UkRegulator_Get_ReturnsViewWithViewModelData_When_ReExManualInputSession_IsNull()
    {
        // Arrange
        _organisationSession.ReExManualInputSession = null;

        // Act
        var result = await _systemUnderTest.UkRegulator();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<UkRegulatorForNonUKViewModel>().Subject;
        model.UkRegulatorNation.Should().BeNull();

        _sessionManagerMock.Verify(x => x.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task UkRegulator_Get_ReturnsView_Without_AnyDataForViewModel()
    {
        // Arrange

        // Act
        var result = await _systemUnderTest.UkRegulator();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<UkRegulatorForNonUKViewModel>().Subject;
        model.UkRegulatorNation.Should().BeNull();

        _sessionManagerMock.Verify(x => x.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task UkRegulator_Post_With_InvalidModelState_ReturnsViewWithModel()
    {
        // Arrange
        var model = new UkRegulatorForNonUKViewModel();
        _systemUnderTest.ModelState.AddModelError("UkRegulatorNation", "Required");

        // Act
        var result = await _systemUnderTest.UkRegulator(model);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(model);

        _sessionManagerMock.Verify(x => x.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task UkRegulator_Post_ValidModel_SavesSessionAndRedirects()
    {
        // Arrange
        var model = new UkRegulatorForNonUKViewModel { UkRegulatorNation = Nation.Scotland };

        // Act
        var result = await _systemUnderTest.UkRegulator(model);

        // Assert
        _organisationSession.ReExManualInputSession.UkRegulatorNation.Should().Be(Nation.Scotland);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(OrganisationController.NonUkRoleInOrganisation));
        redirectResult.RouteValues?["currentPagePath"].Should().Be(PagePath.UkRegulator);
        redirectResult.RouteValues?["nextPagePath"].Should().Be(PagePath.NonUkRoleInOrganisation);

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task UkRegulator_Post_ValidModel_SavesSessionAndRedirects_With_NullSession()
    {
        // Arrange
        var model = new UkRegulatorForNonUKViewModel { UkRegulatorNation = Nation.England };
        _organisationSession.ReExManualInputSession = null;

        // Act
        var result = await _systemUnderTest.UkRegulator(model);

        // Assert
        _organisationSession.ReExManualInputSession.UkRegulatorNation.Should().Be(Nation.England);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(OrganisationController.NonUkRoleInOrganisation));
        redirectResult.RouteValues?["currentPagePath"].Should().Be(PagePath.UkRegulator);
        redirectResult.RouteValues?["nextPagePath"].Should().Be(PagePath.NonUkRoleInOrganisation);

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }
}
