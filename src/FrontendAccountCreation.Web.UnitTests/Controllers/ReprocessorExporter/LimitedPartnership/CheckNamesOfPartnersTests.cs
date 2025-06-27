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
public class CheckNamesOfPartnersTests : LimitedPartnershipTestBase
{
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
                PagePath.LimitedPartnershipCheckNamesOfPartners
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
    public async Task CheckNamesOfPartners_Get_ReturnsView()
    {
        // Act
        var result = await _systemUnderTest.CheckNamesOfPartners();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<List<ReExPersonOrCompanyPartner>>();
    }

    [TestMethod]
    public async Task CheckNamesOfPartners_Get_SetsBackLink()
    {
        // Act
        var result = await _systemUnderTest.CheckNamesOfPartners();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.ViewData["BackLinkToDisplay"].Should().Be(PagePath.LimitedPartnershipNamesOfPartners);

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), _orgSessionMock), Times.Never);
    }

    [TestMethod]
    public async Task CheckNamesOfPartnersDelete_Get_RemovesPartnerFromSession()
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

        List<ReExPersonOrCompanyPartner> model = [jack, jill];
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Partners = model;

        // Act
        var result = await _systemUnderTest.CheckNamesOfPartnersDelete(jack.Id);

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(LimitedPartnershipController.CheckNamesOfPartners));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), _orgSessionMock), Times.Once);

        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Partners.Should()
            .NotBeNull().And
            .ContainSingle().Which
            .Should()
            .BeEquivalentTo(jill);
    }

    [TestMethod]
    public async Task CheckNamesOfPartnersDelete_Get_WhenGivenUnmatchedId_RedirectsTo_CheckNamesOfPartners()
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

        List<ReExPersonOrCompanyPartner> model = [jack, jill];
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Partners = model;
        // Act
        var result = await _systemUnderTest.CheckNamesOfPartnersDelete(Guid.NewGuid());

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(LimitedPartnershipController.CheckNamesOfPartners));

        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Partners.Count.Should().Be(2);
    }

    [TestMethod]
    public async Task CheckNamesOfPartners_Post_Save_RedirectsToCorrectPage()
    {
        // Arrange
        List<ReExPersonOrCompanyPartner> modelNotUsed = new();

        // Act
        var result = await _systemUnderTest.CheckNamesOfPartners(modelNotUsed);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), _orgSessionMock), Times.Once);

        redirectResult.ActionName.Should().Be(nameof(LimitedPartnershipController.LimitedPartnershipRole));
    }

    [TestMethod]
    public async Task CheckNamesOfPartners_Get_WhenPartnershipIsNull_ReturnsEmptyModel()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession.Partnership = null;

        // Act
        var result = await _systemUnderTest.CheckNamesOfPartners();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as List<ReExPersonOrCompanyPartner>;
        model.Should().NotBeNull();
        model.Should().BeEmpty(); // Model is empty as Partnership is null
    }

    [TestMethod]
    public async Task CheckNamesOfPartners_Get_WhenLimitedPartnershipIsNull_ReturnsEmptyModel()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership = null;

        // Act
        var result = await _systemUnderTest.CheckNamesOfPartners();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as List<ReExPersonOrCompanyPartner>;
        model.Should().NotBeNull();
        model.Should().BeEmpty(); // Model is empty as LimitedPartnership is null
    }
}