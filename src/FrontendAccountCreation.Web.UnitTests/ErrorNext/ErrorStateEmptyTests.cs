using FluentAssertions;
using FrontendAccountCreation.Web.ErrorNext;

namespace FrontendAccountCreation.Web.UnitTests.ErrorNext;

[TestClass]
public class ErrorStateEmptyTests
{
    [TestMethod]
    public void Errors_ShouldBeEmpty()
    {
        // Arrange
        var errorState = new ErrorStateEmpty();

        // Act & Assert
        errorState.Errors.Should().BeEmpty();
    }

    [TestMethod]
    public void HasErrors_ShouldBeFalse()
    {
        // Arrange
        var errorState = new ErrorStateEmpty();

        // Act & Assert
        errorState.HasErrors.Should().BeFalse();
    }

    [TestMethod]
    public void GetErrorIfTriggeredByElementId_ShouldAlwaysReturnNull()
    {
        // Arrange
        var errorState = new ErrorStateEmpty();

        // Act & Assert
        errorState.GetErrorIfTriggeredByElementId().Should().BeNull();
        errorState.GetErrorIfTriggeredByElementId("any").Should().BeNull();
        errorState.GetErrorIfTriggeredByElementId("a", "b").Should().BeNull();
    }

    [TestMethod]
    public void Instance_ShouldReturnSingletonInstance()
    {
        // Act
        var instance1 = ErrorStateEmpty.Instance;
        var instance2 = ErrorStateEmpty.Instance;

        // Assert
        instance1.Should().NotBeNull();
        instance2.Should().NotBeNull();
        instance1.Should().BeSameAs(instance2); // Singleton: both references are the same
    }
}
