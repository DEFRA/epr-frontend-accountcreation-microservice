using FluentAssertions;
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
    public async Task LimitedLiabilityPartnership_Post_WithValidModel_SavesAndRedirects()
    {
        // Arrange
        var model = new LimitedLiabilityPartnershipViewModel
        {
            IsMemberOfLimitedLiabilityPartnership = false
        };

        // Act
        var result = await _systemUnderTest.LimitedLiabilityPartnership(model);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectResult.ActionName.Should().Be(nameof(ApprovedPersonController.AddApprovedPerson));
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedLiabilityPartnership.IsMemberOfLimitedLiabilityPartnership.Should().BeFalse();
        _orgSessionMock.ReExCompaniesHouseSession.IsInEligibleToBeApprovedPerson.Should().BeTrue();
    }
}
