namespace FrontendAccountCreation.Web.UnitTests.Controllers.AccountCreation;

using Core.Constants;
using Core.Services.FacadeModels;
using Core.Sessions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Web.ViewModels.AccountCreation;

[TestClass]
public class InvitationTests : AccountCreationTestBase
{
    private readonly AccountCreationSession _accountCreationSessionMock = new();
    
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_accountCreationSessionMock);
    }

    [TestMethod]
    public async Task PostFullName_GivenFullNameProvided_WhenPageIsPosted_ThenRedirectsToLandingPage()
    {
        // Arrange
        _accountCreationSessionMock.InviteToken = "invite-token";
        var request = new FullNameViewModel { FirstName = "John", LastName = "Smith", PostAction = "InviteeFullName" };
        var expectedInviteeDetails = new EnrolInvitedUserModel
        {
            FirstName = "John",
            LastName = "Smith",
            InviteToken = "invite-token"
        };

        // Act
        var result = await _systemUnderTest.InviteeFullName(request);
        
        // Assert
        result.Should().BeOfType<RedirectResult>();
        ((RedirectResult)result).Url.Should().Be("/report-data");
        _facadeServiceMock
            .Verify(x => x.PostEnrolInvitedUserAsync(It.Is<EnrolInvitedUserModel>(x => 
                    x.FirstName == expectedInviteeDetails.FirstName &&
                    x.LastName == expectedInviteeDetails.LastName &&
                    x.InviteToken == expectedInviteeDetails.InviteToken))
                , Times.Once);
    }
    
    [TestMethod]
    public async Task PostFullName_GivenNoFirstName_WhenInviteeFullNamePosted_ThenReturnViewWithError()
    {
        // Arrange
        _accountCreationSessionMock.InviteToken = "invite-token";
        _systemUnderTest.ModelState.AddModelError(nameof(FullNameViewModel.FirstName),
            "Field is required");
        
        var request = new FullNameViewModel { LastName = "Smith", PostAction = "InviteeFullName" };

        // Act
        var result = await _systemUnderTest.InviteeFullName(request);
        
        // Assert
        result.Should().BeOfType<ViewResult>();
        ((ViewResult)result).ViewName.Should().Be("FullName");
        _facadeServiceMock.Verify(x => x.PostEnrolInvitedUserAsync(It.IsAny<EnrolInvitedUserModel>()), Times.Never);
    }
    
    [TestMethod]
    public async Task PostFullName_GivenNoLastName_WhenInviteeFullNamePosted_ThenReturnViewWithError()
    {
        // Arrange
        _accountCreationSessionMock.InviteToken = "invite-token";
        _systemUnderTest.ModelState.AddModelError(nameof(FullNameViewModel.LastName),
            "Field is required");
        
        var request = new FullNameViewModel { FirstName = "John", PostAction = "InviteeFullName" };

        // Act
        var result = await _systemUnderTest.InviteeFullName(request);
        
        // Assert
        result.Should().BeOfType<ViewResult>();
        ((ViewResult)result).ViewName.Should().Be("FullName");
        _facadeServiceMock.Verify(x => x.PostEnrolInvitedUserAsync(It.IsAny<EnrolInvitedUserModel>()), Times.Never);
    }
    
    [TestMethod]
    public async Task Invitation_GivenTokenProvided_ThenTempDataSet_AndRedirectsToFullNameAction()
    {
        // Arrange
        var inviteToken = "an-invite-token";
        var mockTempData = new Mock<ITempDataDictionary>();
        _systemUnderTest.TempData = mockTempData.Object;

        // Act
        var result = await _systemUnderTest.Invitation(inviteToken);
        
        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        result.ActionName.Should().Be("InviteeFullName");
        mockTempData.VerifySet(x => x["InviteToken"] = inviteToken, Times.Once);
    }
    
    [TestMethod]
    public async Task GetInviteeFullName_GivenInviteTokenInTempData_WhenInviteeFullNameGet_ThenRedirectsToInvitationAction()
    {
        // Arrange
        var inviteToken = "an-invite-token";
        var mockTempData = new Mock<ITempDataDictionary>();
        mockTempData.Setup(x => x["InviteToken"]).Returns(inviteToken);
        _systemUnderTest.TempData = mockTempData.Object;
        
        // Act
        var result = await _systemUnderTest.InviteeFullName();
        
        // Assert
        result.Should().BeOfType<ViewResult>();
        ((ViewResult)result).ViewName.Should().Be("FullName");
    }
    
    [TestMethod]
    public async Task GetInviteeFullName_GivenInviteTokenNotInTempData_OrInSession_WhenInviteeFullNameGet_ThenRedirectsToError()
    {
        // Arrange
        var inviteToken = "an-invite-token";
        var mockTempData = new Mock<ITempDataDictionary>();
        _systemUnderTest.TempData = mockTempData.Object;
        
        // Act
        var result = await _systemUnderTest.InviteeFullName();
        
        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be("Error");
    }
    
    [TestMethod]
    public async Task GetInviteeFullName_GivenInviteTokenNotInTempData_ButInSession_WhenInviteeFullNameGet_ThenRedirectsToView()
    {
        // Arrange
        var inviteToken = "an-invite-token";
        var mockTempData = new Mock<ITempDataDictionary>();
        _systemUnderTest.TempData = mockTempData.Object;
        _accountCreationSessionMock.InviteToken = "invite-token";
        
        // Act
        var result = await _systemUnderTest.InviteeFullName();
        
        // Assert
        result.Should().BeOfType<ViewResult>();
        ((ViewResult)result).ViewName.Should().Be("FullName");
    }
    
    [TestMethod]
    [DataRow(EnrolmentStatus.Enrolled)]
    [DataRow(EnrolmentStatus.NotSet)]
    [DataRow(EnrolmentStatus.Rejected)]
    [DataRow(EnrolmentStatus.Nominated)]
    [DataRow(EnrolmentStatus.Approved)]
    [DataRow(EnrolmentStatus.Pending)]
    [DataRow(EnrolmentStatus.NotSet)]
    [DataRow(EnrolmentStatus.OnHold)]
    public async Task GetInviteeFullName_GivenUserEnrolmentStatusIsNotInvited_ThenRedirectsToReportDataUrl(string enrolmentStatus)
    {
        // Arrange
        var userAccountModel = CreateUserAccountModel(enrolmentStatus);
        _facadeServiceMock.Setup(x => x.GetUserAccount()).ReturnsAsync(userAccountModel);
        
        // Act
        var result = await _systemUnderTest.InviteeFullName();
        
        // Assert
        result.Should().BeOfType<RedirectResult>();
        result.As<RedirectResult>().Url.Should().Be("/report-data");
    }
    
    [TestMethod]
    public async Task GetInviteeFullName_GivenUserEnrolmentStatusIsInvited_ThenDoesNotRedirectToReportDataUrl()
    {
        // Arrange
        var userAccountModel = CreateUserAccountModel(EnrolmentStatus.Invited);
        _facadeServiceMock.Setup(x => x.GetUserAccount()).ReturnsAsync(userAccountModel);
        
        // Act
        var result = await _systemUnderTest.InviteeFullName();
        
        // Assert
        result.Should().NotBeOfType<RedirectResult>();
    }
}
