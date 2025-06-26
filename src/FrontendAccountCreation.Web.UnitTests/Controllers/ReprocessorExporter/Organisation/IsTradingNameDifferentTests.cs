using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
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

    public override Task POST_UserSelectsYesOrNo_UserIsRedirected(YesNoAnswer userAnswer)
    {
        // replaced with the specific implementation below
        return Task.CompletedTask;
    }

    [TestMethod]
    [DataRow(YesNoAnswer.Yes, true, nameof(OrganisationController.TradingName))]
    [DataRow(YesNoAnswer.Yes, false, nameof(OrganisationController.TradingName))]
    [DataRow(YesNoAnswer.No,  true,  nameof(OrganisationController.IsOrganisationAPartner), OrganisationType.CompaniesHouseCompany)]
    [DataRow(YesNoAnswer.No,  false, nameof(OrganisationController.AddressOverseas))]
    [DataRow(YesNoAnswer.No, true, nameof(OrganisationController.TypeOfOrganisation))]
    public async Task POST_UserSelectsYesOrNo_UserIsRedirected(
        YesNoAnswer userAnswer,
        bool sessionIsUkMainAddress,
        string expectedRedirect,
        OrganisationType? orgType = null)
    {
        var orgCreationSession = new OrganisationSession
        {
            Journey = [CurrentPagePath],
            IsUkMainAddress = sessionIsUkMainAddress,
            OrganisationType = orgType
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(orgCreationSession);

        var requestViewModel = new IsTradingNameDifferentViewModel();
        SetActualViewModelYesNoAnswer(requestViewModel, userAnswer);

        var result = await PostPageAction(_systemUnderTest, requestViewModel);

        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(expectedRedirect);
    }
}
