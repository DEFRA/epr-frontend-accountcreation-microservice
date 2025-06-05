using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Linq.Expressions;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter;

//todo: using ctor params would look neater and easier to implement in the derived classes

// supports [Parallelize(Scope = ExecutionScope.MethodLevel)], but don't want to apply that for all tests in the assembly
public abstract class YesNoPageTestBase<TViewModel> : OrganisationTestBase
    where TViewModel : class, new()
{
    protected Func<OrganisationController, Task<IActionResult>> GetPageAction { get; }
    protected Func<OrganisationController, TViewModel, Task<IActionResult>> PostPageAction { get; }

    // we could replace these 3 with a single property,
    // but that's probably too much implementation complexity for a yes/no test base that is tied to OrganisationTestBase.
    protected Action<OrganisationSession, bool?> SetSessionValueForGetTest { get; }
    protected Func<OrganisationSession, bool?> GetSessionValueForPostTest { get; }

    // Expression to define the Yes/No property on the ViewModel
    protected abstract Expression<Func<TViewModel, YesNoAnswer?>> ViewModelYesNoPropertyExpression { get; }

    protected abstract string CurrentPagePath { get; }
    protected abstract string ExpectedBacklinkPagePath { get; }
    protected abstract List<string> JourneyForGetBacklinkTest { get; }
    protected abstract string RedirectActionNameOnYes { get; }
    protected abstract string RedirectActionNameOnNo { get; }

    private readonly Lazy<ViewModelPropertyAccessors<TViewModel, YesNoAnswer?>> _lazyViewModelAccessors;

    protected string ActualViewModelYesNoPropertyName => _lazyViewModelAccessors.Value.PropertyName;

    protected YesNoAnswer? GetActualViewModelYesNoAnswer(TViewModel viewModel) =>
        _lazyViewModelAccessors.Value.Getter(viewModel);

    protected void SetActualViewModelYesNoAnswer(TViewModel viewModel, YesNoAnswer? value) =>
        _lazyViewModelAccessors.Value.Setter(viewModel, value);

    protected virtual string ViewModelMissingInputErrorMessage => $"Select yes or no for {ActualViewModelYesNoPropertyName}";

    protected YesNoPageTestBase(
        Func<OrganisationController, Task<IActionResult>> getPageAction,
        Func<OrganisationController, TViewModel, Task<IActionResult>> postPageAction,
        Action<OrganisationSession, bool?> setSessionValueForGetTest,
        Func<OrganisationSession, bool?> getSessionValueForPostTest)
    {
        GetPageAction = getPageAction;
        PostPageAction = postPageAction;
        SetSessionValueForGetTest = setSessionValueForGetTest;
        GetSessionValueForPostTest = getSessionValueForPostTest;

        _lazyViewModelAccessors = new Lazy<ViewModelPropertyAccessors<TViewModel, YesNoAnswer?>>(
            () => new ViewModelPropertyAccessors<TViewModel, YesNoAnswer?>(ViewModelYesNoPropertyExpression)
        );
    }

    [TestInitialize]
    public virtual void InitializePageTest()
    {
        SetupBase();
        // _lazyViewModelAccessors.Value can be accessed here if needed for some initial check,
        // otherwise it will be initialized on first use of its properties/methods.
    }

    [TestMethod]
    public virtual async Task GET_BackLinkIsCorrect()
    {
        var orgCreationSession = new OrganisationSession { Journey = JourneyForGetBacklinkTest };
        // Basic validation (optional, for robustness)
        if (JourneyForGetBacklinkTest == null || !JourneyForGetBacklinkTest.Any() || JourneyForGetBacklinkTest.LastOrDefault() != CurrentPagePath)
            throw new InvalidOperationException($"JourneyForGetBacklinkTest for {CurrentPagePath} must be provided, not empty, and end with the CurrentPagePath.");
        if (JourneyForGetBacklinkTest.Count < 2 || JourneyForGetBacklinkTest[^2] != ExpectedBacklinkPagePath)
            throw new InvalidOperationException($"JourneyForGetBacklinkTest for {CurrentPagePath} must have at least two pages, with the second to last being the ExpectedBacklinkPagePath ({ExpectedBacklinkPagePath}).");

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(orgCreationSession);

        var result = await GetPageAction(_systemUnderTest);

        result.Should().NotBeNull().And.BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        AssertBackLink(viewResult, ExpectedBacklinkPagePath);
    }

    [TestMethod]
    [DataRow(true, YesNoAnswer.Yes)]
    [DataRow(false, YesNoAnswer.No)]
    [DataRow(null, null)]
    public virtual async Task GET_CorrectViewModelIsReturnedInTheView(bool? sessionValue, YesNoAnswer? expectedViewModelAnswer)
    {
        var orgCreationSession = new OrganisationSession { Journey = [CurrentPagePath] };
        SetSessionValueForGetTest(orgCreationSession, sessionValue);
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(orgCreationSession);

        var result = await GetPageAction(_systemUnderTest);

        result.Should().NotBeNull().And.BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<TViewModel>();
        var viewModel = (TViewModel)viewResult.Model;
        GetActualViewModelYesNoAnswer(viewModel).Should().Be(expectedViewModelAnswer);
    }

    [TestMethod]
    [DataRow(YesNoAnswer.Yes, true)]
    [DataRow(YesNoAnswer.No, false)]
    public virtual async Task POST_UserSelectsYesOrNo_SessionUpdatedCorrectly(YesNoAnswer userAnswer, bool expectedSessionValue)
    {
        var requestViewModel = new TViewModel();
        SetActualViewModelYesNoAnswer(requestViewModel, userAnswer);

        await PostPageAction(_systemUnderTest, requestViewModel);

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(),
            It.Is<OrganisationSession>(os => GetSessionValueForPostTest(os) == expectedSessionValue)),
            Times.Once);
    }

    [TestMethod]
    public virtual async Task POST_UserSelectsNothing_SessionNotUpdated()
    {
        var requestViewModel = new TViewModel();
        SetActualViewModelYesNoAnswer(requestViewModel, null);
        _systemUnderTest.ModelState.AddModelError(ActualViewModelYesNoPropertyName, ViewModelMissingInputErrorMessage);

        await PostPageAction(_systemUnderTest, requestViewModel);

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Never);
    }

    [TestMethod]
    public virtual async Task POST_UserSelectsNothing_ViewIsReturnedWithCorrectModel()
    {
        var requestViewModel = new TViewModel();
        SetActualViewModelYesNoAnswer(requestViewModel, null);
        _systemUnderTest.ModelState.AddModelError(ActualViewModelYesNoPropertyName, ViewModelMissingInputErrorMessage);

        var result = await PostPageAction(_systemUnderTest, requestViewModel);

        result.Should().NotBeNull().And.BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<TViewModel>();
        var viewModel = (TViewModel)viewResult.Model;
        GetActualViewModelYesNoAnswer(viewModel).Should().BeNull();
    }

    [TestMethod]
    [DataRow(YesNoAnswer.Yes)]
    [DataRow(YesNoAnswer.No)]
    public virtual async Task POST_UserSelectsYesOrNo_UserIsRedirected(YesNoAnswer userAnswer)
    {
        var requestViewModel = new TViewModel();
        SetActualViewModelYesNoAnswer(requestViewModel, userAnswer);
        var expectedRedirect = userAnswer == YesNoAnswer.Yes ? RedirectActionNameOnYes : RedirectActionNameOnNo;

        var result = await PostPageAction(_systemUnderTest, requestViewModel);

        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(expectedRedirect);
    }
}