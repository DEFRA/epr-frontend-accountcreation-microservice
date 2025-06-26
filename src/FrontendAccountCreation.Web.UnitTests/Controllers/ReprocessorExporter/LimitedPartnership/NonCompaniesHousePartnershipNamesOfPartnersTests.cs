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
public class NonCompaniesHousePartnershipNamesOfPartnersTests : LimitedPartnershipTestBase
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
                PagePath.NonCompaniesHousePartnershipType,
                PagePath.NonCompaniesHousePartnershipNamesOfPartners
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
    public async Task NonCompaniesHousePartnershipNamesOfPartners_Get_WhenSessionIsEmpty_ReturnsViewWithNewEmptyPartner()
    {
        // Arrange
        _orgSessionMock.ReExManualInputSession.TypesOfPartner = null;

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipNamesOfPartners();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        var viewModel = viewResult.Model.Should().BeOfType<PartnershipPartnersViewModel>().Which;
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
    public async Task NonCompaniesHousePartnershipNamesOfPartners_Get_ReturnsViewPopulatedFromSession(bool hasCompanyPartners, bool hasIndividualPartners, int expectedCount)
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

        ReExTypesOfPartner typesOfPartner = new()
        {
            HasCompanyPartners = hasCompanyPartners,
            HasIndividualPartners = hasIndividualPartners,
            Partners = [abduls, biffa, copper, joanne, raj]
        };
        _orgSessionMock.ReExManualInputSession.TypesOfPartner = typesOfPartner;

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipNamesOfPartners();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        var viewModel = viewResult.Model.Should().BeOfType<PartnershipPartnersViewModel>().Which;
        viewModel.Partners.Should().HaveCount(expectedCount);
    }

    [TestMethod]
    [DataRow(false, false, 1)] // this should not happen
    [DataRow(true, false, 3)]
    [DataRow(false, true, 2)]
    [DataRow(true, true, 4)]
    public async Task NonCompaniesHousePartnershipNamesOfPartners_Get_WhenGivenNewPartner_ReturnsViewPopulatedFromSession(bool hasCompanyPartners, bool hasIndividualPartners, int expectedCount)
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

        ReExTypesOfPartner typesOfPartner = new()
        {
            HasCompanyPartners = hasCompanyPartners,
            HasIndividualPartners = hasIndividualPartners,
            Partners = [abduls, biffa, joanne, newbie]
        };
        _orgSessionMock.ReExManualInputSession.TypesOfPartner = typesOfPartner;

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipNamesOfPartners();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        var viewModel = viewResult.Model.Should().BeOfType<PartnershipPartnersViewModel>().Which;
        viewModel.Partners.Should().HaveCount(expectedCount);
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipNamesOfPartners_Get_UpdatesBacklink()
    {
        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipNamesOfPartners();

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<PartnershipPartnersViewModel>();

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), _orgSessionMock), Times.Never);
        viewResult.ViewData["BackLinkToDisplay"].Should().Be(PagePath.NonCompaniesHousePartnershipType);
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
        ReExTypesOfPartner typesOfPartner = new()
        {
            Partners = partners
        };
        _orgSessionMock.ReExManualInputSession.TypesOfPartner = typesOfPartner;

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipNamesOfPartnersDelete(jack.Id);

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(LimitedPartnershipController.NonCompaniesHousePartnershipNamesOfPartners));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), _orgSessionMock), Times.Once);

        _orgSessionMock.ReExManualInputSession.TypesOfPartner.Partners?.Count.Should().Be(1);
        _orgSessionMock.ReExManualInputSession.TypesOfPartner.Partners[0].Should().BeEquivalentTo(jill);
    }

    [TestMethod]
    [DataRow(false, false, "NonCompaniesHousePartnershipNamesOfPartners.ValidationError_Both")] // this should not happen
    [DataRow(true, false, "NonCompaniesHousePartnershipNamesOfPartners.ValidationError_Company")]
    [DataRow(false, true, "NonCompaniesHousePartnershipNamesOfPartners.ValidationError_Individual")]
    [DataRow(true, true, "NonCompaniesHousePartnershipNamesOfPartners.ValidationError_Both")]
    public async Task NonCompaniesHousePartnershipNamesOfPartners_Post_ModelStateInvalid_ReturnsError(bool hasCompanyPartners, bool hasIndividualPartners, string expectedError)
    {
        // Arrange
        PartnershipPartnersViewModel model = new()
        {
            ExpectsCompanyPartners = hasCompanyPartners,
            ExpectsIndividualPartners = hasIndividualPartners
        };

        _systemUnderTest.ModelState.AddModelError("Error", "Error Message");

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipNamesOfPartners(model, "save");

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        viewResult.Model.Should().BeOfType<PartnershipPartnersViewModel>();

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
    public async Task NonCompaniesHousePartnershipNamesOfPartners_Post_Add_AppendsEmptyPartnerToSession()
    {
        // Arrange
        var jill = new ReExPersonOrCompanyPartner
        {
            Id = Guid.NewGuid(),
            Name = "Jill",
            IsPerson = true,
        };

        List<ReExPersonOrCompanyPartner> partners = [jill];

        ReExTypesOfPartner typesOfPartner = new()
        {
            Partners = partners
        };
        _orgSessionMock.ReExManualInputSession.TypesOfPartner = typesOfPartner;

        PartnershipPartnersViewModel model = new()
        {
            ExpectsIndividualPartners = true,
            Partners = partners.Select(item => (PartnershipPersonOrCompanyViewModel)item).ToList()
        };

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipNamesOfPartners(model, "add");

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        var viewModel = viewResult.Model.Should().BeOfType<PartnershipPartnersViewModel>().Which;
        viewModel.Partners.Should().HaveCount(2);
        viewModel.Partners[0].Id.Should().Be(jill.Id);
        viewModel.Partners[0].PersonName.Should().Be("Jill");
        viewModel.Partners[1].PersonName.Should().BeNull();

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), _orgSessionMock), Times.Once);
        viewResult.ViewData["BackLinkToDisplay"].Should().Be(PagePath.NonCompaniesHousePartnershipType);

        _orgSessionMock.ReExManualInputSession.TypesOfPartner.Partners?.Count.Should().Be(2);
        _orgSessionMock.ReExManualInputSession.TypesOfPartner.Partners[0].Should().BeEquivalentTo(jill);
        _orgSessionMock.ReExManualInputSession.TypesOfPartner.Partners[1].Name.Should().BeNull();
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipNamesOfPartners_Post_Save_UpdatesSessionAndRedirects()
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

        PartnershipPartnersViewModel model = new()
        {
            ExpectsCompanyPartners = true,
            ExpectsIndividualPartners = true,
            Partners = partners.Select(item => (PartnershipPersonOrCompanyViewModel)item).ToList()
        };

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipNamesOfPartners(model, "save");

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), _orgSessionMock), Times.Once);

        _orgSessionMock.ReExManualInputSession.TypesOfPartner.Partners?.Count.Should().Be(2);
        _orgSessionMock.ReExManualInputSession.TypesOfPartner.Partners[0].Should().BeEquivalentTo(jack);
        _orgSessionMock.ReExManualInputSession.TypesOfPartner.Partners[1].Should().BeEquivalentTo(biffa);

        redirectResult.ActionName.Should().Be(nameof(LimitedPartnershipController.CheckNamesOfPartners));
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipNamesOfPartnersDelete_Get_UpdatesSession_And_Redirects()
    {
        // Arrange
        Guid jackId = Guid.NewGuid();
        Guid jillId = Guid.NewGuid();
        var teamMembers = new List<ReExCompanyTeamMember?>
        {
            new() { Id = jackId, FirstName = "Jack", LastName = "Smith" },
            new() { Id = jillId, FirstName = "Jill", LastName = "Test" },
        };

        _orgSessionMock.ReExManualInputSession.TeamMembers = teamMembers;

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(_orgSessionMock);

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipNamesOfPartnersDelete(jackId);

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(LimitedPartnershipController.NonCompaniesHousePartnershipNamesOfPartners));

        _orgSessionMock.ReExManualInputSession.TeamMembers.Should().ContainSingle(x => x.Id == jillId);
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipNamesOfPartnersDelete_Get_WhenGivenUnMatchedId_Redirects()
    {
        // Arrange
        var teamMembers = new List<ReExCompanyTeamMember?>
        {
            new() { Id = Guid.NewGuid(), FirstName = "Jack", LastName = "Smith" },
            new() { Id = Guid.NewGuid(), FirstName = "Jill", LastName = "Test" },
        };

        _orgSessionMock.ReExManualInputSession.TeamMembers = teamMembers;

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(_orgSessionMock);

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipNamesOfPartnersDelete(Guid.NewGuid());

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(LimitedPartnershipController.NonCompaniesHousePartnershipNamesOfPartners));

        _orgSessionMock.ReExManualInputSession.TeamMembers.Count.Should().Be(2);
    }
}