using FrontendAccountCreation.Core.Sessions.ReEx.Partnership.ApprovedPersons;
using FrontendAccountCreation.Core.Sessions.ReEx.Partnership;
using FrontendAccountCreation.Core.Sessions.ReEx;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.LimitedPartnership
{
    [TestClass]
    public class LimitedPartnershipAddApprovedPersonTests : LimitedPartnershipTestBase
    {
        private Guid _approvedPersonId;

        [TestInitialize]
        public void Setup()
        {
            SetupBase();

            _approvedPersonId = Guid.NewGuid();

            _orgSessionMock = new OrganisationSession
            {
                ReExCompaniesHouseSession = new ReExCompaniesHouseSession
                {
                    Partnership = new ReExPartnership
                    {
                        LimitedPartnership = new ReExLimitedPartnership
                        {
                        }
                    }
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(_orgSessionMock);
        }

        [TestMethod]
        public async Task Get_LimitedPartnershipAddApprovedPerson_ReturnsViewAndSetsBackLink()
        {
            // Arrange
            var session = new OrganisationSession();
            _sessionManagerMock
                .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(session);

            // Act
            var result = await _systemUnderTest.LimitedPartnershipAddApprovedPerson(Guid.NewGuid());

            // Assert
            result.Should().BeOfType<ViewResult>();

            _sessionManagerMock.Verify(s => s.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
        }

        [TestMethod]
        public async Task Post_LimitedPartnershipAddApprovedPerson_ModelStateInvalid_ReturnsViewWithModel()
        {
            // Arrange
            var model = new LimitedPartnershipAddApprovedPersonViewModel();
            _systemUnderTest.ModelState.AddModelError("InviteUserOption", "Required");

            var session = new OrganisationSession();
            _sessionManagerMock
                .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(session);

            // Act
            var result = await _systemUnderTest.LimitedPartnershipAddApprovedPerson(model);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;
            viewResult.Model.Should().Be(model);

            _sessionManagerMock.Verify(s => s.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
        }

        [TestMethod]
        public async Task Post_LimitedPartnershipAddApprovedPerson_OptionBeAnApprovedPerson_RedirectsToYouAreApprovedPerson()
        {
            // Arrange
            var model = new LimitedPartnershipAddApprovedPersonViewModel
            {
                InviteUserOption = "BeAnApprovedPerson"
            };

            _sessionManagerMock
                .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(new OrganisationSession());

            // Act
            var result = await _systemUnderTest.LimitedPartnershipAddApprovedPerson(model);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>()
                  .Which.ActionName.Should().Be("YouAreApprovedPerson");
        }

        [TestMethod]
        public async Task Post_LimitedPartnershipAddApprovedPerson_OptionInviteAnotherPerson_RedirectsToTeamMemberRoleInOrganisation()
        {
            // Arrange
            var model = new LimitedPartnershipAddApprovedPersonViewModel
            {
                InviteUserOption = "InviteAnotherPerson"
            };

            _sessionManagerMock
                .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(new OrganisationSession());

            // Act
            var result = await _systemUnderTest.LimitedPartnershipAddApprovedPerson(model);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>()
                  .Which.ActionName.Should().Be("TeamMemberRoleInOrganisation");
        }

        [TestMethod]
        public async Task Post_LimitedPartnershipAddApprovedPerson_OptionInviteLater_RedirectsToCheckYourDetails()
        {
            // Arrange
            var model = new LimitedPartnershipAddApprovedPersonViewModel
            {
                InviteUserOption = "InviteLater"
            };

            _sessionManagerMock
                .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(new OrganisationSession());

            // Act
            var result = await _systemUnderTest.LimitedPartnershipAddApprovedPerson(model);

            // Assert
            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("CheckYourDetails");
            redirect.ControllerName.Should().Be("AccountCreation");
        }


    }
}
