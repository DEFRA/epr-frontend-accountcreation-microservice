using FluentAssertions;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.ApprovedPerson;

[TestClass]
public class PersonCanNotBeInvitedTests : ApprovedPersonTestBase
{
    private Guid _testId;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();
        _testId = Guid.NewGuid();
    }

    [TestMethod]
    public async Task Get_ReturnsViewWithCorrectId()
    {
        // Act
        var result = await _systemUnderTest.PersonCanNotBeInvited(_testId);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewModel = ((ViewResult)result).Model.Should().BeOfType<ApprovedPersonCanNotBeInvitedViewModel>().Subject;
        viewModel.Id.Should().Be(_testId);
    }

    [TestMethod]
    public async Task Post_RedirectsToCheckYourDetails()
    {
        // Arrange
        var model = new ApprovedPersonCanNotBeInvitedViewModel { Id = _testId };

        // Act
        var result = await _systemUnderTest.PersonCanNotBeInvited(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = result as RedirectToActionResult;
        redirect!.ActionName.Should().Be("CheckYourDetails");
    }

    [TestMethod]
    public async Task PersonCanNotBeInvited_Post_ReturnsRedirectToCheckYourDetails()
    {
        // Arrange
        var model = new ApprovedPersonCanNotBeInvitedViewModel
        {
            Id = Guid.NewGuid()
        };

        // Act
        var result = await _systemUnderTest.PersonCanNotBeInvited(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = result as RedirectToActionResult;
        redirect!.ActionName.Should().Be("CheckYourDetails");
    }
}