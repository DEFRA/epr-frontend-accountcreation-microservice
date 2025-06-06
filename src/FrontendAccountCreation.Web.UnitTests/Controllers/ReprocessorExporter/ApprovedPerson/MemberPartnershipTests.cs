using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
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
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
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
        redirectResult.ActionName.Should().Be(nameof(_systemUnderTest.PartnerDetails));
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
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
        redirectResult.ActionName.Should().Be("CanNotInviteThisPerson");
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
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

    [TestMethod]
    public async Task MemberPartnershipAdd_Get_RedirectsTo_MemberPartnership()
    {
         // Act
        var result = await _systemUnderTest.MemberPartnershipAdd();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(ApprovedPersonController.MemberPartnership));
    }

    [TestMethod]
    public async Task MemberPartnershipEdit_Get_RedirectsTo_TeamMemberRoleInOrganisation()
    {
        var result = await _systemUnderTest.MemberPartnershipEdit(Guid.NewGuid());

        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(ApprovedPersonController.MemberPartnership));
    }
}
