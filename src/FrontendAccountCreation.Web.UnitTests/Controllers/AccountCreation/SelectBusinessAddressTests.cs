using FluentAssertions;
using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.AccountCreation;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.AccountCreation;

[TestClass]
public class SelectBusinessAddressTests : AccountCreationTestBase
{
    private AccountCreationSession _accountCreationSessionMock = null!;
    private static readonly IList<Address> _addressesMock = new List<Address>
    {
        new() { BuildingNumber = "10", Street = "Gracefield Gardens", Town = "London" },
        new() { BuildingNumber = "11", Street = "Gracefield Gardens", Town = "London" },
        new() { BuildingNumber = "12", Street = "Gracefield Gardens", Town = "London" }
    };
    

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string> { PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.TypeOfOrganisation, PagePath.TradingName, PagePath.BusinessAddressPostcode, PagePath.SelectBusinessAddress },
            ManualInputSession = new ManualInputSession() { BusinessAddress = new Address { Postcode = "NW2 3TB" } },
            IsUserChangingDetails = false
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_accountCreationSessionMock);
        
        var addressListMock = new AddressList
        {
            Addresses = _addressesMock
        };
        _facadeServiceMock.Setup(x => x.GetAddressListByPostcodeAsync(It.IsAny<string>())).ReturnsAsync(addressListMock);
    }

    [TestMethod]
    public async Task GivenFailedPostcodeLookup_WhenSelectBusinessAddressCalled_TheUserHasBeenRedirectedToBusinessAddressPage()
    {
        //Arrange
        _facadeServiceMock
            .Setup(x => x.GetAddressListByPostcodeAsync(It.IsAny<string>()))
            .Throws(new Exception());

        //Act
        var result = await _systemUnderTest.SelectBusinessAddress();

        //Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirection = (RedirectToActionResult)result;

        redirection.ActionName.Should().Be(nameof(AccountCreationController.BusinessAddress));

        _tempDataDictionaryMock.Invocations.SelectMany(x => x.Arguments).Should().Contain(PostcodeLookupFailedKey);
    }
    
    [TestMethod]
    public async Task GivenFailedPostcodeLookup_GetAddressList_ReturnsNull_WhenSelectBusinessAddressCalled_TheUserHasBeenRedirectedToBusinessAddressPage()
    {
        //Arrange
        _facadeServiceMock
            .Setup(x => x.GetAddressListByPostcodeAsync(It.IsAny<string>()))
            .Returns((Task<AddressList?>)null);

        //Act
        var result = await _systemUnderTest.SelectBusinessAddress();

        //Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirection = (RedirectToActionResult)result;

        redirection.ActionName.Should().Be(nameof(AccountCreationController.BusinessAddress));

        _tempDataDictionaryMock.Invocations.SelectMany(x => x.Arguments).Should().Contain(PostcodeLookupFailedKey);
    }
    
    [TestMethod]
    public async Task GivenFinishedPreviousPage_WhenSelectBusinessAddressCalled_ThenSelectBusinessAddressPageReturned_WithBusinessAddressPostcodeAsTheBackLink()
    {
        //Act
        var result = await _systemUnderTest.SelectBusinessAddress();

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<SelectBusinessAddressViewModel>();
        AssertBackLink(viewResult, PagePath.BusinessAddressPostcode);
    }

    [TestMethod]
    public async Task SelectBusinessAddress_WhenAddressIndexIsCorrect_ItShouldUpdateSessionAndRedirectToNextScreen()
    {
        _accountCreationSessionMock.ManualInputSession!.AddressesForPostcode = new List<Address?> { new() };

        var result = await _systemUnderTest.SelectBusinessAddress(
            new SelectBusinessAddressViewModel { SelectedListIndex = "0" });

        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.UkNation));

        _sessionManagerMock.Verify(sessionManager => sessionManager
            .SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task SelectBusinessAddress_WhenAddressIndexIsOutOfBounds_ItShouldReturnViewWithError()
    {
        _accountCreationSessionMock.ManualInputSession!.AddressesForPostcode = new List<Address?> { new() };

        var result = await _systemUnderTest.SelectBusinessAddress(
            new SelectBusinessAddressViewModel { SelectedListIndex = "1" });

        result.Should().BeOfType<ViewResult>();

        ((ViewResult)result).ViewData.ModelState.IsValid.Should().BeFalse();

        _sessionManagerMock.Verify(sessionManager => sessionManager
            .SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Never);
    }

    [TestMethod]
    public async Task SelectBusinessAddress_WhenAddressIndexIsNotNumber_ItShouldReturnViewWithError()
    {
        var result = await _systemUnderTest.SelectBusinessAddress(
            new SelectBusinessAddressViewModel { SelectedListIndex = "a" });

        result.Should().BeOfType<ViewResult>();

        ((ViewResult)result).ViewData.ModelState.IsValid.Should().BeFalse();

        _sessionManagerMock.Verify(sessionManager => sessionManager
            .SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Never);
    }
    
    [TestMethod]
    public async Task GivenNullPostcodeLookup_WhenSelectBusinessAddressCalled_TheUserHasBeenRedirectedToBusinessAddressPage()
    {
        //Arrange
        _facadeServiceMock
            .Setup(x => x.GetAddressListByPostcodeAsync(It.IsAny<string>()));

        //Act
        var result = await _systemUnderTest.SelectBusinessAddress();

        //Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirection = (RedirectToActionResult)result;

        redirection.ActionName.Should().Be(nameof(AccountCreationController.BusinessAddress));

        _tempDataDictionaryMock.Invocations.SelectMany(x => x.Arguments).Should().Contain(PostcodeLookupFailedKey);
    }

    [TestMethod]
    public async Task UserNavigatesToSelectBusinessAddressPage_FromCheckYourDetailsPage_BackLinkShouldBeCheckYourDetails()
    {
        //Arrange
        var accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string>
            {
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.TypeOfOrganisation, PagePath.TradingName,
                PagePath.BusinessAddressPostcode, PagePath.SelectBusinessAddress, PagePath.UkNation, PagePath.ManualInputRoleInOrganisation,
                PagePath.FullName, PagePath.TelephoneNumber, PagePath.CheckYourDetails
            },
            ManualInputSession = new ManualInputSession() { BusinessAddress = new Address { Postcode = "NW2 3TB" } },
            IsUserChangingDetails = true,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(accountCreationSessionMock);

        //Act
        var result = await _systemUnderTest.SelectBusinessAddress();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<SelectBusinessAddressViewModel>();
        AssertBackLink(viewResult, PagePath.CheckYourDetails);

    }
}
