using FluentAssertions;
using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Services.Dto.Company;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.AccountCreation;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Data;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class ConfirmCompanyDetailsTests : OrganisationTestBase
{
    private OrganisationSession? _organisationSessionMock;
    private readonly Company _company = new()
    {
        CompaniesHouseNumber = "123",
        Name = "Dummy Company",
        BusinessAddress = new Address
        {
            BuildingNumber = "15",
            BuildingName = "Navarino Mansions",
            Street = "Gracefield Gardens",
            Town = "Nowhere",
            County = "Some County",
            Postcode = "AB1 0CD"
        }
    };

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _organisationSessionMock = new OrganisationSession
        {
            Journey =
            [
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber,
                PagePath.ConfirmCompanyDetails
            ],
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                Company = _company
            }, OrganisationType = Core.Sessions.OrganisationType.CompaniesHouseCompany
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_organisationSessionMock);
        _facadeServiceMock.Setup(x => x.GetCompanyByCompaniesHouseNumberAsync(It.IsAny<string>())).ReturnsAsync(_company);
    }

    [TestMethod]
    public async Task GivenCompaniesHouseNumber_WhenConfirmCompanyDetailsCalled_ThenConfirmCompanyDetailsViewReturned()
    {
        // Act
        var result = await _systemUnderTest.ConfirmCompanyDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        ((ViewResult)result).Model.Should().BeOfType<ConfirmCompanyDetailsViewModel>();

        var gotViewModel = (ConfirmCompanyDetailsViewModel)((ViewResult)result).Model!;

        gotViewModel.CompaniesHouseNumber.Should().Be(_company.CompaniesHouseNumber);
        gotViewModel.CompanyName.Should().Be(_company.Name);
        gotViewModel.BusinessAddress?.BuildingNumber.Should().Be(_company.BusinessAddress.BuildingNumber);
        gotViewModel.BusinessAddress?.BuildingName.Should().Be(_company.BusinessAddress.BuildingName);
        gotViewModel.BusinessAddress?.Street.Should().Be(_company.BusinessAddress.Street);
        gotViewModel.BusinessAddress?.Town.Should().Be(_company.BusinessAddress.Town);
        gotViewModel.BusinessAddress?.County.Should().Be(_company.BusinessAddress.County);
        gotViewModel.BusinessAddress?.Postcode.Should().Be(_company.BusinessAddress.Postcode);
        gotViewModel.BusinessAddress?.Country.Should().Be(_company.BusinessAddress.Country);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("Great Britan")]
    [DataRow("Not specified")]
    [DataRow("United Kingdom")]
    public async Task GivenCompaniesHouseNumber_WhenConfirmDetailsOfTheCompanyCalled_AndHaveInvalidCountry_ThenRedirectToUkNation(string inValidCountry)
    {
        // Arrange
        _organisationSessionMock.ReExCompaniesHouseSession.Company.BusinessAddress.Country = inValidCountry;

        // Act
        var result = await _systemUnderTest.ConfirmDetailsOfTheCompany();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.UkNation));
    }

    [TestMethod]
    [DataRow("Wales")]
    [DataRow("England")]
    [DataRow("Scotland")]
    [DataRow("Northern Ireland")]
    public async Task GivenCompaniesHouseNumber_WhenConfirmDetailsOfTheCompanyCalled_AndHaveValidCountry_ThenRedirectToIsTradingNameDifferentCheckPage(string validCountry)
    {
        // Arrange
        _organisationSessionMock.ReExCompaniesHouseSession.Company.BusinessAddress.Country = validCountry;
        // Act
        var result = await _systemUnderTest.ConfirmDetailsOfTheCompany();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((result as RedirectToActionResult)!).ActionName.Should().Be(nameof(OrganisationController.IsTradingNameDifferent));
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenCompaniesHouseNumber_WhenConfirmDetailsOfTheCompanyCalled_ThenUpdateSession()
    {
        // Arrange

        // Act
        var result = await _systemUnderTest.ConfirmDetailsOfTheCompany();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenAccountAlreadyExists_WhenConfirmDetailsOfTheCompanyCalled_ThenRedirectsToAccountAlreadyExistsPage()
    {
        _organisationSessionMock.ReExCompaniesHouseSession.Company.AccountCreatedOn = DateTime.Now;

        // Act
        var result = await _systemUnderTest.ConfirmDetailsOfTheCompany();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.AccountAlreadyExists));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenFinishedPreviousPage_WhenConfirmCompanyDetailsCalled_ThenConfirmCompanyDetailsPageReturned_WithCompaniesHouseNumberAsTheBackLink()
    {
        //Act
        var result = await _systemUnderTest.ConfirmCompanyDetails();

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<ConfirmCompanyDetailsViewModel>();
        AssertBackLink(viewResult, PagePath.CompaniesHouseNumber);
    }
}
