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
public class SoleTraderTests() : YesNoPageTestBase<SoleTraderViewModel>(
    c => c.SoleTrader(),
    (c, vm) => c.SoleTrader(vm),
    (session, val) => session.IsIndividualInCharge = val,
    session => session.IsIndividualInCharge,
    vm => vm.IsIndividualInCharge)
{
    // Page and Journey details
    protected override string CurrentPagePath => PagePath.SoleTrader;
    protected override string ExpectedBacklinkPagePath => PagePath.BusinessAddress;
    protected override List<string> JourneyForGetBacklinkTest =>
    [
        PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.IsUkMainAddress,
        PagePath.TradingName, PagePath.TypeOfOrganisation, PagePath.UkNation,
        PagePath.BusinessAddress, // Expected backlink
        PagePath.SoleTrader // Current page
    ];

    // Redirect targets
    protected override string RedirectActionNameOnYes =>
        nameof(ApprovedPersonController.YouAreApprovedPersonSoleTrader);

    //todo: revisit once 'Add an approved person' page is in
    //protected override string RedirectActionNameOnNo => nameof(OrganisationController.NotApprovedPerson);
    protected override string RedirectActionNameOnNo => nameof(ApprovedPersonController.AddApprovedPerson);

    [TestMethod]
    [DataRow(YesNoAnswer.Yes)]
    [DataRow(YesNoAnswer.No)]
    public override async Task POST_UserSelectsYesOrNo_UserIsRedirected(YesNoAnswer userAnswer)
    {
        var requestViewModel = new SoleTraderViewModel();
        SetActualViewModelYesNoAnswer(requestViewModel, userAnswer);
        var expectedRedirect = userAnswer == YesNoAnswer.Yes ? RedirectActionNameOnYes : RedirectActionNameOnNo;

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
        .ReturnsAsync(new OrganisationSession
        {
            ReExManualInputSession = new ReExManualInputSession
            {
                TeamMember = null
            }
        });

        var result = await PostPageAction(_systemUnderTest, requestViewModel);
        result.Should().BeOfType<RedirectToActionResult>()
              .Which.ActionName.Should().Be(expectedRedirect);
    }

    [TestMethod]
    [DataRow(YesNoAnswer.Yes, true)]
    [DataRow(YesNoAnswer.No, false)]
    public override async Task POST_UserSelectsYesOrNo_SessionUpdatedCorrectly(YesNoAnswer userAnswer, bool expectedSessionValue)
    {
        var requestViewModel = new SoleTraderViewModel();
        SetActualViewModelYesNoAnswer(requestViewModel, userAnswer);

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
        .ReturnsAsync(new OrganisationSession
        {
            ReExManualInputSession = new ReExManualInputSession
            {
                TeamMember = null
            }
        });

        await PostPageAction(_systemUnderTest, requestViewModel);

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(),
            It.Is<OrganisationSession>(os => GetSessionValueForPostTest(os) == expectedSessionValue)),
            Times.Once);
    }
}