using FluentAssertions;
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
    public async Task LimitedPartnershipType_Get_ReturnsView()
    {
        // Act
        var result = await _systemUnderTest.LimitedPartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<WhatSortOfPartnerRequestViewModel>();
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
    public async Task LimitedPartnershipType_Get_WithNullCompaniesHouseSession_ReturnsViewWithEmptyModel()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession = null;

        // Act
        var result = await _systemUnderTest.LimitedPartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as WhatSortOfPartnerRequestViewModel;
        model.Should().NotBeNull();
        model!.HasIndividualPartners.Should().BeFalse();
        model!.HasCompanyPartners.Should().BeFalse();
    }

    [TestMethod]
    public async Task LimitedPartnershipType_Get_WithNullPartnershipSession_ReturnsViewWithEmptyModel()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession.Partnership = null;

        // Act
        var result = await _systemUnderTest.LimitedPartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as WhatSortOfPartnerRequestViewModel;
        model.Should().NotBeNull();
        model!.HasIndividualPartners.Should().BeFalse();
        model!.HasCompanyPartners.Should().BeFalse();
    }

    [TestMethod]
    public async Task LimitedPartnershipType_Get_WithNullLimitedPartnershipSession_ReturnsViewWithEmptyModel()
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
        var model = viewResult.Model as WhatSortOfPartnerRequestViewModel;
        model.Should().NotBeNull();
        model!.HasIndividualPartners.Should().BeFalse();
        model!.HasCompanyPartners.Should().BeFalse();
    }

    [TestMethod]
    public async Task LimitedPartnershipType_Get_WithValidLimitedPartnership_ReturnsViewWithPopulatedModel()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession.Partnership = new ReExPartnership
        {
            IsLimitedPartnership = true,
            LimitedPartnership = new ReExTypesOfPartner
            {
                HasIndividualPartners = true,
                HasCompanyPartners = false
            }
        };

        // Act
        var result = await _systemUnderTest.LimitedPartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as WhatSortOfPartnerRequestViewModel;
        model.Should().NotBeNull();
        model!.HasIndividualPartners.Should().BeTrue();
        model!.HasCompanyPartners.Should().BeFalse();
    }

    [TestMethod]
    public async Task LimitedPartnershipType_Post_WithLimitedPartnershipNull_CreatesNewLimitedPartnership()
    {
        // Arrange
        var model = new WhatSortOfPartnerRequestViewModel
        {
            HasIndividualPartners = true,
            HasCompanyPartners = true
        };

        var session = new OrganisationSession
        {
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                Partnership = new ReExPartnership
                {
                    IsLimitedPartnership = true,
                    LimitedPartnership = null
                }
            }
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.LimitedPartnershipType(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(_systemUnderTest.NamesOfPartners));

        // Verify the session save
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExCompaniesHouseSession.Partnership != null &&
                s.ReExCompaniesHouseSession.Partnership.LimitedPartnership != null &&
                s.ReExCompaniesHouseSession.Partnership.LimitedPartnership.HasIndividualPartners == model.HasIndividualPartners &&
                s.ReExCompaniesHouseSession.Partnership.LimitedPartnership.HasCompanyPartners == model.HasCompanyPartners
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task LimitedPartnershipType_Post_WithNullPartnership_CreatesNewSessionAndRedirects()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession.Partnership = null;
        var model = new WhatSortOfPartnerRequestViewModel
        {
            HasIndividualPartners = true,
            HasCompanyPartners = false
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
        var model = new WhatSortOfPartnerRequestViewModel();
        _systemUnderTest.ModelState.AddModelError("HasIndividualPartners", "Required");

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
        var model = new WhatSortOfPartnerRequestViewModel
        {
            HasIndividualPartners = true,
            HasCompanyPartners = false
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
        var model = new WhatSortOfPartnerRequestViewModel
        {
            HasIndividualPartners = false,
            HasCompanyPartners = true
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
        var model = new WhatSortOfPartnerRequestViewModel
        {
            HasIndividualPartners = true,
            HasCompanyPartners = true
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
        var model = new WhatSortOfPartnerRequestViewModel
        {
            HasIndividualPartners = false,
            HasCompanyPartners = false
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
        _systemUnderTest.ModelState.ContainsKey("HasIndividualPartners").Should().BeTrue();
    }
}