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
            IsOrganisationAPartnership = true,
            Journey = new List<string>
            {
                PagePath.IsPartnership,
                PagePath.PartnershipType,
                PagePath.LimitedPartnershipType,
                PagePath.LimitedPartnershipNamesOfPartners
            },
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
    public async Task NamesOfPartners_Get_WhenSessionIsEmpty_ReturnsViewWithNewEmptyPartner()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership = null;

        // Act
        var result = await _systemUnderTest.NamesOfPartners();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        var viewModel = viewResult.Model.Should().BeOfType<LimitedPartnershipPartnersViewModel>().Which;
        viewModel.Partners.Should().ContainSingle();

        viewModel.Partners.Should().HaveCount(1);
        viewModel.Partners[0].Id.Should().NotBeEmpty();
        viewModel.Partners[0].PersonName.Should().BeNull();
        viewModel.Partners[0].CompanyName.Should().BeNull();

        viewModel.ExpectsCompanyPartners.Should().BeTrue();
        viewModel.ExpectsIndividualPartners.Should().BeTrue();
    }

    [TestMethod]
    [DataRow(false, false, 1)] // this should not happen
    [DataRow(true, false, 3)]
    [DataRow(false, true, 2)]
    [DataRow(true, true, 5)]
    public async Task NamesOfPartners_Get_ReturnsViewPopulatedFromSession(bool hasCompanyPartners, bool hasIndividualPartners, int expectedCount)
    {
        // Arrange
        var abduls = new ReExPersonOrCompanyPartner
        {
            Name = "Abduls Skip Hire"
        };

        var biffa = new ReExPersonOrCompanyPartner
        {
            Name = "Biffa Waste Inc"
        };

        var copper = new ReExPersonOrCompanyPartner
        {
            Name = "Propper Copper Recycling"
        };

        var joanne = new ReExPersonOrCompanyPartner
        {
            Name = "Joanne Smith",
            IsPerson = true,
        };

        var raj = new ReExPersonOrCompanyPartner
        {
            Name = "Raj Singh",
            IsPerson = true,
        };

        List<ReExPersonOrCompanyPartner> partners = [abduls, biffa, copper, joanne, raj];
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.HasCompanyPartners = hasCompanyPartners;
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.HasIndividualPartners = hasIndividualPartners;
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Partners = partners;

        // Act
        var result = await _systemUnderTest.NamesOfPartners();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        var viewModel = viewResult.Model.Should().BeOfType<LimitedPartnershipPartnersViewModel>().Which;
        viewModel.Partners.Should().HaveCount(expectedCount);
    }

    [TestMethod]
    [DataRow(false, false, 1)] // this should not happen
    [DataRow(true, false, 3)]
    [DataRow(false, true, 2)]
    [DataRow(true, true, 4)]
    public async Task NamesOfPartners_Get_WhenGivenNewPartner_ReturnsViewPopulatedFromSession(bool hasCompanyPartners, bool hasIndividualPartners, int expectedCount)
    {
        // Arrange
        var abduls = new ReExPersonOrCompanyPartner
        {
            Name = "Abduls Skip Hire"
        };

        var biffa = new ReExPersonOrCompanyPartner
        {
            Name = "Biffa Waste Inc"
        };

        var joanne = new ReExPersonOrCompanyPartner
        {
            Name = "Joanne Smith",
            IsPerson = true,
        };

        var newbie = new ReExPersonOrCompanyPartner();

        List<ReExPersonOrCompanyPartner> partners = [abduls, biffa, joanne, newbie];
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.HasCompanyPartners = hasCompanyPartners;
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.HasIndividualPartners = hasIndividualPartners;
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Partners = partners;

        // Act
        var result = await _systemUnderTest.NamesOfPartners();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        var viewModel = viewResult.Model.Should().BeOfType<LimitedPartnershipPartnersViewModel>().Which;
        viewModel.Partners.Should().HaveCount(expectedCount);
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
    public async Task NamesOfPartnersDelete_Get_RemovesPartnerFromSession()
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

        List<ReExPersonOrCompanyPartner> partners = [jack, jill];
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Partners = partners;

        LimitedPartnershipPartnersViewModel model = new()
        {
            ExpectsIndividualPartners = true,
            Partners = partners.Select(item => (PartnershipPersonOrCompanyViewModel)item).ToList()
        };

        // Act
        var result = await _systemUnderTest.NamesOfPartnersDelete(jack.Id);

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(LimitedPartnershipController.NamesOfPartners));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), _orgSessionMock), Times.Once);

        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Partners?.Count.Should().Be(1);
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Partners[0].Should().BeEquivalentTo(jill);
    }

    [TestMethod]
    [DataRow(false, false, "NamesOfPartners.ValidationError_Both")] // this should not happen
    [DataRow(true, false, "NamesOfPartners.ValidationError_Company")]
    [DataRow(false, true, "NamesOfPartners.ValidationError_Individual")]
    [DataRow(true, true, "NamesOfPartners.ValidationError_Both")]
    public async Task NamesOfPartners_Post_ModelStateInvalid_ReturnsError(bool hasCompanyPartners, bool hasIndividualPartners, string expectedError)
    {
        // Arrange
        LimitedPartnershipPartnersViewModel model = new()
        {
            ExpectsCompanyPartners = hasCompanyPartners,
            ExpectsIndividualPartners = hasIndividualPartners
        };

        _systemUnderTest.ModelState.AddModelError("Error", "Error Message");

        // Act
        var result = await _systemUnderTest.NamesOfPartners(model, "save");

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        viewResult.Model.Should().BeOfType<LimitedPartnershipPartnersViewModel>();

        _systemUnderTest.ModelState.IsValid.Should().BeFalse();
        var errors = _systemUnderTest.ModelState.Select(x => x.Value.Errors)
                          .Where(y => y.Count > 0)
                          .ToList();

        errors.Should().NotBeEmpty();
        errors.Count.Should().Be(1);
        var modelErrorCollection = errors[0];
        modelErrorCollection[0].ErrorMessage.Should().Be(expectedError);
    }

    [TestMethod]
    public async Task NamesOfPartners_Post_Add_AppendsEmptyPartnerToSession()
    {
        // Arrange
        var jill = new ReExPersonOrCompanyPartner
        {
            Id = Guid.NewGuid(),
            Name = "Jill",
            IsPerson = true,
        };

        List<ReExPersonOrCompanyPartner> partners = [jill];
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Partners = partners;

        LimitedPartnershipPartnersViewModel model = new()
        {
            ExpectsIndividualPartners = true,
            Partners = partners.Select(item => (PartnershipPersonOrCompanyViewModel)item).ToList()
        };

        // Act
        var result = await _systemUnderTest.NamesOfPartners(model, "add");

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        var viewModel = viewResult.Model.Should().BeOfType<LimitedPartnershipPartnersViewModel>().Which;
        viewModel.Partners.Should().HaveCount(2);
        viewModel.Partners[0].Id.Should().Be(jill.Id);
        viewModel.Partners[0].PersonName.Should().Be("Jill");
        viewModel.Partners[1].PersonName.Should().BeNull();

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), _orgSessionMock), Times.Once);
        viewResult.ViewData["BackLinkToDisplay"].Should().Be(PagePath.LimitedPartnershipType);

        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Partners?.Count.Should().Be(2);
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Partners[0].Should().BeEquivalentTo(jill);
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Partners[1].Name.Should().BeNull();
    }

    [TestMethod]
    public async Task NamesOfPartners_Post_Save_UpdatesSessionAndRedirects()
    {
        // Arrange
        var jack = new ReExPersonOrCompanyPartner
        {
            Id = Guid.NewGuid(),
            Name = "Jack",
            IsPerson = true,
        };

        var biffa = new ReExPersonOrCompanyPartner
        {
            Id = Guid.NewGuid(),
            Name = "Biffa Waste Inc",
            IsPerson = false,
        };

        List<ReExPersonOrCompanyPartner> partners = [jack, biffa];

        LimitedPartnershipPartnersViewModel model = new()
        {
            ExpectsCompanyPartners = true,
            ExpectsIndividualPartners = true,
            Partners = partners.Select(item => (PartnershipPersonOrCompanyViewModel)item).ToList()
        };

        // Act
        var result = await _systemUnderTest.NamesOfPartners(model, "save");

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), _orgSessionMock), Times.Once);

        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Partners?.Count.Should().Be(2);
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Partners[0].Should().BeEquivalentTo(jack);
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Partners[1].Should().BeEquivalentTo(biffa);

        redirectResult.ActionName.Should().Be(nameof(LimitedPartnershipController.CheckNamesOfPartners));
    }

    [TestMethod]
    public async Task LimitedPartnershipNamesOfPartnersDelete_Get_UpdatesSession_And_RedirectsTo_NamesOfPartners()
    {
        // Arrange
        Guid jackId = Guid.NewGuid();
        Guid jillId = Guid.NewGuid();
        var teamMembers = new List<ReExCompanyTeamMember?>
        {
            new() { Id = jackId, FirstName = "Jack", LastName = "Smith" },
            new() { Id = jillId, FirstName = "Jill", LastName = "Test" },
        };

        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers = teamMembers;

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(_orgSessionMock);

        // Act
        var result = await _systemUnderTest.NamesOfPartnersDelete(jackId);

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(LimitedPartnershipController.NamesOfPartners));

        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers.Should().ContainSingle(x => x.Id == jillId);
    }

    [TestMethod]
    public async Task LimitedPartnershipNamesOfPartnersDelete_Get_WhenGivenUnMatchedId_RedirectsTo_NamesOfPartners()
    {
        // Arrange
        var teamMembers = new List<ReExCompanyTeamMember?>
        {
            new() { Id = Guid.NewGuid(), FirstName = "Jack", LastName = "Smith" },
            new() { Id = Guid.NewGuid(), FirstName = "Jill", LastName = "Test" },
        };

        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers = teamMembers;

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(_orgSessionMock);

        // Act
        var result = await _systemUnderTest.NamesOfPartnersDelete(Guid.NewGuid());

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(LimitedPartnershipController.NamesOfPartners));

        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers.Count.Should().Be(2);
    }
}