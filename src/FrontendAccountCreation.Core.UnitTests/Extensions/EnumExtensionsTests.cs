using FluentAssertions;
using FrontendAccountCreation.Core.Extensions;
using System;

namespace FrontendAccountCreation.Core.UnitTests.Extensions
{
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
    }
}