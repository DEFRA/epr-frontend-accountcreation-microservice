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
		// Arrange
		var session = new OrganisationSession
		{
			ReExCompaniesHouseSession = new ReExCompaniesHouseSession
			{
				Company = new Company { Name = "Test Ltd" },
				TeamMembers =
                [
                    new ReExCompanyTeamMember { FirstName = "Alice" }
				]
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
		viewModel.ReExCompanyTeamMembers.Should().HaveCount(1);
		viewModel.ReExCompanyTeamMembers![0].FirstName.Should().Be("Alice");
	}

	[TestMethod]
	public async Task GET_Success_WithCompaniesHouseSession_ReturnsCorrectViewAndModel()
	{
		// Arrange
		var session = new OrganisationSession
		{
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
			ReExManualInputSession = new ReExManualInputSession
			{
				ProducerType = ProducerType.SoleTrader,
				TradingName = "Sole Trader Ltd",
				TeamMember = new ReExCompanyTeamMember { FirstName = "Bob" }
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
			ReExManualInputSession = new ReExManualInputSession
			{
				ProducerType = ProducerType.SoleTrader,
				TradingName = "Solo Ltd",
				TeamMember = null
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
}