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
public class LimitedLiabilityPartnershipTests : LimitedPartnershipTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _orgSessionMock = new OrganisationSession
        {
            Journey =
            [
                PagePath.IsPartnership,
                PagePath.PartnershipType,
                PagePath.LimitedPartnershipType,
                PagePath.LimitedPartnershipNamesOfPartners,
                PagePath.LimitedPartnershipCheckNamesOfPartners
            ],
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
    public async Task LimitedLiabilityPartnership_Get_WhenCompaniesHouseSessionIsNull_Returns_View()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession = null;

        // Act
        var result = await _systemUnderTest.LimitedLiabilityPartnership();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        var model = viewResult.Model.Should().BeOfType<LimitedLiabilityPartnershipViewModel>().Which;
        model.IsMemberOfLimitedLiabilityPartnership.Should().BeNull();
    }

    [TestMethod]
    public async Task LimitedLiabilityPartnership_Get_WhenPartnershipSessionIsNull_Returns_View()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession.Partnership = null;

        // Act
        var result = await _systemUnderTest.LimitedLiabilityPartnership();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        var model = viewResult.Model.Should().BeOfType<LimitedLiabilityPartnershipViewModel>().Which;
        model.IsMemberOfLimitedLiabilityPartnership.Should().BeNull();
    }

    [TestMethod]
    public async Task LimitedLiabilityPartnership_Get_WhenLimiedLiabilityPartnershipSessionIsNull_Returns_View()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedLiabilityPartnership = null;

        // Act
        var result = await _systemUnderTest.LimitedLiabilityPartnership();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        var model = viewResult.Model.Should().BeOfType<LimitedLiabilityPartnershipViewModel>().Which;
        model.IsMemberOfLimitedLiabilityPartnership.Should().BeNull();
    }

    [TestMethod]
    public async Task LimitedLiabilityPartnership_Get_Returns_View_With_ModelValue_From_Session()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession.Partnership = new ReExPartnership
        {
            LimitedLiabilityPartnership = new ReExLimitedLiabilityPartnership
            {
                IsMemberOfLimitedLiabilityPartnership = true
            }
        };

        // Act
        var result = await _systemUnderTest.LimitedLiabilityPartnership();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        var model = viewResult.Model.Should().BeOfType<LimitedLiabilityPartnershipViewModel>().Which;
        model.IsMemberOfLimitedLiabilityPartnership.Should().BeTrue();
    }

    [TestMethod]
    public async Task LimitedLiabilityPartnership_Post_WithInvalidModel_ReturnsViewWithModel()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError("IsMemberOfLimitedLiabilityPartnership", "Required");
        var model = new LimitedLiabilityPartnershipViewModel();

        // Act
        var result = await _systemUnderTest.LimitedLiabilityPartnership(model);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        viewResult.Model.Should().Be(model);
    }

    [TestMethod]
    [DataRow(false, null)]
    [DataRow(true, RoleInOrganisation.Member)]
    public async Task LimitedLiabilityPartnership_Post_WithValidModel_SavesAndRedirects(bool isMember, RoleInOrganisation? expectedRole)
    {
        // Arrange
        var model = new LimitedLiabilityPartnershipViewModel
        {
            IsMemberOfLimitedLiabilityPartnership = isMember
        };

        // Act
        var result = await _systemUnderTest.LimitedLiabilityPartnership(model);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectResult.ActionName.Should().Be(nameof(ApprovedPersonController.AddApprovedPerson));
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedLiabilityPartnership.IsMemberOfLimitedLiabilityPartnership.Should().Be(isMember);
        _orgSessionMock.ReExCompaniesHouseSession.IsInEligibleToBeApprovedPerson.Should().NotBe(isMember);
        _orgSessionMock.ReExCompaniesHouseSession.RoleInOrganisation.Should().Be(expectedRole);
    }

    [TestMethod]
    public async Task LimitedLiabilityPartnership_Post_WithPartnershipSessionNull_AndValidModel_SavesAndRedirects()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession.Partnership = null;

        var model = new LimitedLiabilityPartnershipViewModel
        {
            IsMemberOfLimitedLiabilityPartnership = true
        };

        // Act
        var result = await _systemUnderTest.LimitedLiabilityPartnership(model);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectResult.ActionName.Should().Be(nameof(ApprovedPersonController.AddApprovedPerson));
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedLiabilityPartnership.IsMemberOfLimitedLiabilityPartnership.Should().BeTrue();
        _orgSessionMock.ReExCompaniesHouseSession.IsInEligibleToBeApprovedPerson.Should().BeFalse();
        _orgSessionMock.ReExCompaniesHouseSession.RoleInOrganisation.Should().Be(RoleInOrganisation.Member);
    }

    [TestMethod]
    public async Task LimitedLiabilityPartnership_Post_WithLimitedLiabilityPartnershipSessionNull_AndValidModel_SavesAndRedirects()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedLiabilityPartnership = null;

        var model = new LimitedLiabilityPartnershipViewModel
        {
            IsMemberOfLimitedLiabilityPartnership = true
        };

        // Act
        var result = await _systemUnderTest.LimitedLiabilityPartnership(model);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectResult.ActionName.Should().Be(nameof(ApprovedPersonController.AddApprovedPerson));
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedLiabilityPartnership.IsMemberOfLimitedLiabilityPartnership.Should().BeTrue();
        _orgSessionMock.ReExCompaniesHouseSession.IsInEligibleToBeApprovedPerson.Should().BeFalse();
        _orgSessionMock.ReExCompaniesHouseSession.RoleInOrganisation.Should().Be(RoleInOrganisation.Member);
    }
}