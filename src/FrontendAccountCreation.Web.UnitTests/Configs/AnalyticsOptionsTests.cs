using Microsoft.VisualStudio.TestTools.UnitTesting;
using FrontendAccountCreation.Web.Configs;

namespace FrontendAccountCreation.Tests.Configs
{
    [TestClass]
    public class AnalyticsOptionsTests
    {
        [TestMethod]
        public void DefaultCookieName_ShouldReturnCookiePrefix()
        {
            // Arrange
            var expectedPrefix = "TestPrefix";
            var options = new AnalyticsOptions
            {
                CookiePrefix = expectedPrefix
            };

            // Act
            var defaultCookieName = options.DefaultCookieName;

            // Assert
            Assert.AreEqual(expectedPrefix, defaultCookieName);
        }

        [TestMethod]
        public void AdditionalCookieName_ShouldReturnFormattedString()
        {
            // Arrange
            var expectedPrefix = "TestPrefix";
            var expectedMeasurementId = "TestMeasurementId";
            var options = new AnalyticsOptions
            {
                CookiePrefix = expectedPrefix,
                MeasurementId = expectedMeasurementId
            };

            // Act
            var additionalCookieName = options.AdditionalCookieName;

            // Assert
            var expectedAdditionalCookieName = $"{expectedPrefix}_{expectedMeasurementId}";
            Assert.AreEqual(expectedAdditionalCookieName, additionalCookieName);
        }

        [TestMethod]
        public void AdditionalCookieName_ShouldHandleNullMeasurementId()
        {
            // Arrange
            var expectedPrefix = "TestPrefix";
            var options = new AnalyticsOptions
            {
                CookiePrefix = expectedPrefix,
                MeasurementId = null
            };

            // Act
            var additionalCookieName = options.AdditionalCookieName;

            // Assert
            var expectedAdditionalCookieName = $"{expectedPrefix}_";
            Assert.AreEqual(expectedAdditionalCookieName, additionalCookieName);
        }
    }
}
