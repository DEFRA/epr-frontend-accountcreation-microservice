using FluentAssertions;
using FrontendAccountCreation.Core.Extensions;

namespace FrontendAccountCreation.Core.UnitTests.Extensions;

public enum Test
{
    [System.ComponentModel.Description("First Value")]
    First,

    [System.ComponentModel.Description("Second Value")]
    Second,

    Third // No description
}

[TestClass]
public class EnumExtensionsTests
{
    [TestMethod]
    [DataRow(Test.First, "First Value")]
    [DataRow(Test.Second, "Second Value")]
    [DataRow(Test.Third, "Third")]
    public void GetDescription_ReturnsExpectedDescription(Test value, string expected)
    {
        // Act
        var description = value.GetDescription();

        // Assert
        description.Should().Be(expected);
    }

    [TestMethod]
    [DataRow(Test.First, "First Value")]
    [DataRow(Test.Second, "Second Value")]
    [DataRow(Test.Third, null)]
    public void GetDescriptionOrNull_ReturnsExpectedDescription(Test value, string expected)
    {
        // Act
        var description = value.GetDescriptionOrNull();

        // Assert
        description.Should().Be(expected);
    }

    [TestMethod]
    public void GetDescription_WithInvalidEnumValue_ReturnsEnumToString()
    {
        Enum value = (Test)999;
        var description = value.GetDescription();
        description.Should().Be("999");
    }

    [TestMethod]
    public void GetDescriptionOrNull_WithInvalidEnumValue_ReturnsNull()
    {
        Enum value = (Test)999;
        var description = value.GetDescriptionOrNull();
        description.Should().BeNull();
    }

    [TestMethod]
    public void GetDescriptionOrNull_WithNullEnum_ThrowsNullReferenceException()
    {
        Enum value = null;
        Action act = () => value.GetDescriptionOrNull();
        act.Should().Throw<NullReferenceException>();
    }

    [TestMethod]
    public void GetDescription_WithNullEnum_ThrowsNullReferenceException()
    {
        Enum value = null;
        Action act = () => value.GetDescription();
        act.Should().Throw<NullReferenceException>();
    }
}