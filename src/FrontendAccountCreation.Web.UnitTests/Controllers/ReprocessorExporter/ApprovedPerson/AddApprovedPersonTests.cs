using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.ApprovedPerson;

[TestClass]
public class AddApprovedPersonTests : ApprovedPersonTestBase
{
    private OrganisationSession _orgSessionMock;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _orgSessionMock = new OrganisationSession
        {
            Journey = new List<string>
            {
                PagePath.RegisteredAsCharity,
                PagePath.RegisteredWithCompaniesHouse,
                PagePath.CompaniesHouseNumber,
                PagePath.ConfirmCompanyDetails,
                PagePath.RoleInOrganisation,
                "Pagebefore",
                PagePath.TeamMemberRoleInOrganisation,
            },
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession(),
            IsUserChangingDetails = false,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_orgSessionMock);
    }

    [TestMethod]
    public async Task AddApprovedPerson_SessionRetrievedAndSaved_ReturnsView()
    {
        // Arrange
        var session = new OrganisationSession();
        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _sessionManagerMock
            .Setup(s => s.SaveSessionAsync(It.IsAny<ISession>(), session))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.AddApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
        _sessionManagerMock.Verify(s => s.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
        _sessionManagerMock.Verify(s => s.SaveSessionAsync(It.IsAny<ISession>(), session), Times.Once);
    }

    [TestMethod]
    public async Task AddApprovedPerson_ModelStateInvalid_ReturnsViewWithModel()
    {
        // Arrange
        var model = new AddApprovedPersonViewModel();
        _systemUnderTest.ModelState.AddModelError("InviteUserOption", "Required");

        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new OrganisationSession());

        // Act
        var result = await _systemUnderTest.AddApprovedPerson(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType(model.GetType());
    }

    [TestMethod]
    public async Task AddApprovedPerson_IAgreeToBeApprovedPerson_RedirectsToYouAreApprovedPerson()
    {
        // Arrange
        var model = new AddApprovedPersonViewModel
        {
            InviteUserOption = InviteUserOptions.BeAnApprovedPerson.ToString()
        };

        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new OrganisationSession());

        // Act
        var result = await _systemUnderTest.AddApprovedPerson(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = (RedirectToActionResult)result;
        redirect.ActionName.Should().Be(nameof(ApprovedPersonController.YouAreApprovedPerson));
    }


    [TestMethod]
    public async Task AddApprovedPerson_InviteAnotherApprovedPerson_RedirectsToTeamMemberRoleInOrganisation()
    {
        // Arrange
        var model = new AddApprovedPersonViewModel
        {
            InviteUserOption = InviteUserOptions.InviteAnotherPerson.ToString()
        };

        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new OrganisationSession());

        // Act
        var result = await _systemUnderTest.AddApprovedPerson(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = (RedirectToActionResult)result;
        redirect.ActionName.Should().Be(nameof(_systemUnderTest.TeamMemberRoleInOrganisation));
    }

    [TestMethod]
    public async Task AddApprovedPerson_InviteApprovedPersonLater_RedirectsToCheckYourDetails()
    {
        // Arrange
        var model = new AddApprovedPersonViewModel
        {
            InviteUserOption = InviteUserOptions.InviteLater.ToString()
        };

        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new OrganisationSession());

        // Act
        var result = await _systemUnderTest.AddApprovedPerson(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = (RedirectToActionResult)result;
        redirect.ActionName.Should().Be("CheckYourDetails");
    }

    [TestMethod]
    public async Task AddApprovedPerson_Get_PartnershipAndIneligible_ReturnsInEligibleView()
    {
        // Arrange
        _orgSessionMock.IsOrganisationAPartnership = true;
        _orgSessionMock.ReExCompaniesHouseSession = new ReExCompaniesHouseSession
        {
            IsInEligibleToBeApprovedPerson = true,
            IsComplianceScheme = true
        };

        // Act
        var result = await _systemUnderTest.AddApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.ViewName.Should().Be("InEligibleAddNotApprovedPerson");
    }

    [TestMethod]
    public async Task AddApprovedPerson_Get_PartnershipOnly_ReturnsLimitedPartnershipView()
    {
        // Arrange
        _orgSessionMock.IsOrganisationAPartnership = true;
        _orgSessionMock.ReExCompaniesHouseSession = new ReExCompaniesHouseSession
        {
            IsInEligibleToBeApprovedPerson = false,
            IsComplianceScheme = false
        };

        // Act
        var result = await _systemUnderTest.AddApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.ViewName.Should().Be("LimitedPartnershipAddApprovedPerson");
    }

    [TestMethod]
    public async Task AddApprovedPerson_Get_NotPartnershipButIneligible_ReturnsAddNotApprovedPersonView()
    {
        // Arrange
        _orgSessionMock.IsOrganisationAPartnership = false;
        _orgSessionMock.ReExCompaniesHouseSession = new ReExCompaniesHouseSession
        {
            IsInEligibleToBeApprovedPerson = true
        };

        // Act
        var result = await _systemUnderTest.AddApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.ViewName.Should().Be("AddNotApprovedPerson");
    }

    [TestMethod]
    public async Task AddApprovedPerson_Get_NotPartnershipAndNotIneligible_ReturnsDefaultView()
    {
        // Arrange
        _orgSessionMock.IsOrganisationAPartnership = false;
        _orgSessionMock.ReExCompaniesHouseSession = new ReExCompaniesHouseSession
        {
            IsInEligibleToBeApprovedPerson = false
        };

        // Act
        var result = await _systemUnderTest.AddApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.ViewName.Should().BeNull(); // default view
    }

    [TestMethod]
    public async Task AddApprovedPerson_ModelInvalid_IsPartnership_ReturnsLimitedPartnershipAddApprovedPersonView()
    {
        // Arrange
        var model = new AddApprovedPersonViewModel();

        var session = new OrganisationSession
        {
            IsOrganisationAPartnership = true,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                IsInEligibleToBeApprovedPerson = false
            }
        };

        _systemUnderTest.ModelState.AddModelError("InviteUserOption", "Required");

        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.AddApprovedPerson(model);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("LimitedPartnershipAddApprovedPerson");
        viewResult.Model.Should().Be(model);
    }

    [TestMethod]
    public async Task TeamMemberDetails_IdIsEmpty_RedirectsToTeamMemberRoleInOrganisation()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act
        var result = await _systemUnderTest.TeamMemberDetails(emptyId);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = (RedirectToActionResult)result;
        redirect.ActionName.Should().Be(nameof(ApprovedPersonController.TeamMemberRoleInOrganisation));
    }

    [TestMethod]
    public async Task AddApprovedPerson_WhenUserIsChangingDetails_SetsBackLinkToCheckYourDetails()
    {
        // Arrange
        var session = new OrganisationSession
        {
            IsUserChangingDetails = true,
            Journey = _orgSessionMock.Journey
        };

        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.AddApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

}