using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.Pages.Re_Ex.Organisation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

//to-do: have base test helper that is fed from page itself

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

        //todo: can create in base. might need virtual method to allow overriding
        _ukRegulator = new UkRegulator(SessionManagerMock.Object, SharedLocalizerMock.Object, LocalizerMock.Object)
        {
            PageContext = PageContext
        };
    }

    [TestMethod]
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

        //to-do: have separate test for varifying this?
        SessionManagerMock.Verify(x => x.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task OnGet_NationIsNull_SetsSelectedValueCorrectly()
    {
        // Act
        await _ukRegulator.OnGet();

        // Assert
        _ukRegulator.SelectedValue.Should().BeNull();

        //to-do: have separate test for varifying this?
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

        //to-do: have separate test for varifying this?
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

    [TestMethod]
    public async Task OnPost_ValidModel_SavesSessionAndRedirects()
    {
        // Arrange
        _ukRegulator.SelectedValue = Nation.Wales.ToString();

        // Act
        var result = await _ukRegulator.OnPost();

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(OrganisationController.NonUkRoleInOrganisation));
        redirectResult.RouteValues?["currentPagePath"].Should().Be(PagePath.UkRegulator);
        redirectResult.RouteValues?["nextPagePath"].Should().Be(PagePath.NonUkRoleInOrganisation);

        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public void Radios_ShouldReturnCorrectRadios()
    {
        // Act
        var radios = _ukRegulator.Radios.ToList();

        // Assert
        radios.Should().HaveCount(4);
        radios[0].Value.Should().Be("England");
        radios[1].Value.Should().Be("Scotland");
        radios[2].Value.Should().Be("Wales");
        radios[3].Value.Should().Be("NorthernIreland");
        //to-do: setup shared localizer and check localized strings
    }

    [TestMethod]
    public void Question_ShouldReturnLocalizedQuestion()
    {
        // Arrange
        LocalizerMock.Setup(l => l["UkRegulator.Question"])
            .Returns(new LocalizedString("UkRegulator.Question", "Test question string"));

        // Act
        var question = _ukRegulator.Question;

        // Assert
        question.Should().Be("Test question string");
    }

    [TestMethod]
    public void Hint_ShouldReturnLocalizedDescription()
    {
        // Arrange
        LocalizerMock.Setup(l => l["UkRegulator.Hint"])
            .Returns(new LocalizedString("UkRegulator.Hint", "Test hint string"));

        // Act
        var hint = _ukRegulator.Hint;

        // Assert
        hint.Should().Be("Test hint string");
    }
}
