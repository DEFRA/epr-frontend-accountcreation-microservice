using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation
{
    [TestClass]
    public class TeamMembersDetailsTests : OrganisationTestBase
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
                    "PageBefore",
                    PagePath.TeamMemberRoleInOrganisation
                ],
                CompaniesHouseSession = new ReExCompaniesHouseSession
                {
                    CurrentTeamMemberIndex = 0,
                    TeamMembers =
                    [
                        new ReExCompanyTeamMember
                        {
                            FullName = "John Smith",
                            TelephoneNumber = "0123456789",
                            Email = "john@example.com"
                        }
                    ]
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_orgSessionMock);
        }

        [TestMethod]
        public async Task TeamMembersDetails_Get_WithExistingTeamMember_ReturnsPopulatedView()
        {
            // Act
            var result = await _systemUnderTest.TeamMembersDetails();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;

            var model = viewResult.Model.Should().BeOfType<TeamMemberViewModel>().Subject;
            model.FullName.Should().Be("John Smith");
            model.Telephone.Should().Be("0123456789");
            model.Email.Should().Be("john@example.com");

            AssertBackLink(viewResult, PagePath.TeamMemberRoleInOrganisation);
        }

        [TestMethod]
        public async Task TeamMembersDetails_Get_WithNoTeamMember_ReturnsEmptyView()
        {
            // Arrange
            _orgSessionMock.CompaniesHouseSession = new ReExCompaniesHouseSession(); // No members

            // Act
            var result = await _systemUnderTest.TeamMembersDetails();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;
            viewResult.Model.Should().BeNull();

            AssertBackLink(viewResult, PagePath.TeamMemberRoleInOrganisation);
        }

        [TestMethod]
        public async Task TeamMembersDetails_Post_WithValidModel_UpdatesMemberAndRedirects()
        {
            // Arrange
            var model = new TeamMemberViewModel
            {
                FullName = "Jane Doe",
                Telephone = "0987654321",
                Email = "jane@example.com"
            };

            // Act
            var result = await _systemUnderTest.TeamMembersDetails(model);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be("CheckInvitationDetails");

            var updatedMember = _orgSessionMock.CompaniesHouseSession.TeamMembers[0];
            updatedMember.FullName.Should().Be("Jane Doe");
            updatedMember.TelephoneNumber.Should().Be("0987654321");
            updatedMember.Email.Should().Be("jane@example.com");

            _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), _orgSessionMock), Times.Once);
        }

        [TestMethod]
        public async Task TeamMembersDetails_Post_WithInvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var model = new TeamMemberViewModel
            {
                FullName = "Incomplete User" // Missing Email
            };

            _systemUnderTest.ModelState.AddModelError("Email", "Required");

            // Act
            var result = await _systemUnderTest.TeamMembersDetails(model);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;

            viewResult.Model.Should().BeEquivalentTo(model);
            AssertBackLink(viewResult, PagePath.TeamMemberRoleInOrganisation);
        }
    }
}