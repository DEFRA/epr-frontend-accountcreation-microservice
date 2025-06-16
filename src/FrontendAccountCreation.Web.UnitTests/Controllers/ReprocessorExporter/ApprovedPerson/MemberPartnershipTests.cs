using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
        var session = new OrganisationSession
        {
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession()
        };

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
    public async Task Post_MemberPartnership_WhenMemberExistsAndSaysNo_RedirectsToCannotInvite()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var session = new OrganisationSession
        {
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                TeamMembers = new List<ReExCompanyTeamMember>
                {
                    new() { Id = memberId, Email = "test@test.com" }
                }
            }
        };

        var model = new IsMemberPartnershipViewModel
        {
            Id = memberId,
            IsMemberPartnership = YesNoAnswer.No
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.MemberPartnership(model);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("CanNotInviteThisPerson");
    }

    [TestMethod]
    public async Task Post_MemberPartnership_WhenNewMemberAndSaysYes_AddsMemberAndRedirectsToPartnerDetails()
    {
        // Arrange
        var session = new OrganisationSession
        {
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                TeamMembers = new List<ReExCompanyTeamMember>()
            }
        };

        var model = new IsMemberPartnershipViewModel
        {
            Id = null,
            IsMemberPartnership = YesNoAnswer.Yes
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.MemberPartnership(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        session.ReExCompaniesHouseSession.TeamMembers.Should().ContainSingle();
    }

    [TestMethod]
    public async Task Post_MemberPartnership_WhenExistingMemberAndSaysYes_UpdatesRoleAndRedirectsToPartnerDetails()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var session = new OrganisationSession
        {
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                TeamMembers = new List<ReExCompanyTeamMember>
                {
                    new() { Id = memberId, Role = ReExTeamMemberRole.CompanySecretary }
                }
            }
        };

        var model = new IsMemberPartnershipViewModel
        {
            IsMemberPartnership = YesNoAnswer.Yes
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        var tempDataMock = new Mock<ITempDataDictionary>();
        var tempDataStore = new Dictionary<string, object>();
        tempDataMock.Setup(t => t[It.IsAny<string>()])
            .Returns((string key) => tempDataStore.GetValueOrDefault(key));
        tempDataMock.SetupSet(t => t[It.IsAny<string>()] = It.IsAny<object>())
            .Callback((string key, object value) => tempDataStore[key] = value);

        _systemUnderTest.TempData = tempDataMock.Object;
        _systemUnderTest.SetFocusId(memberId);

        // Act
        var result = await _systemUnderTest.MemberPartnership(model);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(_systemUnderTest.PartnerDetails));
        session.ReExCompaniesHouseSession.TeamMembers!.First().Role.Should().Be(ReExTeamMemberRole.Member);
    }

    [TestMethod]
    public async Task Post_MemberPartnership_WhenNewMemberAndSaysNo_DoesNotAddMemberAndRedirectsToCannotInvite()
    {
        // Arrange
        var session = new OrganisationSession
        {
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                TeamMembers = new List<ReExCompanyTeamMember>()
            }
        };

        var model = new IsMemberPartnershipViewModel
        {
            Id = null,
            IsMemberPartnership = YesNoAnswer.No
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.MemberPartnership(model);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("CanNotInviteThisPerson");
        session.ReExCompaniesHouseSession.TeamMembers.Should().BeEmpty();
    }

    [TestMethod]
    public async Task MemberPartnershipEdit_Get_RedirectsTo_TeamMemberRoleInOrganisation()
    {
        var result = await _systemUnderTest.MemberPartnershipEdit(Guid.NewGuid());

        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(ApprovedPersonController.MemberPartnership));
    }

    [TestMethod]
    public async Task Get_MemberPartnership_WithFocusId_ExistingTeamMember_SetsModelToYes()
    {
        // Arrange
        var focusId = Guid.NewGuid();
        var session = new OrganisationSession
        {
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                TeamMembers = new List<ReExCompanyTeamMember>
                {
                    new ReExCompanyTeamMember { Id = focusId, Role = ReExTeamMemberRole.Member }
                }
            }
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        var tempDataMock = new Mock<ITempDataDictionary>();
        var tempDataStorage = new Dictionary<string, object>();
        tempDataMock.Setup(t => t[It.IsAny<string>()])
            .Returns((string key) => tempDataStorage.TryGetValue(key, out var value) ? value : null);
        tempDataMock.SetupSet(t => t[It.IsAny<string>()] = It.IsAny<object>())
            .Callback((string key, object value) => tempDataStorage[key] = value);
        _systemUnderTest.TempData = tempDataMock.Object;
        _systemUnderTest.SetFocusId(focusId);

        // Act
        var result = await _systemUnderTest.MemberPartnership();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model.Should().BeOfType<IsMemberPartnershipViewModel>().Subject;
        model.Id.Should().BeNull(); 
        model.IsMemberPartnership.Should().Be(YesNoAnswer.Yes);
        _systemUnderTest.GetFocusId().Should().Be(focusId);
    }

    [TestMethod]
    public async Task Get_MemberPartnership_WithFocusId_NonExistingTeamMember_SetsModelToNo()
    {
        var focusId = Guid.NewGuid();
        var session = new OrganisationSession
        {
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                TeamMembers = new List<ReExCompanyTeamMember>()
            }
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        var tempDataMock = new Mock<ITempDataDictionary>();
        var tempDataStorage = new Dictionary<string, object>();
        tempDataMock.Setup(t => t[It.IsAny<string>()])
            .Returns((string key) => tempDataStorage.TryGetValue(key, out var value) ? value : null);
        tempDataMock.SetupSet(t => t[It.IsAny<string>()] = It.IsAny<object>())
            .Callback((string key, object value) => tempDataStorage[key] = value);
        _systemUnderTest.TempData = tempDataMock.Object;
        _systemUnderTest.SetFocusId(focusId);

        var result = await _systemUnderTest.MemberPartnership();

        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model.Should().BeOfType<IsMemberPartnershipViewModel>().Subject;
        model.Id.Should().BeNull();
        model.IsMemberPartnership.Should().Be(YesNoAnswer.No);
        _systemUnderTest.GetFocusId().Should().Be(focusId);
    }
   
    [TestMethod]
    public async Task Get_MemberPartnership_NoFocusId_ViewModelIsMemberPartnershipIsNull()
    {
        // Arrange
        var session = new OrganisationSession
        {
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                TeamMembers = new List<ReExCompanyTeamMember>()
            }
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        var tempDataMock = new Mock<ITempDataDictionary>();
        tempDataMock.Setup(t => t["FocusId"]).Returns(null);
        _systemUnderTest.TempData = tempDataMock.Object;

        // Act
        var result = await _systemUnderTest.MemberPartnership();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model.Should().BeOfType<IsMemberPartnershipViewModel>().Subject;
        model.Id.Should().BeNull();
        model.IsMemberPartnership.Should().BeNull();
    }

    [TestMethod]
    public async Task Get_MemberPartnership_ReExCompaniesHouseSessionIsNull_ViewModelIsMemberPartnershipIsNull()
    {
        // Arrange
        var session = new OrganisationSession
        {
            ReExCompaniesHouseSession = null
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        var tempDataMock = new Mock<ITempDataDictionary>();
        tempDataMock.Setup(t => t["FocusId"]).Returns(Guid.NewGuid().ToString()); // Has a focus ID
        _systemUnderTest.TempData = tempDataMock.Object;

        // Act
        var result = await _systemUnderTest.MemberPartnership();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model.Should().BeOfType<IsMemberPartnershipViewModel>().Subject;
        model.Id.Should().BeNull(); // FocusId check will return null if session.ReExCompaniesHouseSession is null
        model.IsMemberPartnership.Should().BeNull();
    }

    [TestMethod]
    public async Task Get_MemberPartnership_ReExCompaniesHouseSessionTeamMembersIsNull_ViewModelIsMemberPartnershipIsNull()
    {
        var session = new OrganisationSession
        {
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                TeamMembers = null
            }
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        var tempDataMock = new Mock<ITempDataDictionary>();
        tempDataMock.Setup(t => t["FocusId"]).Returns(Guid.NewGuid().ToString());
        _systemUnderTest.TempData = tempDataMock.Object;

        var result = await _systemUnderTest.MemberPartnership();

        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model.Should().BeOfType<IsMemberPartnershipViewModel>().Subject;
        model.Id.Should().BeNull();
        model.IsMemberPartnership.Should().BeNull();
    }
}
