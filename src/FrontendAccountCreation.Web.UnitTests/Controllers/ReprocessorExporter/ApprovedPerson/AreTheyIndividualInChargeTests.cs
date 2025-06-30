using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.ApprovedPerson;

[TestClass]
public class AreTheyIndividualInChargeTests : ApprovedPersonTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task Get_AreTheyIndividualInCharge_ShouldReturnViewWithCorrectModel_WhenSessionHasValueAndNotReset()
    {
        // Arrange
        var session = new OrganisationSession
        {
            AreTheyIndividualInCharge = true
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.AreTheyIndividualInCharge(resetOptions: false);

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        var model = viewResult!.Model as TheyIndividualInChargeViewModel;
        model.Should().NotBeNull();
        model!.AreTheyIndividualInCharge.Should().Be(YesNoAnswer.Yes);

        _sessionManagerMock.Verify(x => x.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task Get_AreTheyIndividualInCharge_ShouldReturnViewWithNull_WhenSessionHasNoValueOrReset()
    {
        // Arrange
        var session = new OrganisationSession
        {
            AreTheyIndividualInCharge = null
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.AreTheyIndividualInCharge(resetOptions: false);

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        var model = viewResult!.Model as TheyIndividualInChargeViewModel;
        model.Should().NotBeNull();
        model!.AreTheyIndividualInCharge.Should().BeNull();

        // Test with resetOptions true
        session.AreTheyIndividualInCharge = true;
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        result = await _systemUnderTest.AreTheyIndividualInCharge(resetOptions: true);

        viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        model = viewResult!.Model as TheyIndividualInChargeViewModel;
        model.Should().NotBeNull();
        model!.AreTheyIndividualInCharge.Should().BeNull();

        _sessionManagerMock.Verify(x => x.GetSessionAsync(It.IsAny<ISession>()), Times.Exactly(2));
    }

    [TestMethod]
    public async Task Post_AreTheyIndividualInCharge_ShouldReturnView_WhenModelStateIsInvalid()
    {
        // Arrange
        var session = new OrganisationSession();
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);
        _systemUnderTest.ModelState.AddModelError("AreTheyIndividualInCharge", "Required");

        var model = new TheyIndividualInChargeViewModel();

        // Act
        var result = await _systemUnderTest.AreTheyIndividualInCharge(model);

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult!.Model.Should().Be(model);

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Never);
    }

    [TestMethod]
    public async Task Post_AreTheyIndividualInCharge_ShouldRedirectToNonCompaniesHouseTeamMemberDetails_WhenAnswerIsYes()
    {
        // Arrange
        var session = new OrganisationSession();
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        var model = new TheyIndividualInChargeViewModel
        {
            AreTheyIndividualInCharge = YesNoAnswer.Yes
        };

        // Act
        var result = await _systemUnderTest.AreTheyIndividualInCharge(model);

        // Assert
        var redirect = result as RedirectToActionResult;
        redirect.Should().NotBeNull();
        redirect!.ActionName.Should().Be(nameof(ApprovedPersonController.NonCompaniesHouseTeamMemberDetails));
        redirect.RouteValues.Should().BeNull();
        session.AreTheyIndividualInCharge.Should().BeTrue();
    }

    [TestMethod]
    public async Task Post_AreTheyIndividualInCharge_ShouldRedirectToPersonCanNotBeInvited_WhenAnswerIsNoOrNull()
    {
        // Arrange
        var session = new OrganisationSession();
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        var model = new TheyIndividualInChargeViewModel
        {
            AreTheyIndividualInCharge = YesNoAnswer.No
        };

        // Act
        var result = await _systemUnderTest.AreTheyIndividualInCharge(model);

        // Assert
        var redirect = result as RedirectToActionResult;
        redirect.Should().NotBeNull();
        redirect!.ActionName.Should().Be(nameof(ApprovedPersonController.PersonCanNotBeInvited));
        session.AreTheyIndividualInCharge.Should().BeFalse();

        // Test with null
        model.AreTheyIndividualInCharge = null;
        result = await _systemUnderTest.AreTheyIndividualInCharge(model);

        redirect = result as RedirectToActionResult;
        redirect.Should().NotBeNull();
        redirect!.ActionName.Should().Be(nameof(ApprovedPersonController.PersonCanNotBeInvited));
        session.AreTheyIndividualInCharge.Should().BeFalse();
    }
}
