using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.Pages.Re_Ex.Organisation;
using FrontendAccountCreation.Web.ViewModels;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class IsTradingNameDifferentTests() : YesNoPageTestBase<IsTradingNameDifferentViewModel>(
    c => c.IsTradingNameDifferent(),
    (c, vm) => c.IsTradingNameDifferent(vm),
    (session, val) => session.IsTradingNameDifferent = val,
    session => session.IsTradingNameDifferent,
    vm => vm.IsTradingNameDifferent)
{
    // Page and Journey details
    protected override string CurrentPagePath => PagePath.IsTradingNameDifferent;
    protected override string ExpectedBacklinkPagePath => PagePath.UkNation;

    protected override List<string> JourneyForGetBacklinkTest =>
    [
        PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber,
        PagePath.ConfirmCompanyDetails, PagePath.UkNation, PagePath.IsTradingNameDifferent
    ];

    // Redirect targets
    protected override string RedirectActionNameOnYes => "";
    protected override string RedirectActionNameOnNo => "";

    [Ignore]
    public override Task POST_UserSelectsYesOrNo_UserIsRedirected(YesNoAnswer userAnswer)
    {
        // replaced with the specific implementation below
        return Task.CompletedTask;
    }

    [TestMethod]
    [DataRow(true, nameof(OrganisationController.IsOrganisationAPartner), OrganisationType.CompaniesHouseCompany)]
    [DataRow(false, nameof(OrganisationController.AddressOverseas))]
    [DataRow(true, nameof(OrganisationController.TypeOfOrganisation))]
    public async Task POST_UserSelectsNo_UserIsRedirected(
        bool sessionIsUkMainAddress,
        string expectedRedirect,
        OrganisationType? orgType = null)
    {
        var orgCreationSession = new OrganisationSession
        {
            Journey = [CurrentPagePath],
            IsUkMainAddress = sessionIsUkMainAddress,
            OrganisationType = orgType,
            TradingName = "Test Trading Name"
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(orgCreationSession);

        var requestViewModel = new IsTradingNameDifferentViewModel();
        SetActualViewModelYesNoAnswer(requestViewModel, YesNoAnswer.No);

        var result = await PostPageAction(_systemUnderTest, requestViewModel);

        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(expectedRedirect);
    }

    [TestMethod]
    [DataRow(true, nameof(TradingName))]
    [DataRow(false, nameof(TradingName))]
    public async Task POST_UserSelectsYes_UserIsRedirected(
        bool sessionIsUkMainAddress,
        string expectedRedirect)
    {
        var orgCreationSession = new OrganisationSession
        {
            Journey = [CurrentPagePath],
            IsUkMainAddress = sessionIsUkMainAddress,
            OrganisationType = null
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(orgCreationSession);

        var requestViewModel = new IsTradingNameDifferentViewModel();
        SetActualViewModelYesNoAnswer(requestViewModel, YesNoAnswer.Yes);

        var result = await PostPageAction(_systemUnderTest, requestViewModel);

        var pageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        pageResult.PageName.Should().Be($"{PageName.Base}/{expectedRedirect}");
    }
}
