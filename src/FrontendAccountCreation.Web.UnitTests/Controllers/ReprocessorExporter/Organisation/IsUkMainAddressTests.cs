using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class IsUkMainAddressTests : OrganisationTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task GET_BackLinkRegisteredWithCompaniesHouse()
    {
        //Arrange
        var orgCreationSession = new OrganisationSession
        {
            Journey =
            [
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse,PagePath.IsUkMainAddress, PagePath.CompaniesHouseNumber,
                PagePath.ConfirmCompanyDetails, PagePath.UkNation, PagePath.IsTradingNameDifferent
            ]
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(orgCreationSession);

        //Act
        var result = await _systemUnderTest.IsUkMainAddress();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        AssertBackLink(viewResult, PagePath.RegisteredWithCompaniesHouse);
    }

    [TestMethod]
    [DataRow(true, YesNoAnswer.Yes)]
    [DataRow(false, YesNoAnswer.No)]
    [DataRow(null, null)]
    public async Task GET_CorrectViewModelIsReturnedInTheView(bool? isUkMainAddressSession, YesNoAnswer? expectedIsUkMainAddressViewModel)
    {
        //Arrange
        var orgCreationSession = new OrganisationSession
        {
            Journey =
            [
                PagePath.RegisteredAsCharity,
                PagePath.RegisteredWithCompaniesHouse,
                PagePath.IsUkMainAddress,
                PagePath.TradingName
            ],
            IsUkMainAddress = isUkMainAddressSession
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(orgCreationSession);

        //Act
        var result = await _systemUnderTest.IsUkMainAddress();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<IsUkMainAddressViewModel>();
        var viewModel = (IsUkMainAddressViewModel?)viewResult.Model;
        viewModel!.IsUkMainAddress.Should().Be(expectedIsUkMainAddressViewModel);
    }

    [TestMethod]
    [DataRow(YesNoAnswer.Yes, true)]
    [DataRow(YesNoAnswer.No, false)]
    public async Task POST_UserSelectsYesOrNo_SessionUpdatedCorrectly(YesNoAnswer userAnswer, bool expectedIsUkMainAddressInSession)
    {
        // Arrange
        var request = new IsUkMainAddressViewModel { IsUkMainAddress = userAnswer };

        // Act
        await _systemUnderTest.IsUkMainAddress(request);

        // Assert
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(),
            It.Is<OrganisationSession>(os => os.IsUkMainAddress == expectedIsUkMainAddressInSession)),
            Times.Once);
    }

    [TestMethod]
    public async Task POST_UserSelectsNothing_SessionNotUpdated()
    {
        // Arrange
        var request = new IsUkMainAddressViewModel { IsUkMainAddress = null };
        _systemUnderTest.ModelState.AddModelError("IsUkMainAddress", "Select if your organisation have a main address in the UK");

        // Act
        await _systemUnderTest.IsUkMainAddress(request);

        // Assert
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(),
                It.IsAny<OrganisationSession>()),
            Times.Never);
    }

    [TestMethod]
    public async Task POST_UserSelectsNothing_ViewIsReturnedWithCorrectModel()
    {
        // Arrange
        var request = new IsUkMainAddressViewModel { IsUkMainAddress = null };
        _systemUnderTest.ModelState.AddModelError("IsUkMainAddress", "Select if your organisation have a main address in the UK");

        // Act
        var result = await _systemUnderTest.IsUkMainAddress(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<IsUkMainAddressViewModel>();
        var viewModel = (IsUkMainAddressViewModel?)viewResult.Model;
        viewModel!.IsUkMainAddress.Should().BeNull();
    }

    [TestMethod]
    [DataRow(YesNoAnswer.Yes, nameof(OrganisationController.TradingName))]
    [DataRow(YesNoAnswer.No, nameof(OrganisationController.IsOrganisationAPartner))]
    public async Task POST_UserSelectsYesOrNo_UserIsRedirected(YesNoAnswer userAnswer, string expectedRedirect)
    {
        // Arrange
        var request = new IsUkMainAddressViewModel { IsUkMainAddress = userAnswer };

        // Act
        var result = await _systemUnderTest.IsUkMainAddress(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(expectedRedirect);
    }
}