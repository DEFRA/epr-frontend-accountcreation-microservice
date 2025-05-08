using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.ApprovedPerson;

[TestClass]
public class AddNotApprovedPersonTests : ApprovedPersonTestBase
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

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(_orgSessionMock);
    }

    [TestMethod]
    public async Task AddNotApprovedPerson_Get_ReturnsView()
    {
        // Act
        var result = await _systemUnderTest.AddNotApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [TestMethod]
    public async Task AddNotApprovedPerson_ModelStateInvalid_ReturnsViewWithModel()
    {
        // Arrange
        var model = new AddNotApprovedPersonViewModel();
        _systemUnderTest.ModelState.AddModelError("InviteUserOption", "Required");

        // Act
        var result = await _systemUnderTest.AddNotApprovedPerson(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType(model.GetType());
    }

    [TestMethod]
    public async Task AddNotApprovedPerson_InviteAnotherPerson_RedirectsToTeamMemberRoleInOrganisation()
    {
        // Arrange
        var model = new AddNotApprovedPersonViewModel
        {
            InviteUserOption = InviteUserOptions.InviteAnotherPerson.ToString()
        };

        // Act
        var result = await _systemUnderTest.AddNotApprovedPerson(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = (RedirectToActionResult)result;
        redirect.ActionName.Should().Be(nameof(_systemUnderTest.TeamMemberRoleInOrganisation));
    }

    [TestMethod]
    public async Task AddNotApprovedPerson_InviteApprovedPersonLater_RedirectsToCheckYourDetails()
    {
        // Arrange
        var model = new AddNotApprovedPersonViewModel
        {
            InviteUserOption = InviteUserOptions.InviteLater.ToString()
        };

        // Act
        var result = await _systemUnderTest.AddNotApprovedPerson(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = (RedirectToActionResult)result;
        redirect.ActionName.Should().Be("CheckYourDetails");
        redirect.ControllerName.Should().Be("AccountCreation");
    }
}