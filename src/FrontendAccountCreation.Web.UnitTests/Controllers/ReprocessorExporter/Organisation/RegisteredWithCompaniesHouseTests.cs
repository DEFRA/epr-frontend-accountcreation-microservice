using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class RegisteredWithCompaniesHouseTests : OrganisationTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task POST_SoleTraderFeatureIsDisabledAndUserSelectsNo_UserIsRedirectedToReExPageNotFound()
    {
        // Arrange
        _featureManagerMock.Setup(f => f.IsEnabledAsync(FeatureFlags.AddOrganisationSoleTraderJourney))
            .ReturnsAsync(false);

        var orgCreationSessionMock = new OrganisationSession
        {
            Journey = [PagePath.RegisteredWithCompaniesHouse]
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(orgCreationSessionMock);

        var request = new RegisteredWithCompaniesHouseViewModel { IsTheOrganisationRegistered = YesNoAnswer.No };

        // Act
        var result = await _systemUnderTest.RegisteredWithCompaniesHouse(_featureManagerMock.Object, request);

        // Assert
        result.Should().BeOfType<RedirectResult>();

        ((RedirectResult)result).Url.Should().Be(PagePath.PageNotFoundReEx);
    }

    [TestMethod]
    public async Task RegisteredWithCompaniesHouse_OrganisationIsRegistered_RedirectsToCompaniesHouseNumberPage_AndUpdateSession()
    {
        // Arrange
        var request = new RegisteredWithCompaniesHouseViewModel { IsTheOrganisationRegistered = YesNoAnswer.Yes };

        // Act
        var result = await _systemUnderTest.RegisteredWithCompaniesHouse(_featureManagerMock.Object, request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.CompaniesHouseNumber));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    [DataRow(OrganisationType.NonCompaniesHouseCompany)]
    [DataRow(OrganisationType.CompaniesHouseCompany)]
    public async Task RegisteredWithCompaniesHouse_OrganisationIsNotRegistered_RedirectsToIsUkMainAddressPage_AndUpdateSession(OrganisationType orgType)
    {
        // Arrange

        var orgCreationSessionMock = new OrganisationSession
        {
            Journey = [PagePath.RegisteredWithCompaniesHouse],
            OrganisationType = orgType
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(orgCreationSessionMock);

        var request = new RegisteredWithCompaniesHouseViewModel { IsTheOrganisationRegistered = YesNoAnswer.No };

        // Act
        var result = await _systemUnderTest.RegisteredWithCompaniesHouse(_featureManagerMock.Object, request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.IsUkMainAddress));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task RegisteredWithCompaniesHouse_PageIsSavedWithNoAnswer_ReturnsViewWithError()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(RegisteredWithCompaniesHouseViewModel.IsTheOrganisationRegistered), "Field is required");

        // Act
        var result = await _systemUnderTest.RegisteredWithCompaniesHouse(_featureManagerMock.Object, new RegisteredWithCompaniesHouseViewModel());

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<RegisteredWithCompaniesHouseViewModel>();

        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<OrganisationSession>>()), Times.Never);
    }

    [TestMethod]
    public async Task RegisteredWithCompaniesHouse_OrganisationIsRegistered_RedirectsToViewResult()
    {
        // Arrange
        var orgCreationSessionMock = new OrganisationSession
        {
            Journey = new List<string> { PagePath.RegisteredWithCompaniesHouse }
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(orgCreationSessionMock);


        // Act
        var result = await _systemUnderTest.RegisteredWithCompaniesHouse();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [TestMethod]
    public async Task RegisteredWithCompaniesHouse_RegisteredWithCompaniesHousePageIsExited_BackLinkIsRegisteredAsCharity()
    {
        //Arrange
        var orgCreationSessionMock = new OrganisationSession
        {
            Journey = new List<string>
            {
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse
            },
            IsUserChangingDetails = false,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(orgCreationSessionMock);

        //Act
        var result = await _systemUnderTest.RegisteredWithCompaniesHouse();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<RegisteredWithCompaniesHouseViewModel>();
        AssertBackLink(viewResult, PagePath.RegisteredAsCharity);
    }

    [TestMethod]
    public async Task UserNavigatesToRegisteredWithCompaniesHousePage_FromCheckYourDetailsPage_BackLinkShouldBeCheckYourDetails()
    {
        //Arrange
        var orgCreationSessionMock = new OrganisationSession
        {
            Journey =
            [
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.TypeOfOrganisation,PagePath.TradingName,
                PagePath.ConfirmCompanyDetails, PagePath.RoleInOrganisation, PagePath.FullName, PagePath.TelephoneNumber,
                PagePath.CheckYourDetails
            ],
            IsUserChangingDetails = true,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(orgCreationSessionMock);

        //Act
        var result = await _systemUnderTest.RegisteredWithCompaniesHouse();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<RegisteredWithCompaniesHouseViewModel>();
        AssertBackLink(viewResult, PagePath.CheckYourDetails);

    }

    [TestMethod]
    [DataRow(OrganisationType.NotSet, null)]
    [DataRow(OrganisationType.CompaniesHouseCompany, YesNoAnswer.Yes)]
    [DataRow(OrganisationType.NonCompaniesHouseCompany, YesNoAnswer.No)]
    public async Task RegisteredWithCompaniesHouse_RegisteredWithCompaniesHousePageIsExited_WithDifferent_OrganisationType(OrganisationType orgType, YesNoAnswer? expectedAnswer)
    {
        //Arrange
        var orgCreationSessionMock = new OrganisationSession
        {
            Journey = new List<string>
            {
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse
            },
            IsUserChangingDetails = false,
            OrganisationType = orgType
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(orgCreationSessionMock);

        expectedAnswer = orgType == OrganisationType.NotSet ? null : expectedAnswer;

        //Act
        var result = await _systemUnderTest.RegisteredWithCompaniesHouse();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<RegisteredWithCompaniesHouseViewModel>();
        AssertBackLink(viewResult, PagePath.RegisteredAsCharity);
        ((FrontendAccountCreation.Web.ViewModels.AccountCreation.RegisteredWithCompaniesHouseViewModel)((Microsoft.AspNetCore.Mvc.ViewResult)result).Model).IsTheOrganisationRegistered.Should().Be(expectedAnswer);
    }

    [TestMethod]
    public async Task TypeOfOrganisation_Returns_View()
    {
        // Arrange

        var orgCreationSessionMock = new OrganisationSession
        {
            Journey = [PagePath.TypeOfOrganisation]
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(orgCreationSessionMock);

        // Act
        var result = await _systemUnderTest.TypeOfOrganisation();
        var viewResult = (ViewResult)result;

        // Assert
        viewResult.ViewData.Count.Should().Be(1);
        _sessionManagerMock.Verify(x => x.GetSessionAsync(It.IsAny<ISession>()), Times.Once());
    }

    [TestMethod]
    public async Task CannotVerifyOrganisation_Returns_View()
    {
        // Arrange

        var orgCreationSessionMock = new OrganisationSession
        {
            Journey = [PagePath.CannotVerifyOrganisation]
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(orgCreationSessionMock);

        // Act
        var result = await _systemUnderTest.CannotVerifyOrganisation();
        var viewResult = (ViewResult)result;

        // Assert
        viewResult.ViewData.Count.Should().Be(1);
        _sessionManagerMock.Verify(x => x.GetSessionAsync(It.IsAny<ISession>()), Times.Once());
    }

    [TestMethod]
    public async Task ConfirmCompanyDetails_Returns_View()
    {
        // Arrange

        var orgCreationSessionMock = new OrganisationSession
        {
            Journey = [PagePath.ConfirmCompanyDetails]
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(orgCreationSessionMock);

        // Act
        var result = await _systemUnderTest.CannotVerifyOrganisation();
        var viewResult = (ViewResult)result;

        // Assert
        viewResult.ViewData.Count.Should().Be(1);
        _sessionManagerMock.Verify(x => x.GetSessionAsync(It.IsAny<ISession>()), Times.Once());
    }
}