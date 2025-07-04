//using FluentAssertions;
//using FrontendAccountCreation.Core.Sessions;
//using FrontendAccountCreation.Core.Sessions.ReEx;
//using FrontendAccountCreation.Web.Constants;
//using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
//using FrontendAccountCreation.Web.ViewModels.AccountCreation;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Moq;

//namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

//[TestClass]
//public class TradingNameTests : OrganisationTestBase
//{
//    private OrganisationSession _organisationSession = null!;

//    [TestInitialize]
//    public void Setup()
//    {
//        SetupBase();

//        _organisationSession = new OrganisationSession
//        {
//            Journey =
//            [
//                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber,
//                PagePath.ConfirmCompanyDetails, PagePath.UkNation, PagePath.IsTradingNameDifferent,
//                PagePath.TradingName
//            ]
//        };

//        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_organisationSession);
//    }

//    [TestMethod]
//    public async Task GET_WhenTradingNameIsNotInSession_ThenViewIsReturnedWithoutTradingName()
//    {
//        //Act
//        var result = await _systemUnderTest.TradingName();

//        //Assert
//        result.Should().BeOfType<ViewResult>();
//        var viewResult = (ViewResult)result;
//        viewResult.Model.Should().BeOfType<TradingNameViewModel>();
//        var viewModel = (TradingNameViewModel?)viewResult.Model;
//        viewModel!.TradingName.Should().BeNull();
//    }

//    [TestMethod]
//    public async Task GET_WhenTradingNameIsInSession_ThenViewIsReturnedWithTradingName()
//    {
//        //Arrange
//        const string tradingName = "Trading name";
//        _organisationSession.TradingName = tradingName;
//        _organisationSession.ReExManualInputSession = new ReExManualInputSession
//        {
//            BusinessAddress = new Core.Addresses.Address()
//        };

//        //Act
//        var result = await _systemUnderTest.TradingName();

//        //Assert
//        result.Should().BeOfType<ViewResult>();
//        var viewResult = (ViewResult)result;
//        viewResult.Model.Should().BeOfType<TradingNameViewModel>();
//        var viewModel = (TradingNameViewModel?)viewResult.Model;
//        viewModel!.TradingName.Should().Be(tradingName);
//    }

//    [TestMethod]
//    public async Task GET_ThenBackLinkIsCorrect()
//    {
//        //Act
//        var result = await _systemUnderTest.TradingName();

//        //Assert
//        result.Should().BeOfType<ViewResult>();
//        var viewResult = (ViewResult)result;
//        AssertBackLink(viewResult, PagePath.IsTradingNameDifferent);
//    }

//    [TestMethod]
//    public async Task POST_GivenTradingName_CompaniesHouseFlow_ThenRedirectToPartnerOrganisation()
//    {
//        // Arrange
//        var request = new TradingNameViewModel { TradingName = "John Brown Greengrocers" };
//        _organisationSession.OrganisationType = OrganisationType.CompaniesHouseCompany;

//        // Act
//        var result = await _systemUnderTest.TradingName(request);

//        // Assert
//        result.Should().BeOfType<RedirectToActionResult>();

//        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.IsOrganisationAPartner));
//    }

//    [TestMethod]
//    public async Task POST_GivenTradingName_NonCompaniesHouseFlow_IsSoleTrader_ThenRedirectToTypeOfOrganisation()
//    {
//        // Arrange
//        var request = new TradingNameViewModel { TradingName = "John Brown Greengrocers" };
//        _organisationSession.OrganisationType = OrganisationType.NonCompaniesHouseCompany;
//        _organisationSession.ReExManualInputSession = new ReExManualInputSession
//        {
//            ProducerType = ProducerType.SoleTrader
//        };

//        // Act
//        var result = await _systemUnderTest.TradingName(request);

//        // Assert
//        result.Should().BeOfType<RedirectToActionResult>();

//        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.TypeOfOrganisation));
//    }

//    [TestMethod]
//    public async Task POST_GivenTradingName_ThenUpdatesSession()
//    {
//        // Arrange
//        var request = new TradingNameViewModel { TradingName = "John Brown Greengrocers" };
//        _organisationSession.OrganisationType = OrganisationType.NonCompaniesHouseCompany;
//        _organisationSession.ReExManualInputSession = new ReExManualInputSession
//        {
//            ProducerType = ProducerType.SoleTrader
//        };

//        // Act
//        await _systemUnderTest.TradingName(request);

//        // Assert
//        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
//    }

//    [TestMethod]
//    public async Task POST_GivenNoTradingName_ThenSessionNotUpdated()
//    {
//        // Arrange
//        _systemUnderTest.ModelState.AddModelError(nameof(TradingNameViewModel.TradingName), "Trading name field is required");

//        // Act
//        await _systemUnderTest.TradingName(new TradingNameViewModel());

//        // Assert
//        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<OrganisationSession>>()),
//            Times.Never);
//    }

//    [TestMethod]
//    public async Task POST_GivenNoTradingName_ThenReturnView()
//    {
//        // Arrange
//        _systemUnderTest.ModelState.AddModelError(nameof(TradingNameViewModel.TradingName), "Trading name field is required");
//        var viewModel = new TradingNameViewModel
//        {
//            TradingName = ""
//        };

//        // Act
//        var result = await _systemUnderTest.TradingName(viewModel);

//        // Assert
//        result.Should().BeOfType<ViewResult>();
//    }

//    [TestMethod]
//    public async Task POST_GivenTradingNameTooLong_ThenReturnViewWithUsersBadInput()
//    {
//        // Arrange
//        const string badTradingName = "123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789";

//        _systemUnderTest.ModelState.AddModelError(nameof(TradingNameViewModel.TradingName), "Trading name must be 170 characters or less");
//        var viewModel = new TradingNameViewModel
//        {
//            TradingName = badTradingName
//        };

//        // Act
//        var result = await _systemUnderTest.TradingName(viewModel);

//        // Assert
//        result.Should().BeOfType<ViewResult>();
//        var viewResult = (ViewResult)result;

//        viewResult.Model.Should().BeOfType<TradingNameViewModel?>();
//        var resultViewModel = (TradingNameViewModel?)viewResult.Model;
//        resultViewModel!.TradingName.Should().Be(badTradingName);
//    }

//    [TestMethod]
//    public async Task POST_GivenNoTradingName_ThenViewHasCorrectBackLink()
//    {
//        // Arrange
//        _systemUnderTest.ModelState.AddModelError(nameof(TradingNameViewModel.TradingName), "Trading name field is required");

//        // Act
//        var result = await _systemUnderTest.TradingName(new TradingNameViewModel());

//        // Assert
//        result.Should().BeOfType<ViewResult>();

//        var viewResult = (ViewResult)result;

//        AssertBackLink(viewResult, PagePath.IsTradingNameDifferent);
//    }

//    [TestMethod]
//    public async Task POST_GivenTradingName_WithNonUKOrganisationProducerType_Flow_Redirects_To_AddressOverseas()
//    {
//        // Arrange
//        var request = new TradingNameViewModel { TradingName = "John Brown Greengrocers" };
//        _organisationSession.ReExManualInputSession = new ReExManualInputSession
//        {
//            ProducerType = ProducerType.NonUkOrganisation
//        };

//        // Act
//        var result = await _systemUnderTest.TradingName(request);

//        // Assert
//        result.Should().BeOfType<RedirectToActionResult>();

//        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.AddressOverseas));
//    }
//}
