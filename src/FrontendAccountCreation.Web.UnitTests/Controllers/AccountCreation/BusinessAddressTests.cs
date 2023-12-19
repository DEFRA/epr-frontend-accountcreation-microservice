using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.AccountCreation;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.AccountCreation;

[TestClass]
public class BusinessAddressTests : AccountCreationTestBase
{
    private AccountCreationSession _session = null!;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _session = new AccountCreationSession
        {
            Journey = new List<string> { PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.TypeOfOrganisation, PagePath.TradingName,
                PagePath.BusinessAddressPostcode, PagePath.SelectBusinessAddress,PagePath.BusinessAddress,
            },
            ManualInputSession = new ManualInputSession
            {
                BusinessAddress = new Core.Addresses.Address()
            },
            IsUserChangingDetails = false
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_session);
    }

    [TestMethod]
    public async Task GivenValidBusinessAddress_WhenBusinessAddressCalled_ThenRedirectToUkNationPage_AndUpdateSession()
    {
        // Arrange
        var request = new BusinessAddressViewModel { BuildingNumber = "10", SubBuildingName = "Dummy House", Postcode = "AB01 BB3", Town = "Nowhere" };

        // Act
        var result = await _systemUnderTest.BusinessAddress(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.UkNation));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenIncompleteBusinessAddress_WhenBusinessAddressCalled_ThenReturnViewWithError()
    {
        // Arrange
        _session.Journey.RemoveAll(x => x == PagePath.SelectBusinessAddress);

        _systemUnderTest.ModelState.AddModelError(nameof(BusinessAddressViewModel.BuildingNumber), "Field is required");

        var request = new BusinessAddressViewModel { Postcode = "AB01 BB3", Town = "Nowhere" };

        // Act
        var result = await _systemUnderTest.BusinessAddress(request);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<BusinessAddressViewModel>();

        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<AccountCreationSession>>()), Times.Never);

        AssertBackLink(viewResult, PagePath.BusinessAddressPostcode);
    }

    [TestMethod]
    public async Task GivenIncompleteBusinessAddressWithTempData_WhenBusinessAddressCalled_ThenReturnViewWithError()
    {
        // Arrange
        _session.Journey.RemoveAll(x => x == PagePath.SelectBusinessAddress);

        _tempDataDictionaryMock.Setup(x => x.ContainsKey(It.IsAny<string>())).Returns(true);

        _systemUnderTest.ModelState.AddModelError(nameof(BusinessAddressViewModel.BuildingNumber), "Field is required");

        var request = new BusinessAddressViewModel { Postcode = "AB01 BB3", Town = "Nowhere" };

        // Act
        var result = await _systemUnderTest.BusinessAddress(request);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<BusinessAddressViewModel>();

        var viewModel = (BusinessAddressViewModel)viewResult.Model!;

        viewModel.ShowWarning.Should().BeTrue();

        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<AccountCreationSession>>()), Times.Never);

        AssertBackLink(viewResult, PagePath.BusinessAddressPostcode);
    }

    [TestMethod]
    public async Task GivenFinishedPreviousPage_WhenBusinessAddressCalled_ThenBusinessAddressPageReturned_WithSelectBusinessAddresseAsTheBackLink()
    {
        //Act
        var result = await _systemUnderTest.BusinessAddress();

        //Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<BusinessAddressViewModel>();

        AssertBackLink(viewResult, PagePath.BusinessAddressPostcode);

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }
    
    [TestMethod]
    public async Task GivenBusinessAddress_WhenBusinessAddressIsManualAddress_ThenUpdatesModelWIthCorrectValue()
    {
        //Arrange
        _session.ManualInputSession.BusinessAddress.IsManualAddress = true;
        _session.ManualInputSession.BusinessAddress.SubBuildingName = "sub building name";
        _session.ManualInputSession.BusinessAddress.BuildingName = "building name";
        _session.ManualInputSession.BusinessAddress.BuildingNumber = "20";
        _session.ManualInputSession.BusinessAddress.Street = "street";
        _session.ManualInputSession.BusinessAddress.Town = "town";
        _session.ManualInputSession.BusinessAddress.County = "Yorkshire";
        
        //Act
        var result = await _systemUnderTest.BusinessAddress();

        //Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<BusinessAddressViewModel>();
        var model = (BusinessAddressViewModel)viewResult.Model!;

        AssertBackLink(viewResult, PagePath.BusinessAddressPostcode);

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
        Assert.AreEqual("sub building name", model.SubBuildingName);
        Assert.AreEqual("building name", model.BuildingName);
        Assert.AreEqual("20", model.BuildingNumber);
        Assert.AreEqual("street", model.Street);
        Assert.AreEqual("town", model.Town);
        Assert.AreEqual("Yorkshire", model.County);
    }

    [TestMethod]
    public async Task UserNavigatesToBusinessAddressPage_FromCheckYourDetailsPage_BackLinkShouldBeCheckYourDetails()
    {
        //Arrange
        var accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string>
            {
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.TypeOfOrganisation, PagePath.TradingName,
                PagePath.BusinessAddressPostcode, PagePath.SelectBusinessAddress,PagePath.BusinessAddress, PagePath.UkNation,
                PagePath.RoleInOrganisation, PagePath.FullName, PagePath.TelephoneNumber, PagePath.CheckYourDetails
            },
            ManualInputSession = new ManualInputSession
            {
                BusinessAddress = new Core.Addresses.Address()
            },
            IsUserChangingDetails = true,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(accountCreationSessionMock);

        //Act
        var result = await _systemUnderTest.BusinessAddress();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<BusinessAddressViewModel>();
        AssertBackLink(viewResult, PagePath.CheckYourDetails);

    }
}
