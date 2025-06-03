using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.ApprovedPerson;

[TestClass]
public class PartnerDetailsTests : ApprovedPersonTestBase
{
    private readonly Guid _testId = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task Get_PartnerDetails_WithValidId_ReturnsViewWithModel()
    {
        // Arrange
        var session = new OrganisationSession
        {
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                TeamMembers = new List<ReExCompanyTeamMember>
                {
                    new()
                    {
                        Id = _testId,
                        FirstName = "Test",
                        LastName = "User",
                        Email = "test@example.com",
                        TelephoneNumber = "1234567890"
                    }
                }
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Set up the session
        var httpContext = new DefaultHttpContext();
        var sessionMock = new Mock<ISession>();
        httpContext.Session = sessionMock.Object;

        // Set up TempData with the focus ID
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        tempData["FocusId"] = _testId.ToString();

        _systemUnderTest.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        _systemUnderTest.TempData = tempData;

        // Act
        var result = await _systemUnderTest.PartnerDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        var model = viewResult!.Model as PartnerDetailsViewModel;
        model.Should().NotBeNull();
        model!.Id.Should().Be(_testId);
        model.FirstName.Should().Be("Test");
        model.LastName.Should().Be("User");
        model.Email.Should().Be("test@example.com");
        model.Telephone.Should().Be("1234567890");
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task Get_PartnerDetails_WithNoId_ReturnsViewWithEmptyModel()
    {
        // Arrange
        var session = new OrganisationSession
        {
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                TeamMembers = new List<ReExCompanyTeamMember>()
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Set up empty query string and session
        var httpContext = new DefaultHttpContext();
        var sessionMock = new Mock<ISession>();
        httpContext.Session = sessionMock.Object;

        // Set up empty TempData
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        _systemUnderTest.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        _systemUnderTest.TempData = tempData;

        // Act
        var result = await _systemUnderTest.PartnerDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        var model = viewResult!.Model as PartnerDetailsViewModel;
        model.Should().NotBeNull();
        model!.Id.Should().BeNull();
        model.FirstName.Should().BeNull();
        model.LastName.Should().BeNull();
        model.Email.Should().BeNull();
        model.Telephone.Should().BeNull();
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task Post_PartnerDetails_WithValidModel_AddsNewMember()
    {
        // Arrange
        var session = new OrganisationSession
        {
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                TeamMembers = new List<ReExCompanyTeamMember>()
            }
        };

        var model = new PartnerDetailsViewModel
        {
            Id = _testId,
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Telephone = "1234567890"
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Set up the session
        var httpContext = new DefaultHttpContext();
        var sessionMock = new Mock<ISession>();
        httpContext.Session = sessionMock.Object;

        // Set up TempData
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        _systemUnderTest.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        _systemUnderTest.TempData = tempData;

        // Act
        var result = await _systemUnderTest.PartnerDetails(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("CheckPartnerInvitation");
        session.ReExCompaniesHouseSession.TeamMembers.Should().HaveCount(1);
        var member = session.ReExCompaniesHouseSession.TeamMembers[0];
        member.Id.Should().Be(_testId);
        member.FirstName.Should().Be("Test");
        member.LastName.Should().Be("User");
        member.Email.Should().Be("test@example.com");
        member.TelephoneNumber.Should().Be("1234567890");
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task Post_PartnerDetails_WithExistingMember_UpdatesMember()
    {
        // Arrange
        var existingMember = new ReExCompanyTeamMember
        {
            Id = _testId,
            FirstName = "Old",
            LastName = "Name",
            Email = "old@example.com",
            TelephoneNumber = "0000000000"
        };

        var session = new OrganisationSession
        {
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                TeamMembers = new List<ReExCompanyTeamMember> { existingMember }
            }
        };

        var model = new PartnerDetailsViewModel
        {
            Id = _testId,
            FirstName = "New",
            LastName = "Name",
            Email = "new@example.com",
            Telephone = "1234567890"
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Set up the session
        var httpContext = new DefaultHttpContext();
        var sessionMock = new Mock<ISession>();
        httpContext.Session = sessionMock.Object;

        // Set up TempData
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        _systemUnderTest.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        _systemUnderTest.TempData = tempData;

        // Act
        var result = await _systemUnderTest.PartnerDetails(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("CheckPartnerInvitation");
        session.ReExCompaniesHouseSession.TeamMembers.Should().HaveCount(1);
        var member = session.ReExCompaniesHouseSession.TeamMembers[0];
        member.Id.Should().Be(_testId);
        member.FirstName.Should().Be("New");
        member.LastName.Should().Be("Name");
        member.Email.Should().Be("new@example.com");
        member.TelephoneNumber.Should().Be("1234567890");
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task Post_PartnerDetails_WithInvalidModel_ReturnsViewWithModel()
    {
        // Arrange
        var session = new OrganisationSession();
        var model = new PartnerDetailsViewModel();

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Set up the session
        var httpContext = new DefaultHttpContext();
        var sessionMock = new Mock<ISession>();
        httpContext.Session = sessionMock.Object;

        // Set up TempData
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        _systemUnderTest.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        _systemUnderTest.TempData = tempData;

        _systemUnderTest.ModelState.AddModelError("FirstName", "Required");

        // Act
        var result = await _systemUnderTest.PartnerDetails(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.Model.Should().Be(model);
    }
}
