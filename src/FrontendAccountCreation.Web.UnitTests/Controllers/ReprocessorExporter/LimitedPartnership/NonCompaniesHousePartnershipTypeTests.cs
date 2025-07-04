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
public class NonCompaniesHousePartnershipTypeTests : LimitedPartnershipTestBase
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
                PagePath.BusinessAddress,
                PagePath.NonCompaniesHousePartnershipType,
            },
            ReExManualInputSession = new ReExManualInputSession
            {
                ProducerType = Core.Sessions.ProducerType.Partnership
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(_orgSessionMock);
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipType_Get_SetsBackLink()
    {
        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.ViewData["BackLinkToDisplay"].Should().Be(PagePath.BusinessAddress);
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipType_Get_WhenNonCompaniesHouseSessionIsNull_ReturnsViewWithEmptyModel()
    {
        // Arrange
        _orgSessionMock.ReExManualInputSession = null;
        
        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as WhatSortOfPartnerRequestViewModel;
        model.Should().NotBeNull();
        model.HasIndividualPartners.Should().BeFalse();
        model.HasCompanyPartners.Should().BeFalse();
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipType_Get_WhenTypesOfPartnerSessionIsNull_ReturnsViewWithEmptyModel()
    {
        // Arrange
        _orgSessionMock.ReExManualInputSession.TypesOfPartner = null;

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as WhatSortOfPartnerRequestViewModel;
        model.Should().NotBeNull();
        model.HasIndividualPartners.Should().BeFalse();
        model.HasCompanyPartners.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public async Task NonCompaniesHousePartnershipType_Get_WithValidTypesOfPartner_ReturnsViewWithPopulatedModel(bool hasPersons, bool hasCompanys)
    {
        // Arrange
        _orgSessionMock.ReExManualInputSession.TypesOfPartner =
            new ReExTypesOfPartner
            {
                HasIndividualPartners = hasPersons,
                HasCompanyPartners = hasCompanys
            };

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as WhatSortOfPartnerRequestViewModel;
        model.Should().NotBeNull();
        model.HasIndividualPartners.Should().Be(hasPersons);
        model.HasCompanyPartners.Should().Be(hasCompanys);
    }

    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public async Task NonCompaniesHousePartnershipType_Post_WithNullTypesOfPartnerSession_And_ValidModel_UpdatesSessionAndRedirect(bool hasPersons, bool hasCompanys)
    {
        // Arrange
        var model = new WhatSortOfPartnerRequestViewModel
        {
            HasIndividualPartners = hasPersons,
            HasCompanyPartners = hasCompanys
        };

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipType(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(_systemUnderTest.NonCompaniesHousePartnershipNamesOfPartners));

        // Verify the session save
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExManualInputSession.TypesOfPartner != null &&
                s.ReExManualInputSession.TypesOfPartner.HasIndividualPartners == hasPersons &&
                s.ReExManualInputSession.TypesOfPartner.HasCompanyPartners == hasCompanys &&
                s.ReExManualInputSession.TypesOfPartner.Partners == null
            )),
            Times.Once);
    }

    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public async Task NonCompaniesHousePartnershipType_Post_WithNotNullTypesOfPartnerSession_And_ValidModel_UpdatesSessionAndRedirect(bool hasPersons, bool hasCompanys)
    {
        // Arrange
        var jill = new ReExPersonOrCompanyPartner
        {
            Id = Guid.NewGuid(),
            Name = "Jill",
            IsPerson = true,
        };

        var biffa = new ReExPersonOrCompanyPartner
        {
            Id = Guid.NewGuid(),
            Name = "Biffa Waste Inc",
            IsPerson = false,
        };

        List<ReExPersonOrCompanyPartner> partners = [jill, biffa];

        ReExTypesOfPartner typesOfPartner = new ReExTypesOfPartner
        {
            HasIndividualPartners = !hasPersons,
            HasCompanyPartners = !hasCompanys,
            Partners = partners
        };

        _orgSessionMock.ReExManualInputSession.TypesOfPartner = typesOfPartner;

        var model = new WhatSortOfPartnerRequestViewModel
        {
            HasIndividualPartners = hasPersons,
            HasCompanyPartners = hasCompanys,
        };

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipType(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(_systemUnderTest.NonCompaniesHousePartnershipNamesOfPartners));

        // Verify the session save
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), _orgSessionMock), Times.Once);

        _orgSessionMock.ReExManualInputSession.TypesOfPartner.HasIndividualPartners = hasPersons;
        _orgSessionMock.ReExManualInputSession.TypesOfPartner.HasCompanyPartners = hasCompanys;
        _orgSessionMock.ReExManualInputSession.TypesOfPartner.Partners?.Count.Should().Be(2);
        _orgSessionMock.ReExManualInputSession.TypesOfPartner.Partners[0].Should().BeEquivalentTo(jill);
        _orgSessionMock.ReExManualInputSession.TypesOfPartner.Partners[1].Should().BeEquivalentTo(biffa);
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipType_Post_WithInvalidModel_ReturnsViewWithModel()
    {
        // Arrange
        var model = new WhatSortOfPartnerRequestViewModel();
        _systemUnderTest.ModelState.AddModelError("HasIndividualPartners", "Required");

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipType(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().Be(model);
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipType_Post_WithBothFieldsFalse_ReturnsViewWithModelError()
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
        var result = await _systemUnderTest.NonCompaniesHousePartnershipType(model);

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