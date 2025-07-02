using FluentAssertions;
using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class AddressOverseasTests : OrganisationTestBase
{
    private OrganisationSession _organisationSession = null!;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _organisationSession = new OrganisationSession
        {
            Journey =
            [
                PagePath.IsTradingNameDifferent, PagePath.AddressOverseas
            ],
            ReExManualInputSession = new ReExManualInputSession()
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_organisationSession);
    }

    [TestMethod]
    public async Task GET_WhenBusinessAddressIsNotInSession_ThenViewIsReturnedWithoutDetails()
    {
        // Act
        var result = await _systemUnderTest.AddressOverseas();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<BusinessAddressOverseasViewModel>();
        var viewModel = (BusinessAddressOverseasViewModel?)viewResult.Model;
        viewModel!.Country.Should().BeNull();
        viewModel.AddressLine1.Should().BeNull();
        viewModel.AddressLine2.Should().BeNull();
        viewModel.TownOrCity.Should().BeNull();
        viewModel.StateProvinceRegion.Should().BeNull();
        viewModel.Postcode.Should().BeNull();
    }

    [TestMethod]
    public async Task GET_WhenBusinessAddressIsInSession_ThenViewIsReturnedWithBusinessAddress()
    {
        // Arrange
        _organisationSession.ReExManualInputSession = new ReExManualInputSession
        {
            BusinessAddress = new Address
            {
                Country = "France",
                BuildingName = "address line 1",
                Street = "address line 2",
                Town = "Paris",
                County = "Île-de-France",
                Postcode = "75001",
                IsManualAddress = true
            }
        };

        // Act
        var result = await _systemUnderTest.AddressOverseas();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<BusinessAddressOverseasViewModel>();
        var viewModel = (BusinessAddressOverseasViewModel?)viewResult.Model;
        viewModel!.Country.Should().Be("France");
        viewModel.AddressLine1.Should().Be("address line 1");
        viewModel.AddressLine2.Should().Be("address line 2");
        viewModel.TownOrCity.Should().Be("Paris");
        viewModel.StateProvinceRegion.Should().Be("Île-de-France");
        viewModel.Postcode.Should().Be("75001");
    }

    [TestMethod]
    public async Task GET_ThenBackLinkIsCorrect()
    {
        // Act
        var result = await _systemUnderTest.AddressOverseas();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        AssertBackLink(viewResult, PagePath.IsTradingNameDifferent);
    }

    [TestMethod]
    public async Task POST_GivenValidBusinessAddressOverseas_ThenRedirectToNextPage()
    {
        // Arrange
        var request = new BusinessAddressOverseasViewModel
        {
            Country = "Germany",
            AddressLine1 = "123 Test Street",
            TownOrCity = "Berlin"
        };

        // Act
        var result = await _systemUnderTest.AddressOverseas(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToPageResult)result;
        redirectResult.PageName.Should().Be("/Organisation/UkRegulator");
    }

    [TestMethod]
    public async Task POST_GivenValidBusinessAddressOverseas_ThenUpdatesSession()
    {
        // Arrange
        var request = new BusinessAddressOverseasViewModel
        {
            Country = "Germany",
            AddressLine1 = "123 Test Street",
            AddressLine2 = "Building B",
            TownOrCity = "Berlin",
            StateProvinceRegion = "Brandenburg",
            Postcode = "10115"
        };

        // Act
        await _systemUnderTest.AddressOverseas(request);

        // Assert
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);

        var address = _organisationSession.ReExManualInputSession!.BusinessAddress;
        address.Should().NotBeNull();
        address!.Country.Should().Be("Germany");
        address.BuildingName.Should().Be("123 Test Street");
        address.Street.Should().Be("Building B");
        address.Town.Should().Be("Berlin");
        address.County.Should().Be("Brandenburg");
        address.Postcode.Should().Be("10115");
        address.IsManualAddress.Should().BeTrue();
    }

    [TestMethod]
    public async Task POST_GivenMissingRequiredFields_ThenSessionNotUpdated()
    {
        // Arrange
        var request = new BusinessAddressOverseasViewModel();
        _systemUnderTest.ModelState.AddModelError(nameof(BusinessAddressOverseasViewModel.AddressLine1), "Enter the first line of your address");
        _systemUnderTest.ModelState.AddModelError(nameof(BusinessAddressOverseasViewModel.TownOrCity), "Enter your town or city");

        // Act
        await _systemUnderTest.AddressOverseas(request);

        // Assert
        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<OrganisationSession>>()), Times.Never);
    }

    [TestMethod]
    public async Task POST_GivenMissingRequiredFields_ThenReturnView()
    {
        // Arrange
        var request = new BusinessAddressOverseasViewModel();
        _systemUnderTest.ModelState.AddModelError(nameof(BusinessAddressOverseasViewModel.AddressLine1), "Enter the first line of your address");

        // Act
        var result = await _systemUnderTest.AddressOverseas(request);

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [TestMethod]
    public async Task POST_GivenMissingRequiredFields_ThenViewHasCorrectBackLink()
    {
        // Arrange
        var request = new BusinessAddressOverseasViewModel();
        _systemUnderTest.ModelState.AddModelError(nameof(BusinessAddressOverseasViewModel.AddressLine1), "Enter the first line of your address");

        // Act
        var result = await _systemUnderTest.AddressOverseas(request);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        AssertBackLink(viewResult, PagePath.IsTradingNameDifferent);
    }

    [TestMethod]
    public async Task POST_GivenFieldTooLong_ThenReturnViewWithUsersBadInput()
    {
        // Arrange
        const string tooLongTownOrCity = "123456789 123456789 123456789 123456789 123456789 123456789 123456789 1";

        var request = new BusinessAddressOverseasViewModel
        {
            TownOrCity = tooLongTownOrCity
        };

        _systemUnderTest.ModelState.AddModelError(nameof(BusinessAddressOverseasViewModel.TownOrCity), "Town or city must be 70 characters or less");

        // Act
        var result = await _systemUnderTest.AddressOverseas(request);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<BusinessAddressOverseasViewModel>();
        var resultViewModel = (BusinessAddressOverseasViewModel?)viewResult.Model;
        resultViewModel!.TownOrCity.Should().Be(tooLongTownOrCity);
    }

    [TestMethod]
    public async Task POST_GivenNullAddress_ThenCreatesNewAddressInSession()
    {
        // Arrange
        _organisationSession.ReExManualInputSession!.BusinessAddress = null;

        var request = new BusinessAddressOverseasViewModel
        {
            Country = "Spain",
            AddressLine1 = "123 Test Street",
            TownOrCity = "Barcelona"
        };

        // Act
        await _systemUnderTest.AddressOverseas(request);

        // Assert
        var address = _organisationSession.ReExManualInputSession.BusinessAddress;
        address.Should().NotBeNull();
        address!.Country.Should().Be("Spain");
        address.BuildingName.Should().Be("123 Test Street");
        address.Town.Should().Be("Barcelona");
    }
}