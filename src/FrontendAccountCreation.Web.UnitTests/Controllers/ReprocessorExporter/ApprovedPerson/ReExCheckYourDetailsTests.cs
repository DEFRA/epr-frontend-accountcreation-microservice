using FluentAssertions;
using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Services.Dto.Company;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.ApprovedPerson;

[TestClass]
public class ReExCheckYourDetailsTests : ApprovedPersonTestBase
{
    private ReExCompaniesHouseSession _companiesHouseSession = null!;
    private ReExManualInputSession _manualInputSession = null!;
    private Company _company = null!;
    private ReExCompanyTeamMember _teamMember = null!;
    private string _tradingName = "Acme Ltd.";

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _company = new Company
        {
            CompaniesHouseNumber = "12345678",
            Name = "TestCo Ltd",
            BusinessAddress = new Address
            {
                BuildingNumber = "10",
                BuildingName = "Block A",
                Street = "High Street",
                Town = "Testville",
                County = "Testshire",
                Postcode = "TE5 6ST"
            }
        };

        _teamMember = new ReExCompanyTeamMember
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane"
        };

        _companiesHouseSession = new ReExCompaniesHouseSession
        {
            Company = _company,
            RoleInOrganisation = RoleInOrganisation.Director,
            TeamMembers = new List<ReExCompanyTeamMember> { _teamMember }
        };

        _manualInputSession = new ReExManualInputSession
        {
            TradingName = _tradingName
        };
    }

    [TestMethod]
    public async Task CheckYourDetails_WhenCompaniesHouseFlow_ShouldPopulateCompanyDetails()
    {
        // Arrange
        var session = new OrganisationSession
        {
            IsTheOrganisationCharity = true,
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            IsTradingNameDifferent = true,
            UkNation = Nation.England,
            ReExCompaniesHouseSession = _companiesHouseSession
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var model = ((ViewResult)result).Model.As<ReExCheckYourDetailsViewModel>();

        model.CompanyName.Should().Be(_company.Name);
        model.CompaniesHouseNumber.Should().Be(_company.CompaniesHouseNumber);
        model.BusinessAddress.Should().BeEquivalentTo(_company.BusinessAddress);
        model.RoleInOrganisation.Should().Be(RoleInOrganisation.Director);
        model.reExCompanyTeamMembers.Should().ContainSingle(x => x.Id == _teamMember.Id);
        model.IsCompaniesHouseFlow.Should().BeTrue();
        model.IsRegisteredAsCharity.Should().BeTrue();
        model.OrganisationType.Should().Be(OrganisationType.CompaniesHouseCompany);
        model.IsTradingNameDifferent.Should().BeTrue();
        model.Nation.Should().Be(Nation.England);
    }

    [TestMethod]
    public async Task CheckYourDetails_WhenManualInput_ShouldSetTradingName()
    {
        // Arrange
        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            IsTradingNameDifferent = false,
            UkNation = Nation.Scotland,
            ReExManualInputSession = _manualInputSession
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var model = ((ViewResult)result).Model.As<ReExCheckYourDetailsViewModel>();

        model.TradingName.Should().Be(_tradingName);
        model.IsCompaniesHouseFlow.Should().BeFalse();
        model.Nation.Should().Be(Nation.Scotland);
        model.OrganisationType.Should().Be(OrganisationType.NonCompaniesHouseCompany);
    }

    [TestMethod]
    public async Task CheckYourDetails_ShouldAlwaysCallSaveSession()
    {
        // Arrange
        var session = new OrganisationSession
        {
            ReExCompaniesHouseSession = _companiesHouseSession
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        await _systemUnderTest.CheckYourDetails();

        // Assert
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), session), Times.Once);
    }
}