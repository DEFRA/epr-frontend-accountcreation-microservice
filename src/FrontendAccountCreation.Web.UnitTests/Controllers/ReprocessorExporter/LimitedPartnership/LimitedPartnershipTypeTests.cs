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
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.LimitedPartnership;

[TestClass]
public class LimitedPartnershipTypeTests : LimitedPartnershipTestBase
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
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                IsPartnership = true,
                Partnership = new ReExPartnership
                {
                    IsLimitedPartnership = true,
                    LimitedPartnership = new ReExLimitedPartnership()
                }
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(_orgSessionMock);
    }

    [TestMethod]
    public async Task PartnershipType_Get_ReturnsView()
    {
        // Act
        var result = await _systemUnderTest.PartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<PartnershipTypeRequestViewModel>();
    }

    [TestMethod]
    public async Task PartnershipType_Get_WithExistingPartnership_ReturnsViewWithCorrectType()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession.IsPartnership = true;

        // Act
        var result = await _systemUnderTest.PartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as PartnershipTypeRequestViewModel;
        model.Should().NotBeNull();
        model!.isLimitedPartnership.Should().Be(Core.Sessions.PartnershipType.LimitedPartnership);
    }

    [TestMethod]
    public async Task PartnershipType_Get_WithExistingNonPartnership_ReturnsViewWithLimitedLiability()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession.IsPartnership = false;

        // Act
        var result = await _systemUnderTest.PartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as PartnershipTypeRequestViewModel;
        model.Should().NotBeNull();
        model!.isLimitedPartnership.Should().Be(Core.Sessions.PartnershipType.LimitedLiabilityPartnership);
    }

    [TestMethod]
    public async Task PartnershipType_Get_SetsBackLink_WhenNotChangingDetails()
    {
        // Arrange
        _orgSessionMock.Journey = new List<string>
        {
            PagePath.TypeOfOrganisation,
            PagePath.PartnershipType
        };
        _orgSessionMock.IsUserChangingDetails = false;

        // Act
        var result = await _systemUnderTest.PartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.ViewData["BackLinkToDisplay"].Should().Be(PagePath.TypeOfOrganisation);
    }

    [TestMethod]
    public async Task PartnershipType_Get_WhenUserChangingDetails_SetsCheckYourDetailsBackLink()
    {
        // Arrange
        _orgSessionMock.IsUserChangingDetails = true;

        // Act
        var result = await _systemUnderTest.PartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.ViewData["BackLinkToDisplay"].Should().Be(PagePath.CheckYourDetails);
    }

    [TestMethod]
    public async Task PartnershipType_Post_WithInvalidModel_ReturnsViewWithModel()
    {
        // Arrange
        var model = new PartnershipTypeRequestViewModel();
        _systemUnderTest.ModelState.AddModelError("isLimitedPartnership", "Required");

        // Act
        var result = await _systemUnderTest.PartnershipType(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().Be(model);
    }

    [TestMethod]
    public async Task PartnershipType_Post_WithValidModel_UpdatesSessionAndRedirects()
    {
        // Arrange
        var model = new PartnershipTypeRequestViewModel
        {
            isLimitedPartnership = Core.Sessions.PartnershipType.LimitedPartnership
        };

        // Act
        var result = await _systemUnderTest.PartnershipType(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(_systemUnderTest.LimitedPartnershipType));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExCompaniesHouseSession.IsPartnership.HasValue &&
                s.ReExCompaniesHouseSession.Partnership.IsLimitedPartnership &&
                s.ReExCompaniesHouseSession.Partnership.LimitedPartnership != null
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task PartnershipType_Post_WithLimitedLiabilityPartnership_UpdatesSessionAndRedirects()
    {
        // Arrange
        var model = new PartnershipTypeRequestViewModel
        {
            isLimitedPartnership = Core.Sessions.PartnershipType.LimitedLiabilityPartnership
        };

        // Act
        var result = await _systemUnderTest.PartnershipType(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(_systemUnderTest.LimitedPartnershipType));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExCompaniesHouseSession.IsPartnership == false
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task PartnershipType_Post_WithLimitedPartnershipAndNullPartnership_CreatesNewPartnership()
    {
        // Arrange
        var model = new PartnershipTypeRequestViewModel
        {
            isLimitedPartnership = Core.Sessions.PartnershipType.LimitedPartnership
        };

        var session = new OrganisationSession
        {
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                Partnership = null
            }
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.PartnershipType(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(_systemUnderTest.LimitedPartnershipType));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExCompaniesHouseSession.IsPartnership == true &&
                s.ReExCompaniesHouseSession.Partnership != null &&
                s.ReExCompaniesHouseSession.Partnership.IsLimitedPartnership == true
            )),
            Times.Once);
    }


    [TestMethod]
    public async Task LimitedPartnershipType_Get_ReturnsView()
    {
        // Act
        var result = await _systemUnderTest.LimitedPartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<LimitedPartnershipTypeRequestViewModel>();
    }

    [TestMethod]
    public async Task LimitedPartnershipType_Get_SetsBackLink()
    {
        // Act
        var result = await _systemUnderTest.LimitedPartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.ViewData["BackLinkToDisplay"].Should().Be(PagePath.PartnershipType);
    }

    [TestMethod]
    public async Task LimitedPartnershipType_Get_WithNullLimitedPartnership_ReturnsViewWithEmptyModel()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession.Partnership = new ReExPartnership
        {
            IsLimitedPartnership = true,
            LimitedPartnership = null
        };

        // Act
        var result = await _systemUnderTest.LimitedPartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as LimitedPartnershipTypeRequestViewModel;
        model.Should().NotBeNull();
        model!.hasIndividualPartners.Should().BeFalse();
        model!.hasCompanyPartners.Should().BeFalse();
    }

    [TestMethod]
    public async Task LimitedPartnershipType_Post_WithNullPartnership_CreatesNewSessionAndRedirects()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession.Partnership = null;
        var model = new LimitedPartnershipTypeRequestViewModel
        {
            hasIndividualPartners = true,
            hasCompanyPartners = false
        };

        // Act
        var result = await _systemUnderTest.LimitedPartnershipType(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(_systemUnderTest.NamesOfPartners));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExCompaniesHouseSession.Partnership != null &&
                s.ReExCompaniesHouseSession.Partnership.IsLimitedPartnership &&
                s.ReExCompaniesHouseSession.Partnership.LimitedPartnership != null &&
                s.ReExCompaniesHouseSession.Partnership.LimitedPartnership.HasIndividualPartners == true &&
                s.ReExCompaniesHouseSession.Partnership.LimitedPartnership.HasCompanyPartners == false
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task LimitedPartnershipType_Post_WithInvalidModel_ReturnsViewWithModel()
    {
        // Arrange
        var model = new LimitedPartnershipTypeRequestViewModel();
        _systemUnderTest.ModelState.AddModelError("hasIndividualPartners", "Required");

        // Act
        var result = await _systemUnderTest.LimitedPartnershipType(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().Be(model);
    }

    [TestMethod]
    public async Task LimitedPartnershipType_Post_WithIndividualPartnersOnly_UpdatesSessionAndRedirects()
    {
        // Arrange
        var model = new LimitedPartnershipTypeRequestViewModel
        {
            hasIndividualPartners = true,
            hasCompanyPartners = false
        };

        // Act
        var result = await _systemUnderTest.LimitedPartnershipType(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(_systemUnderTest.NamesOfPartners));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExCompaniesHouseSession.Partnership != null &&
            s.ReExCompaniesHouseSession.Partnership.IsLimitedPartnership &&
                s.ReExCompaniesHouseSession.Partnership.LimitedPartnership != null &&
                s.ReExCompaniesHouseSession.Partnership.LimitedPartnership.HasIndividualPartners == true &&
                s.ReExCompaniesHouseSession.Partnership.LimitedPartnership.HasCompanyPartners == false
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task LimitedPartnershipType_Post_WithCompanyPartnersOnly_UpdatesSessionAndRedirects()
    {
        // Arrange
        var model = new LimitedPartnershipTypeRequestViewModel
        {
            hasIndividualPartners = false,
            hasCompanyPartners = true
        };

        // Act
        var result = await _systemUnderTest.LimitedPartnershipType(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(_systemUnderTest.NamesOfPartners));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExCompaniesHouseSession.Partnership != null &&
            s.ReExCompaniesHouseSession.Partnership.IsLimitedPartnership &&
                s.ReExCompaniesHouseSession.Partnership.LimitedPartnership != null &&
                s.ReExCompaniesHouseSession.Partnership.LimitedPartnership.HasIndividualPartners == false &&
                s.ReExCompaniesHouseSession.Partnership.LimitedPartnership.HasCompanyPartners == true
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task LimitedPartnershipType_Post_WithBothPartnersSelected_UpdatesSessionAndRedirects()
    {
        // Arrange
        var model = new LimitedPartnershipTypeRequestViewModel
        {
            hasIndividualPartners = true,
            hasCompanyPartners = true
        };

        // Act
        var result = await _systemUnderTest.LimitedPartnershipType(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(_systemUnderTest.NamesOfPartners));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExCompaniesHouseSession.Partnership != null &&
            s.ReExCompaniesHouseSession.Partnership.IsLimitedPartnership &&
                s.ReExCompaniesHouseSession.Partnership.LimitedPartnership != null &&
                s.ReExCompaniesHouseSession.Partnership.LimitedPartnership.HasIndividualPartners == true &&
                s.ReExCompaniesHouseSession.Partnership.LimitedPartnership.HasCompanyPartners == true
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task LimitedPartnershipType_Post_WithBothFieldsFalse_ReturnsViewWithModelError()
    {
        // Arrange
        var model = new LimitedPartnershipTypeRequestViewModel
        {
            hasIndividualPartners = false,
            hasCompanyPartners = false
        };

        // Manually trigger validation as done in controller
        var validationContext = new ValidationContext(model, null, null);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(model, validationContext, validationResults, true);

        foreach (var validationResult in validationResults)
        {
            foreach (var memberName in validationResult.MemberNames)
            {
                _systemUnderTest.ModelState.AddModelError(memberName, validationResult.ErrorMessage);
            }
        }

        // Act
        var result = await _systemUnderTest.LimitedPartnershipType(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().Be(model);
        _systemUnderTest.ModelState.IsValid.Should().BeFalse();

        // Check that validation error exists (the actual message might be resolved from resources)
        _systemUnderTest.ModelState.ErrorCount.Should().BeGreaterThan(0);
        _systemUnderTest.ModelState.ContainsKey("hasIndividualPartners").Should().BeTrue();
    }
}