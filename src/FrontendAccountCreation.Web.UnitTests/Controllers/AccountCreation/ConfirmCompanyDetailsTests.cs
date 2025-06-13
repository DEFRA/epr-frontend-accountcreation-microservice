using FluentAssertions;
using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Services.Dto.Company;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.AccountCreation;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.AccountCreation;

[TestClass]
public class ConfirmCompanyDetailsTests : AccountCreationTestBase
{
    private AccountCreationSession _accountCreationSessionMock = null!;
    private readonly Company _companyMock = new Company
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

        _accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string> { PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber, PagePath.ConfirmCompanyDetails },
            CompaniesHouseSession = new CompaniesHouseSession
            {
                Company = _companyMock
            }
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_accountCreationSessionMock);
        _facadeServiceMock.Setup(x => x.GetCompanyByCompaniesHouseNumberAsync(It.IsAny<string>())).ReturnsAsync(_companyMock);
        _companyServiceMock.Setup(x => x.IsComplianceScheme(It.IsAny<string>())).Returns(true);
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

        gotViewModel.CompaniesHouseNumber.Should().Be(_companyMock.CompaniesHouseNumber);
        gotViewModel.CompanyName.Should().Be(_companyMock.Name);
        gotViewModel.BusinessAddress?.BuildingNumber.Should().Be(_companyMock.BusinessAddress.BuildingNumber);
        gotViewModel.BusinessAddress?.BuildingName.Should().Be(_companyMock.BusinessAddress.BuildingName);
        gotViewModel.BusinessAddress?.Street.Should().Be(_companyMock.BusinessAddress.Street);
        gotViewModel.BusinessAddress?.Town.Should().Be(_companyMock.BusinessAddress.Town);
        gotViewModel.BusinessAddress?.County.Should().Be(_companyMock.BusinessAddress.County);
        gotViewModel.BusinessAddress?.Postcode.Should().Be(_companyMock.BusinessAddress.Postcode);
    }

    [TestMethod]
    public async Task GivenCompaniesHouseNumberAndComplianceScheme_WhenConfirmDetailsOfTheCompanyCalled_ThenRedirectToRoleInOrganisation_AndUpdateSession()
    {
        // Act
        var result = await _systemUnderTest.ConfirmDetailsOfTheCompany();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.RoleInOrganisation));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenCompaniesHouseNumberAndNotComplianceScheme_WhenConfirmDetailsOfTheCompanyCalled_ThenRedirectToUkNation_AndUpdateSession()
    {
        // Arrange
        _companyServiceMock.Setup(x => x.IsComplianceScheme(It.IsAny<string>())).Returns(false);

        // Act
        var result = await _systemUnderTest.ConfirmDetailsOfTheCompany();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.UkNation));
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenAccountAlreadyExists_WhenConfirmDetailsOfTheCompanyCalled_ThenRedirectsToAccountAlreadyExistsPage()
    {
        if(_accountCreationSessionMock.CompaniesHouseSession != null)
        {
            _accountCreationSessionMock.CompaniesHouseSession.Company.AccountCreatedOn = DateTime.Now;
        }

        // Act
        var result = await _systemUnderTest.ConfirmDetailsOfTheCompany();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.AccountAlreadyExists));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenFinishedPreviousPage_WhenConfirmCompanyDetailsCalled_ThenConfirmCompanyDetailsPageReturned_WithTCompaniesHouseNumberAsTheBackLink()
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
