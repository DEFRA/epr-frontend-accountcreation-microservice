using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace FrontendAccountCreation.IntegrationTests.FeatureFlag
{
    [TestFixture]
    public class FeatureFlagTests
    {
        private IConfiguration _configuration;

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ShowYourFeedbackFooter_Is_Displayed(bool value)
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string>
        {
            { Web.Constants.FeatureFlags.ShowYourFeedbackFooter, value.ToString() }
        };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Act
            bool showYourFeedbackFooter = bool.Parse(_configuration[Web.Constants.FeatureFlags.ShowYourFeedbackFooter]);

            // Assert
            showYourFeedbackFooter.Should().Be(value);
        }
    }
}
