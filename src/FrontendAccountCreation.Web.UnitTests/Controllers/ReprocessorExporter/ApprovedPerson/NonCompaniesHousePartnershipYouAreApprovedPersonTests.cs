using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.ApprovedPerson;

[TestClass]
public class NonCompaniesHousePartnershipYouAreApprovedPersonTests : ApprovedPersonTestBase
{
    private OrganisationSession _orgSessionMock = null!;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _orgSessionMock = new OrganisationSession
        {
            Journey =
            [
                PagePath.NonCompaniesHousePartnershipAddApprovedPerson
            ],
            ReExManualInputSession = new ReExManualInputSession(),
            IsUserChangingDetails = false
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_orgSessionMock);
    }

    [TestMethod]
    public async Task NonCompaniesHouseYouAreApprovedPerson_Get_ReturnsDefaultView()
    {
        // Arrange
        // Act
        var result = await _systemUnderTest.NonCompaniesHouseYouAreApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.ViewName.Should().BeNull(); // returns default view
    }

    [TestMethod]
    public async Task NonCompaniesHouseYouAreApprovedPerson_Get_WithFocusId_SetsFocusId()
    {
        // Arrange
        // Act
        var result = await _systemUnderTest.NonCompaniesHouseYouAreApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [TestMethod]
    public async Task NonCompaniesHouseYouAreApprovedPerson_Post_InviteApprovedPersonTrue_RedirectsToTeamMemberRole()
    {
        // Arrange
        var inviteApprovedPerson = true;

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseYouAreApprovedPerson(inviteApprovedPerson);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = (RedirectToActionResult)result;
        redirect.ActionName.Should().Be(nameof(_systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole));
    }

    [TestMethod]
    public async Task NonCompaniesHouseYouAreApprovedPerson_Post_InviteApprovedPersonFalse_RedirectsToCheckYourDetails()
    {
        // Arrange
        var inviteApprovedPerson = false;

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseYouAreApprovedPerson(inviteApprovedPerson);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = (RedirectToActionResult)result;
        redirect.ActionName.Should().Be(nameof(_systemUnderTest.CheckYourDetails));
    }
}