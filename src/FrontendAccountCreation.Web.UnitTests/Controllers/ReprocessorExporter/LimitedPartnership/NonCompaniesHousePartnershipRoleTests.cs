using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.AspNetCore.Http;
using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;

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
                PagePath.NonCompaniesHousePartnershipRole
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
        [DataRow(RoleInOrganisation.Partner, true, nameof(ApprovedPersonController.NonCompaniesHousePartnershipAddApprovedPerson))]
        [DataRow(RoleInOrganisation.PartnerCompanySecretary, true, nameof(ApprovedPersonController.NonCompaniesHousePartnershipAddApprovedPerson))]
        [DataRow(RoleInOrganisation.PartnerDirector, true, nameof(ApprovedPersonController.NonCompaniesHousePartnershipAddApprovedPerson))]
        [DataRow(RoleInOrganisation.NoneOfTheAbove, false, nameof(LimitedPartnershipController.NonCompaniesHousePartnershipInviteApprovedPerson))]
        public async Task NonCompaniesHousePartnershipRole_Post_RedirectsToCorrectPageBasedOnRole(
            RoleInOrganisation role,
            bool isEligibleToBeApprovedPerson,
            string expectedActionName)
        {
            // Arrange
            var model = new NonCompaniesHousePartnershipRoleModel
            {
                RoleInOrganisation = role
            };

            // Act
            var result = await _systemUnderTest.NonCompaniesHousePartnershipRole(model);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirect = (RedirectToActionResult)result;
            redirect.ActionName.Should().Be(expectedActionName);

            _orgSessionMock.ReExManualInputSession.RoleInOrganisation.Should().Be(role);
            _orgSessionMock.ReExManualInputSession.IsEligibleToBeApprovedPerson.Should().Be(isEligibleToBeApprovedPerson);
        }

        [TestMethod]
        public async Task NonCompaniesHousePartnershipRole_Post_InvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var model = new NonCompaniesHousePartnershipRoleModel();
            _systemUnderTest.ModelState.AddModelError("RoleInOrganisation", "Required");

            // Act
            var result = await _systemUnderTest.NonCompaniesHousePartnershipRole(model);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;
            viewResult.Model.Should().Be(model);
        }
    }
}
