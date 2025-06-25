using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Core.Sessions.ReEx.Partnership;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.LimitedPartnership;

[TestClass]
public class LimitedPartnershipRoleTests : LimitedPartnershipTestBase
{
    private OrganisationSession _orgSessionMock = null!;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _orgSessionMock = new OrganisationSession
        {
            Journey = new List<string>
            {
                PagePath.IsPartnership,
                PagePath.PartnershipType,
                PagePath.LimitedPartnershipType,
                PagePath.LimitedPartnershipNamesOfPartners,
                PagePath.LimitedPartnershipRole,
                PagePath.AddAnApprovedPerson
            },
            IsOrganisationAPartnership = true,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                Partnership = new ReExPartnership
                {
                    IsLimitedPartnership = true,
                    LimitedPartnership = new ReExTypesOfPartner()
                }
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(_orgSessionMock);
    }

    [TestMethod]
    public async Task LimitedPartnershipRole_Get_ReturnsView()
    {
        // Act
        var result = await _systemUnderTest.LimitedPartnershipRole();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<LimitedPartnershipRoleViewModel>();
        viewResult.ViewData["BackLinkToDisplay"].Should().Be(PagePath.LimitedPartnershipNamesOfPartners);
    }

    [TestMethod]
    public async Task LimitedPartnershipRole_Get_WhenUserChangingDetails_SetsCheckYourDetailsBackLink()
    {
        // Arrange
        _orgSessionMock.IsUserChangingDetails = true;

        // Act
        var result = await _systemUnderTest.LimitedPartnershipRole();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.ViewData["BackLinkToDisplay"].Should().Be(PagePath.CheckYourDetails);
    }

    [TestMethod]
    public async Task LimitedPartnershipRole_Get_WithExistingRole_ReturnsViewWithRole()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession.RoleInOrganisation = RoleInOrganisation.Director;

        // Act
        var result = await _systemUnderTest.LimitedPartnershipRole();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as LimitedPartnershipRoleViewModel;
        model.Should().NotBeNull();
        model!.LimitedPartnershipRole.Should().Be(RoleInOrganisation.Director);
    }

    [TestMethod]
    public async Task LimitedPartnershipRole_Post_WithInvalidModel_ReturnsViewWithModel()
    {
        // Arrange
        var model = new LimitedPartnershipRoleViewModel();
        _systemUnderTest.ModelState.AddModelError("LimitedPartnershipRole", "Required");

        // Act
        var result = await _systemUnderTest.LimitedPartnershipRole(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().Be(model);
        viewResult.ViewData["BackLinkToDisplay"].Should().Be(PagePath.LimitedPartnershipNamesOfPartners);
    }

    [TestMethod]
    public async Task LimitedPartnershipRole_Post_WithValidModel_UpdatesSessionAndRedirects()
    {
        // Arrange
        var model = new LimitedPartnershipRoleViewModel
        {
            LimitedPartnershipRole = RoleInOrganisation.Director
        };

        // Act
        var result = await _systemUnderTest.LimitedPartnershipRole(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExCompaniesHouseSession.Partnership != null &&
                s.ReExCompaniesHouseSession.Partnership.LimitedPartnership != null &&
                s.ReExCompaniesHouseSession.RoleInOrganisation == RoleInOrganisation.Director
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task LimitedPartnershipRole_Post_RedirectsToApprovedPersonController()
    {
        // Arrange
        var model = new LimitedPartnershipRoleViewModel
        {
            LimitedPartnershipRole = RoleInOrganisation.Director
        };

        // Act
        var result = await _systemUnderTest.LimitedPartnershipRole(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(ApprovedPersonController.AddApprovedPerson));
        redirectResult.ControllerName.Should().Be("ApprovedPerson");

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExCompaniesHouseSession.RoleInOrganisation == RoleInOrganisation.Director
            )),
            Times.Once);
    }
}