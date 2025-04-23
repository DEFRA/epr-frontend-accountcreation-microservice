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
}
