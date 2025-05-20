using FluentAssertions;
using FrontendAccountCreation.Core.Services.Dto.Company;
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
		var session = new OrganisationSession();
		_sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

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
		viewModel.reExCompanyTeamMembers.Should().HaveCount(1);
		viewModel.reExCompanyTeamMembers!.First().FirstName.Should().Be("Alice");
	}
}