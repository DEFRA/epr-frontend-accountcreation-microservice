using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.Pages.Organisation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class UkRegulatorTests : OrganisationPageModelTestBase<UkRegulator>
{
    private UkRegulator _ukRegulator;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        OrganisationSession.Journey =
        [
            PagePath.UkRegulator, PagePath.NonUkRoleInOrganisation
        ];

        OrganisationSession.ReExManualInputSession = new ReExManualInputSession();

        //OrganisationSession = new OrganisationSession
        //{
        //    Journey =
        //    [
        //        PagePath.UkRegulator, PagePath.NonUkRoleInOrganisation
        //    ],
        //    ReExManualInputSession = new ReExManualInputSession()
        //};

        //_sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_organisationSession);

        //todo: can create in base. might need virtual method to allow overriding
        _ukRegulator = new UkRegulator(SessionManagerMock.Object, SharedLocalizerMock.Object, LocalizerMock.Object)
        {
            PageContext = PageContext
        };
    }

    [TestMethod]
    //todo: test for null nation
    [DataRow(Nation.England)]
    [DataRow(Nation.NorthernIreland)]
    [DataRow(Nation.Scotland)]
    [DataRow(Nation.Wales)]
    public async Task OnGet_SetsSelectedValueCorrectly(Nation nationInSession)
    {
        // Arrange
        OrganisationSession.ReExManualInputSession.UkRegulatorNation = nationInSession;

        // Act
        await _ukRegulator.OnGet();

        // Assert
        _ukRegulator.SelectedValue.Should().Be(nationInSession.ToString());

        //todo: have separate test for varifying this?
        SessionManagerMock.Verify(x => x.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task OnGet_NationIsNull_SetsSelectedValueCorrectly()
    {
        // Act
        await _ukRegulator.OnGet();

        // Assert
        _ukRegulator.SelectedValue.Should().BeNull();

        //todo: have separate test for varifying this?
        SessionManagerMock.Verify(x => x.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task OnGet_ReExManualInputSessionIsNull_SetsSelectedValueCorrectly()
    {
        // Arrange
        OrganisationSession.ReExManualInputSession = null;

        // Act
        await _ukRegulator.OnGet();

        // Assert
        _ukRegulator.SelectedValue.Should().BeNull();

        //todo: have separate test for varifying this?
        SessionManagerMock.Verify(x => x.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task OnPost_InvalidModelState_ReturnsPageWithCorrectErrors()
    {
        // Arrange
        _ukRegulator.ModelState.AddModelError("UkRegulatorNation", "Required");

        // Act
        await _ukRegulator.OnPost();

        // Assert
        _ukRegulator.Errors.HasErrors.Should().BeTrue();
        _ukRegulator.Errors.Should().NotBeNull();
        _ukRegulator.Errors.Errors.Should().ContainSingle(e => e.HtmlElementId == "UkRegulatorNation");
        _ukRegulator.Errors.Errors.Should().ContainSingle(e => e.Message == "Required");
    }

    //[TestMethod]
    //public async Task UkRegulator_Post_ValidModel_SavesSessionAndRedirects()
    //{
    //    // Arrange
    //    var model = new UkRegulatorForNonUKViewModel { UkRegulatorNation = Nation.Scotland };

    //    // Act
    //    var result = await _systemUnderTest.UkRegulator(model);

    //    // Assert
    //    _organisationSession.ReExManualInputSession.UkRegulatorNation.Should().Be(Nation.Scotland);

    //    var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
    //    redirectResult.ActionName.Should().Be(nameof(OrganisationController.NonUkRoleInOrganisation));
    //    redirectResult.RouteValues?["currentPagePath"].Should().Be(PagePath.UkRegulator);
    //    redirectResult.RouteValues?["nextPagePath"].Should().Be(PagePath.NonUkRoleInOrganisation);

    //    _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    //}

    //[TestMethod]
    //public async Task UkRegulator_Post_ValidModel_SavesSessionAndRedirects_With_NullSession()
    //{
    //    // Arrange
    //    var model = new UkRegulatorForNonUKViewModel { UkRegulatorNation = Nation.England };
    //    _organisationSession.ReExManualInputSession = null;

    //    // Act
    //    var result = await _systemUnderTest.UkRegulator(model);

    //    // Assert
    //    _organisationSession.ReExManualInputSession.UkRegulatorNation.Should().Be(Nation.England);

    //    var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
    //    redirectResult.ActionName.Should().Be(nameof(OrganisationController.NonUkRoleInOrganisation));
    //    redirectResult.RouteValues?["currentPagePath"].Should().Be(PagePath.UkRegulator);
    //    redirectResult.RouteValues?["nextPagePath"].Should().Be(PagePath.NonUkRoleInOrganisation);

    //    _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    //}
}
