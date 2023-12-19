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
public class CheckYourDetailsTests : AccountCreationTestBase
{
    private AccountCreationSession _accountCreationSessionMock = null!;
    private readonly string _tradingNameMock = "Adam Smith Plumbing Services";
    private readonly Company _companyMock = new Company
    {
        CompaniesHouseNumber = "123",
        Name = "Imaginary Company",
        BusinessAddress = new Address
        {
            BuildingNumber = "15",
            BuildingName = "Navarino Mansions",
            Street = "Gracefield Gardens",
            Town = "Nowhere Town",
            County = "Some County",
            Postcode = "AB1 2CD"
        }
    };

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string> {
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber, PagePath.ConfirmCompanyDetails,
                PagePath.RoleInOrganisation, PagePath.FullName, PagePath.TelephoneNumber, PagePath.CheckYourDetails
            },
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            CompaniesHouseSession = new CompaniesHouseSession { Company = _companyMock }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_accountCreationSessionMock);
    }

    [TestMethod]
    public async Task CheckYourDetails_OrganisationIsRegistered_CheckYourDetailsViewModelForCompanyIsReturned()
    {
        // Arrange
        _accountCreationSessionMock.OrganisationType = OrganisationType.CompaniesHouseCompany;
        _accountCreationSessionMock.UkNation = Nation.NorthernIreland;

        // Act
        var result = await _systemUnderTest.CheckYourDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        ((ViewResult)result).Model.Should().BeOfType<CheckYourDetailsViewModel>();

        var viewModel = (CheckYourDetailsViewModel)((ViewResult)result).Model!;

        viewModel.CompanyName.Should().Be(_companyMock.Name);
        viewModel.CompaniesHouseNumber.Should().Be(_companyMock.CompaniesHouseNumber);
        viewModel.BusinessAddress?.BuildingNumber.Should().Be(_companyMock.BusinessAddress.BuildingNumber);
        viewModel.BusinessAddress?.BuildingName.Should().Be(_companyMock.BusinessAddress.BuildingName);
        viewModel.BusinessAddress?.Street.Should().Be(_companyMock.BusinessAddress.Street);
        viewModel.BusinessAddress?.Town.Should().Be(_companyMock.BusinessAddress.Town);
        viewModel.BusinessAddress?.County.Should().Be(_companyMock.BusinessAddress.County);
        viewModel.BusinessAddress?.Postcode.Should().Be(_companyMock.BusinessAddress.Postcode);
        viewModel.Nation?.Should().Be(_accountCreationSessionMock.UkNation);
    }

    [TestMethod]
    public async Task CheckYourDetails_OrganisationIsNotRegistered_CheckYourDetailsViewModelForTradingNameIsReturned()
    {
        // Arrange
        _accountCreationSessionMock.OrganisationType = OrganisationType.NonCompaniesHouseCompany;
        _accountCreationSessionMock.UkNation = Nation.NorthernIreland;
        _accountCreationSessionMock.ManualInputSession = new ManualInputSession()
        {
            TradingName = _tradingNameMock, BusinessAddress = _companyMock.BusinessAddress,
            ProducerType = ProducerType.SoleTrader
    };

        // Act
        var result = await _systemUnderTest.CheckYourDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        ((ViewResult)result).Model.Should().BeOfType<CheckYourDetailsViewModel>();

        var viewModel = (CheckYourDetailsViewModel)((ViewResult)result).Model!;

        viewModel.TradingName.Should().Be(_tradingNameMock);
        viewModel.BusinessAddress?.BuildingNumber.Should().Be(_companyMock.BusinessAddress.BuildingNumber);
        viewModel.BusinessAddress?.BuildingName.Should().Be(_companyMock.BusinessAddress.BuildingName);
        viewModel.BusinessAddress?.Street.Should().Be(_companyMock.BusinessAddress.Street);
        viewModel.BusinessAddress?.Town.Should().Be(_companyMock.BusinessAddress.Town);
        viewModel.BusinessAddress?.County.Should().Be(_companyMock.BusinessAddress.County);
        viewModel.BusinessAddress?.Postcode.Should().Be(_companyMock.BusinessAddress.Postcode);
        viewModel.Nation?.Should().Be(_accountCreationSessionMock.UkNation);
    }

    [TestMethod]
    public async Task ConfirmYourDetails_ConfirmYourDetailsHttpPostIsCalled_RedirectsToDeclarationPageAndUpdateSession()
    {
        // Act
        var result = await _systemUnderTest.ConfirmYourDetails();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.Declaration));
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task CheckYourDetails_CheckYourDetailsPageIsExited_BackLinkIsTelephoneNumber()
    {
        //Act
        var result = await _systemUnderTest.CheckYourDetails();

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<CheckYourDetailsViewModel>();
        AssertBackLink(viewResult, PagePath.TelephoneNumber);
    }
}
