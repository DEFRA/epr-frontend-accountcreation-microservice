using FluentAssertions;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.LimitedPartnership;

[TestClass]
public class PersonCanNotBeInvitedTests : LimitedPartnershipTestBase
{
    private Guid _testId;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();
        _testId = Guid.NewGuid();
    }

    [TestMethod]
    public void Get_ReturnsViewWithCorrectId()
    {
        // Act
        var result = _systemUnderTest.PersonCanNotBeInvited(_testId);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewModel = ((ViewResult)result).Model.Should().BeOfType<LimitedPartnershipPersonCanNotBeInvitedViewModel>().Subject;
        viewModel.Id.Should().Be(_testId);
    }

    [TestMethod]
    public void Post_RedirectsToCheckYourDetails()
    {
        // Arrange
        var model = new LimitedPartnershipPersonCanNotBeInvitedViewModel { Id = _testId };

        // Act
        var result = _systemUnderTest.PersonCanNotBeInvited(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = (RedirectToActionResult)result;
        redirect.ActionName.Should().Be("CheckYourDetails");
        redirect.ControllerName.Should().Be("AccountCreation");
    }
}