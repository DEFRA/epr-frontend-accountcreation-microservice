using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Core.Sessions.ReEx.Partnership;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.Sessions;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.LimitedPartnership;

[TestClass]
public class NamesOfPartnersTests : LimitedPartnershipTestBase
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
                PagePath.LimitedPartnershipNamesOfPartners
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
    public async Task NamesOfPartners_Get_WhenSessionIsEmpty_ReturnsViewWithNewEmptyPartner()
    {
        // Act
        var result = await _systemUnderTest.NamesOfPartners();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<LimitedPartnershipPartnersViewModel>();

        ((LimitedPartnershipPartnersViewModel)viewResult.Model).Partners.Should().HaveCount(1);
        ((LimitedPartnershipPartnersViewModel)viewResult.Model).Partners[0].Id.Should().NotBeEmpty();
        ((LimitedPartnershipPartnersViewModel)viewResult.Model).Partners[0].PersonName.Should().BeNull();
        ((LimitedPartnershipPartnersViewModel)viewResult.Model).Partners[0].CompanyName.Should().BeNull();
    }

    [TestMethod]
    [DataRow(false, false, 1)] // this should not happen
    [DataRow(true, false, 3)]
    [DataRow(false, true, 2)]
    [DataRow(true, true, 5)]
    public async Task NamesOfPartners_Get_ReturnsViewPopulatedFromSession(bool hasCompanyPartners, bool hasIndividualPartners, int expectedCount)
    {
        // Arrange
        var abduls = new ReExLimitedPartnershipPersonOrCompany
        {
            Name = "Abduls Skip Hire"
        };

        var biffa = new ReExLimitedPartnershipPersonOrCompany
        {
            Name = "Biffa Waste Inc"
        };

        var copper = new ReExLimitedPartnershipPersonOrCompany
        {
            Name = "Propper Copper Recycling"
        };

        var joanne = new ReExLimitedPartnershipPersonOrCompany
        {
            Name = "Joanne Smith",
            IsPerson = true,
        };

        var raj = new ReExLimitedPartnershipPersonOrCompany
        {
            Name = "Raj Singh",
            IsPerson = true,
        };

        List<ReExLimitedPartnershipPersonOrCompany> partners = [abduls, biffa, copper, joanne, raj];
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.HasCompanyPartners = hasCompanyPartners;
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.HasIndividualPartners = hasIndividualPartners;
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Partners = partners;

        // Act
        var result = await _systemUnderTest.NamesOfPartners();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<LimitedPartnershipPartnersViewModel>();

        ((LimitedPartnershipPartnersViewModel)viewResult.Model).Partners.Should().HaveCount(expectedCount);
    }

    [TestMethod]
    [DataRow(false, false, 1)] // this should not happen
    [DataRow(true, false, 3)]
    [DataRow(false, true, 2)]
    [DataRow(true, true, 4)]
    public async Task NamesOfPartners_Get_WhenGivenNewPartner_ReturnsViewPopulatedFromSession(bool hasCompanyPartners, bool hasIndividualPartners, int expectedCount)
    {
        // Arrange
        var abduls = new ReExLimitedPartnershipPersonOrCompany
        {
            Name = "Abduls Skip Hire"
        };

        var biffa = new ReExLimitedPartnershipPersonOrCompany
        {
            Name = "Biffa Waste Inc"
        };

        var joanne = new ReExLimitedPartnershipPersonOrCompany
        {
            Name = "Joanne Smith",
            IsPerson = true,
        };

        var newbie = new ReExLimitedPartnershipPersonOrCompany();

        List<ReExLimitedPartnershipPersonOrCompany> partners = [abduls, biffa, joanne, newbie];
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.HasCompanyPartners = hasCompanyPartners;
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.HasIndividualPartners = hasIndividualPartners;
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Partners = partners;

        // Act
        var result = await _systemUnderTest.NamesOfPartners();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<LimitedPartnershipPartnersViewModel>();

        ((LimitedPartnershipPartnersViewModel)viewResult.Model).Partners.Should().HaveCount(expectedCount);
    }

    [TestMethod]
    public async Task NamesOfPartners_Get_UpdatesBacklink()
    {
        // Act
        var result = await _systemUnderTest.NamesOfPartners();

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<LimitedPartnershipPartnersViewModel>();

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), _orgSessionMock), Times.Never);
        viewResult.ViewData["BackLinkToDisplay"].Should().Be(PagePath.LimitedPartnershipType);
    }

    [TestMethod]
    public async Task NamesOfPartners_Post_WhenPartnerIdSupplied_RemovesPartnerFromSession()
    {
        // Arrange
        var jack = new ReExLimitedPartnershipPersonOrCompany
        {
            Id = Guid.NewGuid(),
            Name = "Jack",
            IsPerson = true,
        };

        var jill = new ReExLimitedPartnershipPersonOrCompany
        {
            Id = Guid.NewGuid(),
            Name = "Jill",
            IsPerson = true,
        };

        List<ReExLimitedPartnershipPersonOrCompany> partners = [jack, jill];
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Partners = partners;

        LimitedPartnershipPartnersViewModel model = new LimitedPartnershipPartnersViewModel
        {
            ExpectsIndividualPartners = true,
            Partners = partners.Select(item => (LimitedPartnershipPersonOrCompanyViewModel)item).ToList()
        };

        // Act
        var result = await _systemUnderTest.NamesOfPartners(model, jack.Id.ToString());

        // Assert
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<LimitedPartnershipPartnersViewModel>();
        ((LimitedPartnershipPartnersViewModel)viewResult.Model).Partners.Should().HaveCount(1);
        ((LimitedPartnershipPartnersViewModel)viewResult.Model).Partners[0].Id.Should().Be(jill.Id);
        ((LimitedPartnershipPartnersViewModel)viewResult.Model).Partners[0].PersonName.Should().Be("Jill");

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), _orgSessionMock), Times.Once);
        viewResult.ViewData["BackLinkToDisplay"].Should().Be(PagePath.LimitedPartnershipType);

        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Partners?.Count.Should().Be(1);
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Partners[0].Id.Should().Be(jill.Id);
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Partners[0].Name.Should().Be("Jill");
    }
}