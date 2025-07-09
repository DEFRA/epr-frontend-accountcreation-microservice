using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.Extensions;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.LimitedPartnership
{
    [TestClass]
    public class NonCompaniesHousePartnershipRoleTests : LimitedPartnershipTestBase
    {
        private const string PreviousPage = "previousPath";
        private OrganisationSession _orgSessionMock = null!;

        [TestInitialize]
        public void Setup()
        {
            SetupBase();

            _orgSessionMock = new OrganisationSession
            {
                Journey = new List<string>
            {
                PreviousPage,
                PagePath.NonCompaniesHousePartnershipYourRole
            },
                ReExManualInputSession = new()
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(_orgSessionMock);
        }

        [TestMethod]
        public async Task NonCompaniesHousePartnershipRole_Get_ReturnsView()
        {
            // Act
            var result = await _systemUnderTest.NonCompaniesHousePartnershipRole();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;
            viewResult.Model.Should().BeOfType<NonCompaniesHousePartnershipRoleModel>();
            viewResult.ViewData["BackLinkToDisplay"].Should().Be(PreviousPage);
        }

        [TestMethod]
        public async Task NonCompaniesHousePartnershipRole_Get_WhenRoleExistsInSession_ReturnsViewWithRole()
        {
            // Arrange
            _orgSessionMock.ReExManualInputSession.RoleInOrganisation = RoleInOrganisation.Director;

            // Act
            var result = await _systemUnderTest.NonCompaniesHousePartnershipRole();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;
            var model = viewResult.Model as NonCompaniesHousePartnershipRoleModel;
            model.Should().NotBeNull();
            model!.RoleInOrganisation.Should().Be(RoleInOrganisation.Director);
            viewResult.ViewData["BackLinkToDisplay"].Should().Be(PreviousPage);
        }

        [TestMethod]
        public async Task NonCompaniesHousePartnershipRole_Post_WithInvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var model = new NonCompaniesHousePartnershipRoleModel();
            _systemUnderTest.ModelState.AddModelError("NonCompaniesHousePartnershipRole", "Required");

            // Act
            var result = await _systemUnderTest.NonCompaniesHousePartnershipRole(model);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;
            viewResult.Model.Should().Be(model);
            viewResult.ViewData["BackLinkToDisplay"].Should().Be(PreviousPage);
        }

        [TestMethod]
        [DataRow(RoleInOrganisation.Partner,true)]
        [DataRow(RoleInOrganisation.PartnerCompanySecretary, true)]
        [DataRow(RoleInOrganisation.PartnerDirector, true)]
        public async Task NonCompaniesHousePartnershipRole_Post_RedirectsToNonCompaniesHousePartnershipAddApprovedPerson(RoleInOrganisation role, bool isEligibleToApprovedPerson)
        {
            // Arrange
            var model = new NonCompaniesHousePartnershipRoleModel
            {
                RoleInOrganisation = role,
            };

            // Act
            var result = await _systemUnderTest.NonCompaniesHousePartnershipRole(model);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = (RedirectToActionResult)result;
            redirectResult.ActionName.Should().Be(nameof(ApprovedPersonController.NonCompaniesHousePartnershipAddApprovedPerson));
            redirectResult.ControllerName.Should().Be(nameof(ApprovedPersonController).WithoutControllerSuffix());

            _sessionManagerMock.Verify(x => x.SaveSessionAsync(
                It.IsAny<ISession>(),
                It.Is<OrganisationSession>(s =>
                    s.ReExManualInputSession.RoleInOrganisation == role
                    && s.ReExManualInputSession.IsEligibleToBeApprovedPerson == isEligibleToApprovedPerson
                    && s.Journey.Contains(PagePath.NonCompaniesHousePartnershipYourRole)
                    && s.Journey.Contains(PagePath.NonCompaniesHousePartnershipAddApprovedPerson)
                )),
                Times.Once);
            _orgSessionMock.Journey.Should().HaveElementAt(1, PagePath.NonCompaniesHousePartnershipYourRole);
            _orgSessionMock.Journey.Should().HaveElementAt(2, PagePath.NonCompaniesHousePartnershipAddApprovedPerson);
        }

        [TestMethod]
        public async Task NonCompaniesHousePartnershipRole_Post_NoneOfTheAbove_SetsIsEligibleToBeApprovedPersonFalse_AndRedirects()
        {
            // Arrange
            var role = RoleInOrganisation.NoneOfTheAbove;
            var model = new NonCompaniesHousePartnershipRoleModel
            {
                RoleInOrganisation = role
            };

            // Act
            var result = await _systemUnderTest.NonCompaniesHousePartnershipRole(model);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = (RedirectToActionResult)result;

            redirectResult.ActionName.Should().Be(nameof(LimitedPartnershipController.NonCompaniesHousePartnershipInviteApprovedPerson));

            _sessionManagerMock.Verify(x => x.SaveSessionAsync(
        It.IsAny<ISession>(),
        It.Is<OrganisationSession>(s =>
            s.ReExManualInputSession.RoleInOrganisation == role &&
            s.ReExManualInputSession.IsEligibleToBeApprovedPerson == false &&
            s.Journey.Contains(PagePath.NonCompaniesHousePartnershipInviteApprovedPerson)
        )),
        Times.Once);

            _orgSessionMock.Journey.Should().HaveElementAt(2, PagePath.NonCompaniesHousePartnershipInviteApprovedPerson);
        }
    }
}
