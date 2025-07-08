using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class UkNationtests : OrganisationTestBase
{
    private OrganisationSession _organisationCreationSessionMock = null!;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _organisationCreationSessionMock = new OrganisationSession
        {
            Journey = new List<string> { PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.TypeOfOrganisation, PagePath.TradingName, PagePath.ConfirmCompanyDetails, PagePath.UkNation },
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            IsUserChangingDetails = false
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_organisationCreationSessionMock);
    }

    [TestMethod]
    public async Task UkNation_Should_Return_UKNationView()
    {
        // Arrange
        _organisationCreationSessionMock.OrganisationType = OrganisationType.CompaniesHouseCompany;

        // Act
        var result = await _systemUnderTest.UkNation();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<UkNationViewModel>();
        var viewModel = (UkNationViewModel?)viewResult.Model;
        viewModel!.IsCompaniesHouseFlow.Should().Be(true);
        _sessionManagerMock.Verify(x => x.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task UkNation_OrganisationIsRegisteredWithCompaniesHouse_RedirectsToIsTradingNameDifferentCheckPageAndUpdateSession()
    {
        // Arrange
        _organisationCreationSessionMock.OrganisationType = OrganisationType.CompaniesHouseCompany;
        var request = new UkNationViewModel() { UkNation = Nation.England};

        // Act
        var result = await _systemUnderTest.UkNation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((result as RedirectToActionResult)!).ActionName.Should().Be(nameof(OrganisationController.IsTradingNameDifferent));
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task UkNation_UkNationSavedWithNoAnswer_ReturnsViewWithErrorAndBackLinkIsConfirmCompanyDetails()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(UkNationViewModel.UkNation), "Field is required");

        // Act
        var result = await _systemUnderTest.UkNation(new UkNationViewModel());

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<UkNationViewModel>();

        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<OrganisationSession>>()), Times.Never);
        AssertBackLink(viewResult, PagePath.ConfirmCompanyDetails);
    }

    [TestMethod]
    public async Task UkNation_OrganisationIsManualInputFlow_RedirectsToBusinessAddressPageAndUpdatesSession()
    {
        // Arrange
        _organisationCreationSessionMock.OrganisationType = OrganisationType.NonCompaniesHouseCompany;
        var request = new UkNationViewModel() { UkNation = Nation.Wales };

        // Act
        var result = await _systemUnderTest.UkNation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(OrganisationController.BusinessAddress));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.Is<OrganisationSession>(s => s.UkNation == Nation.Wales)), Times.Once);
    }

    [TestMethod]
    public async Task UkNation_GetRequest_ShouldSetManualFlowFlagsCorrectly()
    {
        // Arrange
        _organisationCreationSessionMock.OrganisationType = OrganisationType.NonCompaniesHouseCompany;

        // Act
        var result = await _systemUnderTest.UkNation();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = (UkNationViewModel)viewResult.Model!;

        model.IsCompaniesHouseFlow.Should().BeFalse();
        model.IsManualInputFlow.Should().BeTrue();
    }

    [TestMethod]
    public async Task UkNation_InvalidModel_CompaniesHouseFlow_SetsCorrectErrorMessage()
    {
        // Arrange
        _organisationCreationSessionMock.OrganisationType = OrganisationType.CompaniesHouseCompany;
        var model = new UkNationViewModel();
        _systemUnderTest.ModelState.AddModelError(nameof(UkNationViewModel.UkNation), "Required");

        // Act
        var result = await _systemUnderTest.UkNation(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<UkNationViewModel>();
        _systemUnderTest.ModelState[nameof(UkNationViewModel.UkNation)]!.Errors
            .First().ErrorMessage.Should().Be("UkNation.LimitedCompany.ErrorMessage");

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Never);
    }
}
