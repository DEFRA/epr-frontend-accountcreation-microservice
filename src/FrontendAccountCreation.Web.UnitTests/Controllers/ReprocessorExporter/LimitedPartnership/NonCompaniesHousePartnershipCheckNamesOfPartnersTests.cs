using FrontendAccountCreation.Core.Sessions.ReEx.Partnership;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using Microsoft.AspNetCore.Http;
using Moq;
using FluentAssertions;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Mvc;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.LimitedPartnership;

[TestClass]
public class NonCompaniesHousePartnershipCheckNamesOfPartnersTests : LimitedPartnershipTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _orgSessionMock = new OrganisationSession
        {
            Journey = new List<string>
            {
                PagePath.NonCompaniesHousePartnershipType,
                PagePath.NonCompaniesHousePartnershipNamesOfPartners,
                PagePath.NonCompaniesHousePartnershipCheckNamesOfPartners
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
    public async Task NonCompaniesHousePartnershipCheckNamesOfPartners_Get_ReturnsView()
    {
        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipCheckNamesOfPartners();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<List<ReExPersonOrCompanyPartner>>();
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipCheckNamesOfPartners_Get_SetsBackLink()
    {
        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipCheckNamesOfPartners();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.ViewData["BackLinkToDisplay"].Should().Be(PagePath.NonCompaniesHousePartnershipNamesOfPartners);

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), _orgSessionMock), Times.Never);
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipCheckNamesOfPartnersDelete_Get_RemovesPartnerFromSession()
    {
        // Arrange
        var jack = new ReExPersonOrCompanyPartner
        {
            Id = Guid.NewGuid(),
            Name = "Jack",
            IsPerson = true,
        };

        var jill = new ReExPersonOrCompanyPartner
        {
            Id = Guid.NewGuid(),
            Name = "Jill",
            IsPerson = true,
        };

        ReExTypesOfPartner typesOfPartner = new()
        {
            HasIndividualPartners = true,
            Partners = [jack, jill]
        };

        _orgSessionMock.ReExManualInputSession.TypesOfPartner = typesOfPartner;

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipCheckNamesOfPartnersDelete(jack.Id);

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(LimitedPartnershipController.NonCompaniesHousePartnershipCheckNamesOfPartners));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), _orgSessionMock), Times.Once);

        _orgSessionMock.ReExManualInputSession.TypesOfPartner.Partners.Should()
            .NotBeNull().And
            .ContainSingle().Which
            .Should()
            .BeEquivalentTo(jill);
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipNamesOfPartners_Post_Save_RedirectsToCorrectPage()
    {
        // Arrange
        List<ReExPersonOrCompanyPartner> modelNotUsed = new();

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipCheckNamesOfPartners(modelNotUsed);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), _orgSessionMock), Times.Once);

        redirectResult.ActionName.Should().Be(nameof(LimitedPartnershipController.NonCompaniesHousePartnershipRole));
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipCheckNamesOfPartners_Get_WhenTypesOfPartnerIsNull_ReturnsEmptyModel()
    {
        // Arrange
        _orgSessionMock.ReExManualInputSession.TypesOfPartner = null;

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipCheckNamesOfPartners();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as List<ReExPersonOrCompanyPartner>;
        model.Should().NotBeNull();
        model.Should().BeEmpty();
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipCheckNamesOfPartnersDelete_Get_UpdatesSession_And_Redirects()
    {
        // Arrange
        var jack = new ReExPersonOrCompanyPartner
        {
            Id = Guid.NewGuid(),
            Name = "Jack",
            IsPerson = true,
        };

        var jill = new ReExPersonOrCompanyPartner
        {
            Id = Guid.NewGuid(),
            Name = "Jill",
            IsPerson = true,
        };

        ReExTypesOfPartner typesOfPartner = new()
        {
            HasIndividualPartners = true,
            Partners = [jack, jill]
        };

        _orgSessionMock.ReExManualInputSession.TypesOfPartner = typesOfPartner;

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipCheckNamesOfPartnersDelete(jack.Id);

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(LimitedPartnershipController.NonCompaniesHousePartnershipCheckNamesOfPartners));

        _orgSessionMock.ReExManualInputSession.TypesOfPartner.Partners.Should().NotBeNull().And
            .ContainSingle().Which
            .Should()
            .BeEquivalentTo(jill);
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipCheckNamesOfPartnersDelete_Get_WhenGivenUnmatchedId_Redirects()
    {
        // Arrange
        var jack = new ReExPersonOrCompanyPartner
        {
            Id = Guid.NewGuid(),
            Name = "Jack",
            IsPerson = true,
        };

        var jill = new ReExPersonOrCompanyPartner
        {
            Id = Guid.NewGuid(),
            Name = "Jill",
            IsPerson = true,
        };

        ReExTypesOfPartner typesOfPartner = new()
        {
            HasIndividualPartners = true,
            Partners = [jack, jill]
        };

        _orgSessionMock.ReExManualInputSession.TypesOfPartner = typesOfPartner;

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipCheckNamesOfPartnersDelete(Guid.NewGuid());

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(LimitedPartnershipController.NonCompaniesHousePartnershipCheckNamesOfPartners));

        _orgSessionMock.ReExManualInputSession.TypesOfPartner.Partners.Count.Should().Be(2);
    }
}