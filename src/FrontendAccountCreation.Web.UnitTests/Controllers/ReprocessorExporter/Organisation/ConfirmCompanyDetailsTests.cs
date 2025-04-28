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
            }
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
    }

    [TestMethod]
    public async Task GivenCompaniesHouseNumber_WhenConfirmDetailsOfTheCompanyCalled_ThenRedirectToUkNation()
    {
        // Arrange
        _companyServiceMock.Setup(x => x.IsComplianceScheme(It.IsAny<string>())).Returns(false);

        // Act
        var result = await _systemUnderTest.ConfirmDetailsOfTheCompany();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.UkNation));
    }

    [TestMethod]
    public async Task GivenCompaniesHouseNumber_WhenConfirmDetailsOfTheCompanyCalled_ThenUpdateSession()
    {
        // Arrange
        _companyServiceMock.Setup(x => x.IsComplianceScheme(It.IsAny<string>())).Returns(false);

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
