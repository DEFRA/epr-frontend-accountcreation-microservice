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
		viewModel.reExCompanyTeamMembers.Should().HaveCount(1);
		viewModel.reExCompanyTeamMembers![0].FirstName.Should().Be("Alice");
	}
}