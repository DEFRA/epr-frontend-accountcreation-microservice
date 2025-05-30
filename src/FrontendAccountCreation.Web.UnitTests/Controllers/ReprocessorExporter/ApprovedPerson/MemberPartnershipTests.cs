using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.ViewModels;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.ApprovedPerson;

[TestClass]
public class MemberPartnershipTests : ApprovedPersonTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task Get_MemberPartnership_Returns_View()
    {
        // Arrange
        var session = new OrganisationSession();
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.MemberPartnership();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
    }

    [TestMethod]
    public async Task Post_MemberPartnership_WhenIsMemberPartnershipIsYes_RedirectsToPartnerDetails()
    {
        // Arrange
        var session = new OrganisationSession();
        var model = new IsMemberPartnershipViewModel { IsMemberPartnership = YesNoAnswer.Yes };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.MemberPartnership(model);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
    }

    [TestMethod]
    public async Task Post_MemberPartnership_WhenIsMemberPartnershipIsNo_RedirectsToCanNotInviteThisPerson()
    {
        // Arrange
        var session = new OrganisationSession();
        var model = new IsMemberPartnershipViewModel { IsMemberPartnership = YesNoAnswer.No };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.MemberPartnership(model);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
    }

    [TestMethod]
    public async Task Post_MemberPartnership_WhenModelStateIsInvalid_ReturnsViewWithModel()
    {
        // Arrange
        var session = new OrganisationSession();
        var model = new IsMemberPartnershipViewModel();

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _systemUnderTest.ModelState.AddModelError("IsMemberPartnership", "Required");

        // Act
        var result = await _systemUnderTest.MemberPartnership(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.Model.Should().Be(model);
    }
}
