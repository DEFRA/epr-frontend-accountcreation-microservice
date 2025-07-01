using FluentAssertions;
using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Services.Dto.Company;
using FrontendAccountCreation.Core.Services.FacadeModels;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class SuccessTests : OrganisationTestBase
{
	[TestInitialize]
	public void Setup()
	{
		SetupBase();
	}

	[TestMethod]
	public async Task GET_DeclarationContinue_RedirectsToSuccess()
	{
        // Arrange
        var orgSession = new OrganisationSession
        {
            Journey = [PagePath.Declaration, PagePath.DeclarationContinue],
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                Company = new Core.Services.Dto.Company.Company
                {
                    AccountCreatedOn = DateTime.Now,
                    Name = "ReEx Test Ltd",
                    CompaniesHouseNumber = "12345678",
                    OrganisationId = "06352abc-bb77-4855-9705-cf06ae88f5a8",
                    BusinessAddress = new Address
                    {
                        BuildingName = "ReEx House",
                        BuildingNumber = "14",
                        Street = "High street",
                        Town = "Lodnon",
                        Postcode = "E10 6PN",
                        Locality = "XYZ",
                        DependentLocality = "ABC",
                        County = "London",
                        Country = "England"
                    }
                }
            },
            UkNation = Nation.England
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(orgSession);

        var mapperObj = new ReExOrganisationModel
        {
            Company = new ReExCompanyModel()
            {
                CompanyName = "ReEx Test Ltd",
                CompaniesHouseNumber = "12345678",
                OrganisationId = "06352abc-bb77-4855-9705-cf06ae88f5a8",
                CompanyRegisteredAddress = new AddressModel
                {
                    BuildingName = "ReEx House",
                    BuildingNumber = "14",
                    Street = "High street",
                    Town = "Lodnon",
                    Postcode = "E10 6PN",
                    Locality = "XYZ",
                    DependentLocality = "ABC",
                    County = "London",
                    Country = "England"
                },
                Nation = Nation.England,
                OrganisationType = OrganisationType.CompaniesHouseCompany,
                ValidatedWithCompaniesHouse = true
            }
        };

        _reExAccountMapperMock.Setup(x => x.CreateReExOrganisationModel(orgSession))
            .Returns(mapperObj);       

		// Act
		var result = await _systemUnderTest.DeclarationContinue();        

        // Assert
        result.Should().NotBeNull();
		result.Should().BeOfType<RedirectToActionResult>();
		var redirectResult = (RedirectToActionResult)result;
		redirectResult.ActionName.Should().Be(nameof(OrganisationController.Success));
	}

	[TestMethod]
	public async Task GET_Success_ReturnsCorrectViewAndModel()
	{
		var session = new OrganisationSession
		{
			OrganisationType = OrganisationType.CompaniesHouseCompany,
			ReExCompaniesHouseSession = new ReExCompaniesHouseSession
			{
				Company = new Company { Name = "Test Ltd" },
				TeamMembers = new List<ReExCompanyTeamMember>
			{
				new ReExCompanyTeamMember { FirstName = "Alice" }
			}
				}
			};

		// Mock the session manager to return a session with IsCompaniesHouseFlow = true
		_sessionManagerMock
			.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
			.ReturnsAsync(session);

		// Optionally, you can also mock other session properties, if needed:
		_sessionManagerMock
			.Setup(x => x.SaveSessionAsync(It.IsAny<ISession>(), session))
			.Returns(Task.CompletedTask);

		// Act
		var result = await _systemUnderTest.Success();

		// Assert
		result.Should().BeOfType<ViewResult>();
		var viewResult = (ViewResult)result;
		viewResult.Model.Should().BeOfType<ReExOrganisationSuccessViewModel>();
		var viewModel = (ReExOrganisationSuccessViewModel)viewResult.Model!;

		viewModel.CompanyName.Should().Be("Test Ltd");
		viewModel.ReExCompanyTeamMembers.Should().HaveCount(1);
		viewModel.ReExCompanyTeamMembers[0].FirstName.Should().Be("Alice");
	}



	[TestMethod]
	public async Task GET_Success_WithCompaniesHouseSession_ReturnsCorrectViewAndModel()
	{
		// Arrange
		var session = new OrganisationSession
		{
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
			{
				Company = new Company { Name = "Test Ltd" },
				TeamMembers = new List<ReExCompanyTeamMember>
				{
					new ReExCompanyTeamMember { FirstName = "Alice" }
				}
			}
		};

		_sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

		// Act
		var result = await _systemUnderTest.Success();

		// Assert
		result.Should().BeOfType<ViewResult>();
		var viewResult = (ViewResult)result;
		viewResult.Model.Should().BeOfType<ReExOrganisationSuccessViewModel>();

		var viewModel = (ReExOrganisationSuccessViewModel)viewResult.Model!;
		viewModel.CompanyName.Should().Be("Test Ltd");
		viewModel.ReExCompanyTeamMembers.Should().NotBeNull();
		viewModel.ReExCompanyTeamMembers.Should().HaveCount(1);
		viewModel.ReExCompanyTeamMembers![0].FirstName.Should().Be("Alice");
		viewModel.IsSoleTrader.Should().BeFalse();
	}

	[TestMethod]
	public async Task GET_Success_WithSoleTraderSession_ReturnsCorrectViewAndModel()
	{
		// Arrange
		var session = new OrganisationSession
		{
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            ReExManualInputSession = new ReExManualInputSession
			{
				ProducerType = ProducerType.SoleTrader,
				OrganisationName = "Sole Trader Ltd",
				TeamMembers = new List<ReExCompanyTeamMember>
				   {
					   new ReExCompanyTeamMember
					   {
						   Id = Guid.NewGuid(),
						   FirstName = "Bob"
					   }
				   }
			}
		};

		_sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

		// Act
		var result = await _systemUnderTest.Success();

		// Assert
		result.Should().BeOfType<ViewResult>();
		var viewResult = (ViewResult)result;
		viewResult.Model.Should().BeOfType<ReExOrganisationSuccessViewModel>();

		var viewModel = (ReExOrganisationSuccessViewModel)viewResult.Model!;
		viewModel.CompanyName.Should().Be("Sole Trader Ltd");
		viewModel.ReExCompanyTeamMembers.Should().NotBeNull();
		viewModel.ReExCompanyTeamMembers.Should().HaveCount(1);
		viewModel.ReExCompanyTeamMembers![0].FirstName.Should().Be("Bob");
		viewModel.IsSoleTrader.Should().BeTrue();
	}

	[TestMethod]
	public async Task GET_Success_WithSoleTraderSession_NoTeamMember_ReturnsViewModelWithoutTeamMembers()
	{
		// Arrange
		var session = new OrganisationSession
		{
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            ReExManualInputSession = new ReExManualInputSession
			{
				ProducerType = ProducerType.SoleTrader,
				OrganisationName = "Solo Ltd",
				TeamMembers = null
			}
		};

		_sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

		// Act
		var result = await _systemUnderTest.Success();

		// Assert
		result.Should().BeOfType<ViewResult>();
		var viewResult = (ViewResult)result;
		viewResult.Model.Should().BeOfType<ReExOrganisationSuccessViewModel>();

		var viewModel = (ReExOrganisationSuccessViewModel)viewResult.Model!;
		viewModel.CompanyName.Should().Be("Solo Ltd");
		viewModel.ReExCompanyTeamMembers.Should().BeNullOrEmpty();
		viewModel.IsSoleTrader.Should().BeTrue();
	}

	[TestMethod]
	public async Task GET_Success_WithNullSessionProperties_ReturnsViewModelWithDefaults()
	{
		// Arrange
		var session = new OrganisationSession
		{
			ReExManualInputSession = null,
			ReExCompaniesHouseSession = null
		};

		_sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

		// Act
		var result = await _systemUnderTest.Success();

		// Assert
		result.Should().BeOfType<ViewResult>();
		var viewResult = (ViewResult)result;
		viewResult.Model.Should().BeOfType<ReExOrganisationSuccessViewModel>();

		var viewModel = (ReExOrganisationSuccessViewModel)viewResult.Model!;
		viewModel.CompanyName.Should().BeNull();
		viewModel.ReExCompanyTeamMembers.Should().BeNull();
		viewModel.IsSoleTrader.Should().BeFalse();
	}

    [TestMethod]
    public async Task Success_WhenIsCompaniesHouseFlow_PopulatesCompanyDetails()
    {
        // Arrange
        var expectedCompanyName = "Companies House Ltd";
        var teamMembers = new List<ReExCompanyTeamMember>
    {
        new ReExCompanyTeamMember { FirstName = "Alice", LastName = "Smith" }
    };

        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                Company = new Company
                {
                    Name = expectedCompanyName
                },
                TeamMembers = teamMembers
            }
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.Success();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ReExOrganisationSuccessViewModel>().Subject;

        model.IsCompaniesHouseFlow.Should().BeTrue();
        model.CompanyName.Should().Be(expectedCompanyName);
        model.ReExCompanyTeamMembers.Should().BeEquivalentTo(teamMembers);
    }

    [TestMethod]
    public async Task Success_WhenSoleTrader_PopulatesCompanyNameWithOrganisationName()
    {
        // Arrange
        var expectedOrganisationName = "Sole Trader Ltd";
        var teamMembers = new List<ReExCompanyTeamMember>
    {
        new ReExCompanyTeamMember { FirstName = "Bob" }
    };

        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            ReExManualInputSession = new ReExManualInputSession
            {
                ProducerType = ProducerType.SoleTrader,
                OrganisationName = expectedOrganisationName,
                TeamMembers = teamMembers
            }
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.Success();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var viewModel = viewResult.Model.Should().BeOfType<ReExOrganisationSuccessViewModel>().Subject;

        viewModel.IsCompaniesHouseFlow.Should().BeFalse();
        viewModel.IsSoleTrader.Should().BeTrue();
        viewModel.CompanyName.Should().Be(expectedOrganisationName);
        viewModel.ReExCompanyTeamMembers.Should().BeEquivalentTo(teamMembers);
    }

    [TestMethod]
    public async Task Success_WhenNonUk_PopulatesCompanyNameWithNonUkOrganisationName()
    {
        // Arrange
        var expectedName = "Non-UK Org Ltd";
        var teamMembers = new List<ReExCompanyTeamMember>
    {
        new ReExCompanyTeamMember { FirstName = "Alice" }
    };

        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            ReExManualInputSession = new ReExManualInputSession
            {
                ProducerType = ProducerType.NonUkOrganisation,
                OrganisationName = expectedName,
                TeamMembers = teamMembers
            }
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.Success();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var viewModel = viewResult.Model.Should().BeOfType<ReExOrganisationSuccessViewModel>().Subject;

        viewModel.IsCompaniesHouseFlow.Should().BeFalse();
        viewModel.IsSoleTrader.Should().BeFalse();
        viewModel.CompanyName.Should().Be(expectedName);
        viewModel.ReExCompanyTeamMembers.Should().BeEquivalentTo(teamMembers);
    }

    [TestMethod]
    public async Task Success_WhenCompaniesHouseFlowIsTrue_ButCompanyOrTeamMembersAreNull_DoesNotThrowAndSetsNulls()
    {
        // Arrange
        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                Company = null,
                TeamMembers = null
            }
        };

        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.Success();

        // Assert
        var viewResult = (ViewResult)result;
        var viewModel = (ReExOrganisationSuccessViewModel)viewResult.Model!;

        viewModel.IsCompaniesHouseFlow.Should().BeTrue();
        viewModel.CompanyName.Should().BeNull();
        viewModel.ReExCompanyTeamMembers.Should().BeNull();
    }

    [TestMethod]
    public async Task Success_WhenCompaniesHouseFlowIsFalse_AndManualInputIsNull_CompanyNameIsNull()
    {
        // Arrange
        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            ReExManualInputSession = null
        };

        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.Success();

        // Assert
        var viewResult = (ViewResult)result;
        var viewModel = (ReExOrganisationSuccessViewModel)viewResult.Model!;

        viewModel.IsCompaniesHouseFlow.Should().BeFalse();
        viewModel.CompanyName.Should().BeNull();
    }

    [TestMethod]
    public async Task Success_WhenCompaniesHouseFlowIsFalse_AndManualInputNamesAreNull_CompanyNameIsNull()
    {
        // Arrange
        var manualInput = new ReExManualInputSession
        {
            ProducerType = ProducerType.SoleTrader,
            OrganisationName = null
        };

        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            TradingName = null,
            ReExManualInputSession = manualInput
        };

        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.Success();

        // Assert
        var viewResult = (ViewResult)result;
        var viewModel = (ReExOrganisationSuccessViewModel)viewResult.Model!;

        viewModel.IsCompaniesHouseFlow.Should().BeFalse();
        viewModel.IsSoleTrader.Should().BeTrue();
        viewModel.CompanyName.Should().BeNull();
    }


}