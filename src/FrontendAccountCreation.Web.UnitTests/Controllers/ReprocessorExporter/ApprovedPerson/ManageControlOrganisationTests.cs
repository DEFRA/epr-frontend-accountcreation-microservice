using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FrontendAccountCreation.Core.Models;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.ApprovedPerson
{
    [TestClass]
    public class ManageControlOrganisationTests : ApprovedPersonTestBase
    {
        private OrganisationSession _orgSessionMock;

        [TestInitialize]
        public void Setup()
        {
            SetupBase();

            _orgSessionMock = new OrganisationSession
            {
                Journey =
                [ 
                    PagePath.ManageControlOrganisation,
                    PagePath.TeamMemberDetails
                ],
                ReExCompaniesHouseSession = new ReExCompaniesHouseSession(),
                IsUserChangingDetails = false
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_orgSessionMock);
        }

        [TestMethod]
        [DataRow(YesNoNotSure.Yes)]
        [DataRow(YesNoNotSure.No)]
        [DataRow(YesNoNotSure.NotSure)]
        [DataRow(null)]
        public async Task Get_ManageControlOrganisation_ReturnsView_WithViewModel(YesNoNotSure ? yesNoNotSure)
        {
            // Arrange
            var session = new OrganisationSession
            {
                TheyManageOrControlOrganisation = yesNoNotSure
            };
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(session);

            // Act
            var result = await _systemUnderTest.ManageControlOrganisation();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<ManageControlOrganisationViewModel>().Subject;
            model.TheyManageOrControlOrganisation.Should().Be(yesNoNotSure);
        }

        [TestMethod]
        [DataRow(YesNoNotSure.Yes)]
        [DataRow(null)]
        public async Task Get_ManageControlOrganisation_ReturnsView_WithViewModel_ParameterValue_AsTrue(YesNoNotSure? yesNoNotSure)
        {
            // Arrange
            var session = new OrganisationSession
            {
                TheyManageOrControlOrganisation = yesNoNotSure
            };
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(session);

            var invitePerson = yesNoNotSure == YesNoNotSure.Yes;

            // Act
            var result = await _systemUnderTest.ManageControlOrganisation(invitePerson);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<ManageControlOrganisationViewModel>().Subject;
            model.TheyManageOrControlOrganisation.Should().Be(null);
        }

        [TestMethod]
        public async Task Post_ManageControlOrganisation_With_InvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var session = new OrganisationSession();
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(session);

            var model = new ManageControlOrganisationViewModel();
            _systemUnderTest.ModelState.AddModelError("TheyManageOrControlOrganisation", "Required");

            // Act
            var result = await _systemUnderTest.ManageControlOrganisation(model);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(model);
        }

        [TestMethod]
        [DataRow(YesNoNotSure.Yes)]
        [DataRow(YesNoNotSure.No)]
        [DataRow(YesNoNotSure.NotSure)]
        public async Task Post_ManageControlOrganisation_ValidModel_UpdatesSessionAndRedirects(YesNoNotSure yesNoNotSure)
        {
            // Arrange
            var session = new OrganisationSession();
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(session);

            var model = new ManageControlOrganisationViewModel
            {
                TheyManageOrControlOrganisation = yesNoNotSure
            };

            // Act
            var result = await _systemUnderTest.ManageControlOrganisation(model);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;

            var actionMethod = yesNoNotSure == YesNoNotSure.Yes ? nameof(ApprovedPersonController.SoleTraderTeamMemberDetails) : nameof(ApprovedPersonController.PersonCanNotBeInvited);
            redirectResult.ActionName.Should().Be(actionMethod);
            session.TheyManageOrControlOrganisation.Should().Be(yesNoNotSure);
        }
    }
}