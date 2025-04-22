using FluentAssertions;
using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Services.Dto.Company;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.AccountCreation;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using global::FrontendAccountCreation.Core.Addresses;
using global::FrontendAccountCreation.Core.Services.Dto.Company;
using global::FrontendAccountCreation.Core.Sessions;
using global::FrontendAccountCreation.Web.Constants;
using global::FrontendAccountCreation.Web.Controllers.AccountCreation;
using global::FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter;
using global::FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class CompaniesHouseNumberTests : OrganisationTestBase
{
    private OrganisationSession _accountCreationSessionMock = null!;

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
            Postcode = "AB1 0CD",
        }
    };

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _accountCreationSessionMock = new OrganisationSession
        {
            Journey = new List<string> { PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber },
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                Company = _companyMock
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_accountCreationSessionMock);
        _facadeServiceMock.Setup(x => x.GetCompanyByCompaniesHouseNumberAsync(It.IsAny<string>())).ReturnsAsync(_companyMock);
    }

    [TestMethod]
    public async Task CompaniesHouseNumber_CompaniesHouseNumberIsGiven_RedirectsToConfirmCompanyDetailsAndUpdateSession()
    {
        // Arrange
        var request = new CompaniesHouseNumberViewModel { CompaniesHouseNumber = "1234" };

        // Act
        var result = await _systemUnderTest.CompaniesHouseNumber(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.ConfirmCompanyDetails));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
        _facadeServiceMock.Verify(x => x.GetCompanyByCompaniesHouseNumberAsync(It.IsAny<string>()), Times.Once);

    }

    [TestMethod]
    public async Task CompaniesHouseNumber_CompaniesHouseNumberIsGivenButCompaniesHouseApiDown_RedirectsToCannotVerifyOrganisationPage()
    {
        // Arrange
        _facadeServiceMock.Setup(x => x.GetCompanyByCompaniesHouseNumberAsync(It.IsAny<string>())).Throws(new Exception());
        var request = new CompaniesHouseNumberViewModel { CompaniesHouseNumber = "x" };

        // Act
        var result = await _systemUnderTest.CompaniesHouseNumber(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.CannotVerifyOrganisation));
        _facadeServiceMock.Verify(x => x.GetCompanyByCompaniesHouseNumberAsync(It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task CompaniesHouseNumber_NoCompaniesHouseNumberIsGiven_ReturnsViewWithErrorAndBackLinkIsRegisteredWithCompaniesHouse()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(CompaniesHouseNumberViewModel.CompaniesHouseNumber), "Field is required");

        // Act
        var result = await _systemUnderTest.CompaniesHouseNumber(new CompaniesHouseNumberViewModel());

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<CompaniesHouseNumberViewModel>();

        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<OrganisationSession>>()), Times.Never);
        AssertBackLink(viewResult, PagePath.RegisteredWithCompaniesHouse);
    }

    [TestMethod]
    public async Task CompaniesHouseNumber_NoCompanyInformationWasFound_RedirectsToCompaniesHouseNumberPage()
    {
        // Arrange
        _facadeServiceMock
            .Setup(x => x.GetCompanyByCompaniesHouseNumberAsync(It.IsAny<string>()))
            .ReturnsAsync((Company?)null);

        // Act
        var result = await _systemUnderTest.CompaniesHouseNumber(new CompaniesHouseNumberViewModel());

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.CompaniesHouseNumber));
        _facadeServiceMock.Verify(x => x.GetCompanyByCompaniesHouseNumberAsync(It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task CompaniesHouseNumber_CompaniesHouseNumberPageIsExited_BackLinkIsRegisteredWithCompaniesHouse()
    {
        //Act
        var result = await _systemUnderTest.CompaniesHouseNumber();

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<CompaniesHouseNumberViewModel>();
        AssertBackLink(viewResult, PagePath.RegisteredWithCompaniesHouse);
    }

    [TestMethod]
    public async Task UserNavigatesToCompaniesHouseNumberPage_FromCheckYourDetailsPage_BackLinkShouldBeCheckYourDetails()
    {
        //Arrange
        var accountCreationSessionMock = new OrganisationSession
        {
            Journey = new List<string>
            {
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.TypeOfOrganisation,PagePath.TradingName,
                PagePath.CompaniesHouseNumber, PagePath.RoleInOrganisation, PagePath.FullName, PagePath.TelephoneNumber,
                PagePath.CheckYourDetails
            },
            IsUserChangingDetails = true,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(accountCreationSessionMock);

        //Act
        var result = await _systemUnderTest.CompaniesHouseNumber();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<CompaniesHouseNumberViewModel>();
        AssertBackLink(viewResult, PagePath.CheckYourDetails);

    }

    [TestMethod]
    public async Task CompaniesHouseNumber_CompaniesHouseNumberPage_HasBeenRedirectedToBecauseOfValidationError()
    {
        //Arrange
        _tempDataDictionaryMock.Setup(dictionary => dictionary["ModelState"]).Returns("{\"Errors\":[\"one\",\"two\"]}");
        _tempDataDictionaryMock.Setup(dictionary => dictionary["CompaniesHouseNumber"]).Returns("123456");
        _systemUnderTest.TempData = _tempDataDictionaryMock.Object;

        //Act
        var result = await _systemUnderTest.CompaniesHouseNumber();

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<CompaniesHouseNumberViewModel>();
        var viewModel = (CompaniesHouseNumberViewModel)viewResult.Model;
        viewModel.CompaniesHouseNumber.Should().Be("123456");
    }
}

