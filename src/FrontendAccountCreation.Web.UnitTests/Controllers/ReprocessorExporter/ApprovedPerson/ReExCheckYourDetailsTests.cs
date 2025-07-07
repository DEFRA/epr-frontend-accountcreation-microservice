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
            LimitedPartnership = new ReExTypesOfPartner
            {
                Partners = new List<ReExPersonOrCompanyPartner>
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
    }

    [TestMethod]
    public async Task CheckYourDetails_WhenCompaniesHouseFlow_ShouldPopulateCompanyDetails()
    {
        // Arrange
        var session = new OrganisationSession
        {
            TradingName = _tradingName,
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
            TradingName = _tradingName,
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
            TradingName = _tradingName,
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
            TradingName = _tradingName,
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
            TradingName = _tradingName,
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
            TradingName = _tradingName,
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
            TradingName = _tradingName,
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
            TradingName = _tradingName,
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
            TradingName = _tradingName,
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
            TradingName = _tradingName,
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
            TradingName = _tradingName,
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
            TradingName = _tradingName,
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            ReExManualInputSession = new ReExManualInputSession
            {
                ProducerType = ProducerType.SoleTrader,
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
        model.TradingName.Should().Be(_tradingName);
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
                OrganisationName = "Non-UK Ltd",
                TeamMembers = [new ReExCompanyTeamMember { FirstName = "Charlie", LastName = "Davis" }]
            },
            IsTradingNameDifferent = true,
            TradingName = "NonUK Trade"
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();
        var model = ((ViewResult)result).Model.As<ReExCheckYourDetailsViewModel>();

        // Assert
        model.CompanyName.Should().Be("Non-UK Ltd");
        model.TradingName.Should().Be("NonUK Trade");
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
            TeamMembers = new List<ReExCompanyTeamMember> { teamMember }
        };

        var session = new OrganisationSession
        {
            IsUkMainAddress = false, // Triggers Non-UK flow
            ReExManualInputSession = manualInputSession,
            TradingName = "Global Traders",
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

    [TestMethod]
    public async Task GET_CheckYourDetails_WhenNonUk_SetsCorrectViewModelValues()
    {
        // Arrange
        var manualInput = new ReExManualInputSession
        {
            ProducerType = ProducerType.NonUkOrganisation,
            BusinessAddress = new Address
            {
                Country = "France",
                BuildingName = "address line 1",
                Street = "address line 2",
                Town = "Paris",
                County = "Île-de-France",
                Postcode = "75001",
                IsManualAddress = true
            },
            OrganisationName = "Non UK Org Ltd",
            TeamMembers = new List<ReExCompanyTeamMember>
        {
            new ReExCompanyTeamMember { FirstName = "Jane", LastName = "Doe" }
        },
            UkRegulatorNation = Nation.NorthernIreland
        };

        var session = new OrganisationSession
        {
            IsTheOrganisationCharity = false,
            IsTradingNameDifferent = false,
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            UkNation = Nation.NorthernIreland,
            IsUkMainAddress = false,
            ReExManualInputSession = manualInput
        };

        _sessionManagerMock.Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _sessionManagerMock.Setup(s => s.SaveSessionAsync(It.IsAny<ISession>(), session))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.Model.Should().BeOfType<ReExCheckYourDetailsViewModel>();

        var model = (ReExCheckYourDetailsViewModel)viewResult.Model!;

        model.IsNonUk.Should().BeTrue();
        model.ProducerType.Should().Be(ProducerType.NonUkOrganisation);
        model.BusinessAddress.Should().BeSameAs(manualInput.BusinessAddress);
        model.CompanyName.Should().Be("Non UK Org Ltd");
        model.reExCompanyTeamMembers.Should().HaveCount(1);
        model.reExCompanyTeamMembers![0].FirstName.Should().Be("Jane");
        model.Nation.Should().Be(Nation.NorthernIreland);
    }

    [TestMethod]
    public async Task GET_CheckYourDetails_WhenIsCompaniesHouseFlow_SetsCorrectViewModelProperties()
    {
        // Arrange
        var company = new Company
        {
            BusinessAddress = new Address
            {
                Country = "UK",
                BuildingName = "address line 1",
                Street = "address line 2",
                Town = "London",
                County = "London",
                Postcode = "AB1 2BC"
            },
            Name = "Test Company Ltd",
            CompaniesHouseNumber = "CH123456"
        };

        var teamMembers = new List<ReExCompanyTeamMember>
    {
        new ReExCompanyTeamMember { FirstName = "John", LastName = "Doe" },
        new ReExCompanyTeamMember { FirstName = "Jane", LastName = "Smith" }
    };

        var companyHouseSession = new ReExCompaniesHouseSession
        {
            Company = company,
            RoleInOrganisation = RoleInOrganisation.Director,
            TeamMembers = teamMembers
        };

        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            IsOrganisationAPartnership = true,
            ReExCompaniesHouseSession = companyHouseSession
        };

        _sessionManagerMock.Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _sessionManagerMock.Setup(s => s.SaveSessionAsync(It.IsAny<ISession>(), session))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        var model = (ReExCheckYourDetailsViewModel)viewResult!.Model!;

        model.IsCompaniesHouseFlow.Should().BeTrue();

        model.BusinessAddress.Should().BeSameAs(company.BusinessAddress);
        model.CompanyName.Should().Be(company.Name);
        model.CompaniesHouseNumber.Should().Be(company.CompaniesHouseNumber);
        model.RoleInOrganisation.Should().Be(companyHouseSession.RoleInOrganisation);
        model.reExCompanyTeamMembers.Should().HaveCount(2);
        model.reExCompanyTeamMembers![0].FirstName.Should().Be("John");
        model.reExCompanyTeamMembers[1].LastName.Should().Be("Smith");
    }

    [TestMethod]
    public async Task GET_CheckYourDetails_WhenIsSoleTrader_SetsCorrectViewModelProperties()
    {
        // Arrange
        var teamMember = new ReExCompanyTeamMember
        {
            FirstName = "Sole",
            LastName = "Trader"
        };

        var manualInputSession = new ReExManualInputSession
        {
            ProducerType = ProducerType.SoleTrader,
            BusinessAddress = new Address
            {
                Country = "UK",
                BuildingName = "address line 1",
                Street = "address line 2",
                Town = "London",
                County = "London",
                Postcode = "AB1 2BC",
                IsManualAddress = true,
            },
            TeamMembers = new List<ReExCompanyTeamMember> { teamMember }
        };

        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            TradingName = "Sole Trader Trading",
            ReExManualInputSession = manualInputSession
        };

        _sessionManagerMock.Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _sessionManagerMock.Setup(s => s.SaveSessionAsync(It.IsAny<ISession>(), session))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        var model = (ReExCheckYourDetailsViewModel)viewResult!.Model!;

        model.IsSoleTrader.Should().BeTrue();
        model.ProducerType.Should().Be(ProducerType.SoleTrader);
        model.BusinessAddress.Should().BeSameAs(manualInputSession.BusinessAddress);
        model.TradingName.Should().Be("Sole Trader Trading");

        model.reExCompanyTeamMembers.Should().HaveCount(1);
        model.reExCompanyTeamMembers[0].FirstName.Should().Be("Sole");
        model.reExCompanyTeamMembers[0].LastName.Should().Be("Trader");
    }

    [TestMethod]
    public async Task GET_CheckYourDetails_WhenIsNonUk_AssignsAllManualInputFields()
    {
        // Arrange
        var expectedAddress = new Address
        {
            Country = "France",
            BuildingName = "address line 1",
            Street = "address line 2",
            Town = "Paris",
            County = "Île-de-France",
            Postcode = "75001",
            IsManualAddress = true
        };
        var expectedTeamMembers = new List<ReExCompanyTeamMember>
    {
        new ReExCompanyTeamMember { FirstName = "Anna", LastName = "Smith" }
    };

        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            IsUkMainAddress = false,
            ReExManualInputSession = new ReExManualInputSession
            {
                ProducerType = ProducerType.NonUkOrganisation,
                BusinessAddress = expectedAddress,
                OrganisationName = "Global Org Ltd",
                TeamMembers = expectedTeamMembers,
                UkRegulatorNation = Nation.England
            }
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _sessionManagerMock
            .Setup(x => x.SaveSessionAsync(It.IsAny<ISession>(), session))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewModel = (result as ViewResult)?.Model as ReExCheckYourDetailsViewModel;
        viewModel.Should().NotBeNull();

        viewModel!.IsNonUk.Should().BeTrue();
        viewModel.ProducerType.Should().Be(ProducerType.NonUkOrganisation);
        viewModel.BusinessAddress.Should().BeSameAs(expectedAddress);
        viewModel.CompanyName.Should().Be("Global Org Ltd");
        viewModel.reExCompanyTeamMembers.Should().BeEquivalentTo(expectedTeamMembers);
        viewModel.Nation.Should().Be(Nation.England);
    }

    [TestMethod]
    public async Task GET_CheckYourDetails_WhenSoleTrader_AssignsManualInputFieldsCorrectly()
    {
        // Arrange
        var expectedAddress = new Address
        {
            Country = "UK",
            BuildingName = "address line 1",
            Street = "address line 2",
            Town = "London",
            County = "London",
            Postcode = "AB1 2BC",
            IsManualAddress = true,
        };
        var teamMember = new ReExCompanyTeamMember
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe"
        };

        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            IsUkMainAddress = true,
            TradingName = "Solo Biz Ltd",
            ReExManualInputSession = new ReExManualInputSession
            {
                ProducerType = ProducerType.SoleTrader,
                BusinessAddress = expectedAddress,
                TeamMembers = new List<ReExCompanyTeamMember> { teamMember }
            }
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _sessionManagerMock
            .Setup(x => x.SaveSessionAsync(It.IsAny<ISession>(), session))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        var model = viewResult?.Model as ReExCheckYourDetailsViewModel;

        model.Should().NotBeNull();
        model!.IsSoleTrader.Should().BeTrue();
        model.ProducerType.Should().Be(ProducerType.SoleTrader);
        model.BusinessAddress.Should().BeSameAs(expectedAddress);
        model.TradingName.Should().Be("Solo Biz Ltd");
        model.reExCompanyTeamMembers.Should().HaveCount(1);
        model.reExCompanyTeamMembers[0].Should().BeEquivalentTo(teamMember);
    }

    [TestMethod]
    public async Task GET_CheckYourDetails_WhenCompaniesHouseFlow_AssignsExpectedFields()
    {
        // Arrange
        var company = new Company
        {
            Name = "Companies Ltd",
            BusinessAddress = new Address
            {
                Country = "UK",
                BuildingName = "address line 1",
                Street = "address line 2",
                Town = "London",
                County = "London",
                Postcode = "AB1 2BC"
            },
            CompaniesHouseNumber = "12345678"
        };

        var teamMembers = new List<ReExCompanyTeamMember>
    {
        new ReExCompanyTeamMember { FirstName = "Alice", LastName = "Smith" }
    };

        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            IsOrganisationAPartnership = true,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                Company = company,
                RoleInOrganisation = RoleInOrganisation.Director,
                TeamMembers = teamMembers
            }
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _sessionManagerMock
            .Setup(x => x.SaveSessionAsync(It.IsAny<ISession>(), session))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ReExCheckYourDetailsViewModel>().Subject;

        model.CompanyName.Should().Be("Companies Ltd");
        model.BusinessAddress.Should().Be(company.BusinessAddress);
        model.CompaniesHouseNumber.Should().Be("12345678");
        model.RoleInOrganisation.Should().Be(RoleInOrganisation.Director);
        model.IsOrganisationAPartnership.Should().BeTrue();
        model.reExCompanyTeamMembers.Should().BeEquivalentTo(teamMembers);
    }

    [TestMethod]
    public async Task CheckYourDetails_WhenCompaniesHouseFlowAndNestedPropertiesNull_SetsViewModelPropertiesToNullOrDefaults()
    {
        // Arrange
        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                Company = null,
                Partnership = null,
                RoleInOrganisation = null,
                TeamMembers = null
            },
            IsOrganisationAPartnership = null
        };

        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();

        // Assert
        var viewResult = (ViewResult)result;
        var model = (ReExCheckYourDetailsViewModel)viewResult.Model!;

        model.IsCompaniesHouseFlow.Should().BeTrue();
        model.BusinessAddress.Should().BeNull();
        model.CompanyName.Should().BeNull();
        model.CompaniesHouseNumber.Should().BeNull();
        model.RoleInOrganisation.Should().BeNull();
        model.IsOrganisationAPartnership.Should().BeFalse();
        model.LimitedPartnershipPartners.Should().BeNull();
        model.IsLimitedLiabilityPartnership.Should().BeFalse();
        model.reExCompanyTeamMembers.Should().BeNull();

        _sessionManagerMock.Verify(m => m.SaveSessionAsync(It.IsAny<ISession>(), session), Times.Once);
    }

    [TestMethod]
    public async Task CheckYourDetails_WhenIsSoleTraderAndManualInputSessionIsNull_SetsPropertiesSafely()
    {
        // Arrange
        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            ReExManualInputSession = null
        };
        session.ReExManualInputSession = null;
        session.ReExManualInputSession = null;

        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();

        // Assert
        var viewResult = (ViewResult)result;
        var model = (ReExCheckYourDetailsViewModel)viewResult.Model!;
        model.BusinessAddress.Should().BeNull();
        model.TradingName.Should().BeNull();

        model.reExCompanyTeamMembers.Should().BeNull();

        _sessionManagerMock.Verify(m => m.SaveSessionAsync(It.IsAny<ISession>(), session), Times.Once);
    }

    [TestMethod]
    public async Task CheckYourDetails_WhenIsSoleTraderAndTeamMembersNull_DoesNotThrowAndAddsNoMembers()
    {
        // Arrange
        var manualInputSession = new ReExManualInputSession
        {
            ProducerType = ProducerType.SoleTrader,
            BusinessAddress = new Address
            {
                Country = "UK",
                BuildingName = "address line 1",
                Street = "address line 2",
                Town = "London",
                County = "London",
                Postcode = "AB1 2BC"
            },
            TeamMembers = null
        };

        var session = new OrganisationSession
        {
            TradingName = "Sole Trader Trading",
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            ReExManualInputSession = manualInputSession
        };

        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();

        // Assert
        var viewResult = (ViewResult)result;
        var model = (ReExCheckYourDetailsViewModel)viewResult.Model!;

        model.ProducerType.Should().Be(ProducerType.SoleTrader);
        model.TradingName.Should().Be("Sole Trader Trading");
        model.reExCompanyTeamMembers.Should().BeEmpty();

        _sessionManagerMock.Verify(m => m.SaveSessionAsync(It.IsAny<ISession>(), session), Times.Once);
    }

    [TestMethod]
    public async Task CheckYourDetails_WhenIsSoleTraderAndHasTeamMembers_AddsFirstMember()
    {
        // Arrange
        var teamMember = new ReExCompanyTeamMember { FirstName = "Member1" };

        var manualInputSession = new ReExManualInputSession
        {
            ProducerType = ProducerType.SoleTrader,
            BusinessAddress = new Address
            {
                Country = "UK",
                BuildingName = "address line 1",
                Street = "address line 2",
                Town = "London",
                County = "London",
                Postcode = "AB1 2BC"
            },
            TeamMembers = new List<ReExCompanyTeamMember> { teamMember }
        };

        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            ReExManualInputSession = manualInputSession,
            TradingName = "Trading",
        };

        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();

        // Assert
        var viewResult = (ViewResult)result;
        var model = (ReExCheckYourDetailsViewModel)viewResult.Model!;

        model.ProducerType.Should().Be(ProducerType.SoleTrader);
        model.TradingName.Should().Be("Trading");

        model.reExCompanyTeamMembers.Should().ContainSingle();
        model.reExCompanyTeamMembers[0].Should().Be(teamMember);

        _sessionManagerMock.Verify(m => m.SaveSessionAsync(It.IsAny<ISession>(), session), Times.Once);
    }

    [TestMethod]
    public async Task CheckYourDetails_WhenIsNonUkAndManualInputSessionIsNull_SetsPropertiesSafely()
    {
        // Arrange
        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            IsUkMainAddress = false,
            ReExManualInputSession = null
        };

        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();

        // Assert
        var viewResult = (ViewResult)result;
        var model = (ReExCheckYourDetailsViewModel)viewResult.Model!;

        model.IsNonUk.Should().BeTrue();

        model.ProducerType.Should().BeNull();
        model.BusinessAddress.Should().BeNull();
        model.TradingName.Should().BeNull();
        model.reExCompanyTeamMembers.Should().BeNull();
        model.Nation.Should().BeNull();

        _sessionManagerMock.Verify(m => m.SaveSessionAsync(It.IsAny<ISession>(), session), Times.Once);
    }

    [TestMethod]
    public async Task CheckYourDetails_WhenIsNonUkAndManualInputSessionPropertiesAreSet_AssignsPropertiesCorrectly()
    {
        // Arrange
        var manualInputSession = new ReExManualInputSession
        {
            ProducerType = ProducerType.NonUkOrganisation,
            BusinessAddress = new Address
            {
                Country = "France",
                BuildingName = "address line 1",
                Street = "address line 2",
                Town = "Paris",
                County = "Île-de-France",
                Postcode = "75001",
                IsManualAddress = true
            },
            OrganisationName = "NonUk Org",
            TeamMembers = new List<ReExCompanyTeamMember> { new ReExCompanyTeamMember { FirstName = "Team Member 1" } },
            UkRegulatorNation = Nation.Wales
        };

        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            IsUkMainAddress = false,
            ReExManualInputSession = manualInputSession,
            IsTradingNameDifferent = true,
            TradingName = "NonUK Trade"
        };

        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.CheckYourDetails();

        // Assert
        var viewResult = (ViewResult)result;
        var model = (ReExCheckYourDetailsViewModel)viewResult.Model!;

        model.IsNonUk.Should().BeTrue();
        model.ProducerType.Should().Be(ProducerType.NonUkOrganisation);
        model.CompanyName.Should().Be("NonUk Org");
        model.TradingName.Should().Be("NonUK Trade");
        model.reExCompanyTeamMembers.Should().ContainSingle();
        model.Nation.Should().Be(Nation.Wales);

        _sessionManagerMock.Verify(m => m.SaveSessionAsync(It.IsAny<ISession>(), session), Times.Once);
    }

}