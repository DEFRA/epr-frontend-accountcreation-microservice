using FluentAssertions;
using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Services.Dto.Company;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Core.Sessions.ReEx.Partnership;
using FrontendAccountCreation.Web.Configs;
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
    private ReExPartnership _partnership = new();

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

        _partnership = new ReExPartnership
        {
            LimitedPartnership = new ReExLimitedPartnership
            {
                Partners = new List<ReExLimitedPartnershipPersonOrCompany>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "test",
                        IsPerson = true
                    }
                }
            }
        };

        _companiesHouseSession = new ReExCompaniesHouseSession
        {
            Company = _company,
            RoleInOrganisation = RoleInOrganisation.Director,
            TeamMembers = new List<ReExCompanyTeamMember> { _teamMember },
            Partnership = _partnership
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
            ReExCompaniesHouseSession = _companiesHouseSession,
            IsOrganisationAPartnership = true
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
        model.IsOrganisationAPartnership.Should().BeTrue();
        model.LimitedPartnershipPartners.Should().BeEquivalentTo(_partnership.LimitedPartnership.Partners);
    }

    [TestMethod]
    public async Task CheckYourDetails_WhenCompaniesHouseFlow_MissingSome_Data()
    {
        // Arrange
        _companiesHouseSession.Company.Name = null;
        _companiesHouseSession.Company.CompaniesHouseNumber = null;
        _companiesHouseSession.RoleInOrganisation = null;

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

        model.CompanyName.Should().BeNullOrEmpty();
        model.CompaniesHouseNumber.Should().BeNullOrEmpty();
        model.BusinessAddress.Should().BeEquivalentTo(_company.BusinessAddress);
        model.reExCompanyTeamMembers.Should().ContainSingle(x => x.Id == _teamMember.Id);
        model.IsCompaniesHouseFlow.Should().BeTrue();
        model.IsRegisteredAsCharity.Should().BeTrue();
        model.OrganisationType.Should().Be(OrganisationType.CompaniesHouseCompany);
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

    [TestMethod]
    [DataRow(ProducerType.NonUkOrganisation, "CheckYourDetails.NonUkOrganisation")]
    [DataRow(ProducerType.Partnership, "CheckYourDetails.Partnership")]
    [DataRow(ProducerType.SoleTrader, "CheckYourDetails.SoleTrader")]
    [DataRow(ProducerType.UnincorporatedBody, "CheckYourDetails.UnincorporatedBody")]
    [DataRow(ProducerType.Other, "CheckYourDetails.Other")]
    [DataRow((ProducerType)999, "")]
    public void TypeOfProducer_ShouldReturnCorrectResourceKey(ProducerType input, string expected)
    {
        // Arrange
        var viewModel = new ReExCheckYourDetailsViewModel
        {
            ProducerType = input
        };

        // Act
        var result = viewModel.TypeOfProducer;

        // Assert
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow(RoleInOrganisation.Director, "CheckYourDetails.Director")]
    [DataRow(RoleInOrganisation.CompanySecretary, "CheckYourDetails.CompanySecretary")]
    [DataRow(RoleInOrganisation.Partner, "CheckYourDetails.Partner")]
    [DataRow(RoleInOrganisation.Member, "CheckYourDetails.Member")]
    [DataRow((RoleInOrganisation)999, "")]
    public void YourRole_ShouldReturnCorrectResourceKey(RoleInOrganisation input, string expected)
    {
        // Arrange
        var viewModel = new ReExCheckYourDetailsViewModel
        {
            RoleInOrganisation = input
        };

        // Act
        var result = viewModel.YourRole;

        // Assert
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow(Nation.England, "CheckYourDetails.England")]
    [DataRow(Nation.Scotland, "CheckYourDetails.Scotland")]
    [DataRow(Nation.Wales, "CheckYourDetails.Wales")]
    [DataRow(Nation.NorthernIreland, "CheckYourDetails.NorthernIreland")]
    [DataRow((Nation)999, "")]
    public void UkNation_ShouldReturnCorrectResourceKey(Nation input, string expected)
    {
        // Arrange
        var viewModel = new ReExCheckYourDetailsViewModel
        {
            Nation = input
        };

        // Act
        var result = viewModel.UkNation;

        // Assert
        result.Should().Be(expected);
    }

    [TestMethod]
    public async Task CheckYourDetails_WhenCompaniesHouseFlow_ShouldSetBusinessAddress()
    {
        // Arrange
        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            ReExCompaniesHouseSession = _companiesHouseSession
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();
        var model = ((ViewResult)result).Model.As<ReExCheckYourDetailsViewModel>();

        // Assert
        model.BusinessAddress.Should().BeEquivalentTo(_companiesHouseSession.Company.BusinessAddress);
    }

    [TestMethod]
    public async Task CheckYourDetails_WhenCompaniesHouseFlow_ShouldSetCompanyName()
    {
        // Arrange
        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            ReExCompaniesHouseSession = _companiesHouseSession
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();
        var model = ((ViewResult)result).Model.As<ReExCheckYourDetailsViewModel>();

        // Assert
        model.CompanyName.Should().Be(_companiesHouseSession.Company.Name);
    }

    [TestMethod]
    public async Task CheckYourDetails_WhenCompaniesHouseFlow_ShouldSetCompaniesHouseNumber()
    {
        // Arrange
        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            ReExCompaniesHouseSession = _companiesHouseSession
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();
        var model = ((ViewResult)result).Model.As<ReExCheckYourDetailsViewModel>();

        // Assert
        model.CompaniesHouseNumber.Should().Be(_companiesHouseSession.Company.CompaniesHouseNumber);
    }

    [TestMethod]
    public async Task CheckYourDetails_WhenCompaniesHouseFlow_ShouldSetRoleInOrganisation()
    {
        // Arrange
        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            ReExCompaniesHouseSession = _companiesHouseSession
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();
        var model = ((ViewResult)result).Model.As<ReExCheckYourDetailsViewModel>();

        // Assert
        model.RoleInOrganisation.Should().Be(_companiesHouseSession.RoleInOrganisation);
    }

    [TestMethod]
    public async Task CheckYourDetails_WhenManualInputFlow_ShouldSetIsSoleTrader()
    {
        // Arrange
        var manualInputSession = new ReExManualInputSession
        {
            ProducerType = ProducerType.SoleTrader
        };

        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            ReExManualInputSession = manualInputSession
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();
        var model = ((ViewResult)result).Model.As<ReExCheckYourDetailsViewModel>();

        // Assert
        model.IsSoleTrader.Should().BeTrue();
        model.ProducerType.Should().Be(ProducerType.SoleTrader);
    }

    [TestMethod]
    public async Task CheckYourDetailsPost_ShouldRedirectToDeclaration()
    {
        // Arrange
        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            ReExCompaniesHouseSession = _companiesHouseSession
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                           .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.CheckYourDetailsPost();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;

        redirectResult!.ActionName.Should().Be(nameof(OrganisationController.Declaration));
        redirectResult.ControllerName.Should().Be("Organisation");
    }

    [TestMethod]
    public async Task CheckYourDetails_ShouldSet_MakeChangesToYourLimitedCompanyLink_ViewBag()
    {
        // Arrange
        var session = new OrganisationSession
        {
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession()
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var link = _systemUnderTest.ViewBag.MakeChangesToYourLimitedCompanyLink as string;
        link.Should().Be("https://gov.uk/update-company-info");
    }

    [TestMethod]
    public async Task CheckYourDetails_WithNullSessions_ShouldNotThrow()
    {
        // Arrange
        var session = new OrganisationSession
        {
            ReExCompaniesHouseSession = null,
            ReExManualInputSession = null
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var model = ((ViewResult)result).Model.As<ReExCheckYourDetailsViewModel>();
        model.CompanyName.Should().BeNull();
        model.TradingName.Should().BeNull();
    }

    [TestMethod]
    public async Task CheckYourDetails_WhenLimitedCompanyWithNoTeamMembers_ShouldHandleEmptyTeamList()
    {
        // Arrange
        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                Company = new Company
                {
                    Name = "No Team Ltd",
                    CompaniesHouseNumber = "99999999"
                },
                TeamMembers = new List<ReExCompanyTeamMember>() // Empty list
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();
        var model = ((ViewResult)result).Model.As<ReExCheckYourDetailsViewModel>();

        // Assert
        model.CompanyName.Should().Be("No Team Ltd");
        model.reExCompanyTeamMembers.Should().BeEmpty();
    }

    [TestMethod]
    public async Task CheckYourDetails_WhenIsCompaniesHouseFlowIsTrue_ShouldPopulateWithCompanyHouseSession()
    {
        // Arrange
        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                Company = new Company
                {
                    Name = "Test Ltd",
                    CompaniesHouseNumber = "12345678"
                },
                TeamMembers = new List<ReExCompanyTeamMember>
            {
                new ReExCompanyTeamMember { FirstName = "Alice", LastName = "Smith" }
            }
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();
        var model = ((ViewResult)result).Model.As<ReExCheckYourDetailsViewModel>();

        // Assert
        model.CompanyName.Should().Be("Test Ltd");
        model.CompaniesHouseNumber.Should().Be("12345678");
        model.reExCompanyTeamMembers.Should().ContainSingle(x => x.FirstName == "Alice");
    }

    [TestMethod]
    public async Task CheckYourDetails_WhenIsSoleTraderIsTrue_ShouldPopulateWithManualInputSession()
    {
        // Arrange
        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            ReExManualInputSession = new ReExManualInputSession
            {
                ProducerType = ProducerType.SoleTrader,
                TradingName = "Sole Trader Ltd",
                TeamMembers = new List<ReExCompanyTeamMember>
            {
                new ReExCompanyTeamMember { FirstName = "Bob", LastName = "Johnson" }
            }
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();
        var model = ((ViewResult)result).Model.As<ReExCheckYourDetailsViewModel>();

        // Assert
        model.TradingName.Should().Be("Sole Trader Ltd");
        model.reExCompanyTeamMembers.Should().ContainSingle(x => x.FirstName == "Bob");
    }

    [TestMethod]
    public async Task CheckYourDetails_WhenIsNonUkIsTrue_ShouldPopulateWithNonUkOrganisation()
    {
        // Arrange
        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            IsUkMainAddress = false, // Indicating Non-UK
            ReExManualInputSession = new ReExManualInputSession
            {
                ProducerType = ProducerType.NonUkOrganisation,
                NonUkOrganisationName = "Non-UK Ltd",
                TeamMembers = new List<ReExCompanyTeamMember>
            {
                new ReExCompanyTeamMember { FirstName = "Charlie", LastName = "Davis" }
            }
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();
        var model = ((ViewResult)result).Model.As<ReExCheckYourDetailsViewModel>();

        // Assert
        model.TradingName.Should().Be("Non-UK Ltd");
        model.reExCompanyTeamMembers.Should().ContainSingle(x => x.FirstName == "Charlie");
    }

    [TestMethod]
    public async Task CheckYourDetails_WhenNonUkFlow_ShouldSetIsNonUkAndPopulateManualInputFields()
    {
        // Arrange
        var teamMember = new ReExCompanyTeamMember
        {
            Id = Guid.NewGuid(),
            FullName = "Jane Smith",
            TelephoneNumber = "0123456789",
            Email = "jane.smith@example.com",
            FirstName = "Jane",
            LastName = "Smith"
        };

        var manualInputSession = new ReExManualInputSession
        {
            ProducerType = ProducerType.Other,
            BusinessAddress = new Address { Street = "456 International Road" },
            TradingName = "Global Traders",
            TeamMembers = new List<ReExCompanyTeamMember> { teamMember }
        };

        var session = new OrganisationSession
        {
            IsUkMainAddress = false, // Triggers Non-UK flow
            ReExManualInputSession = manualInputSession
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();
        var model = ((ViewResult)result).Model.As<ReExCheckYourDetailsViewModel>();

        // Assert
        model.IsNonUk.Should().BeTrue();
        model.ProducerType.Should().Be(ProducerType.Other);

        model.reExCompanyTeamMembers.Should().ContainSingle();
        var returnedMember = model.reExCompanyTeamMembers.First();

        returnedMember.FullName.Should().Be("Jane Smith");
        returnedMember.TelephoneNumber.Should().Be("0123456789");
        returnedMember.Email.Should().Be("jane.smith@example.com");
        returnedMember.FirstName.Should().Be("Jane");
        returnedMember.LastName.Should().Be("Smith");
    }
}