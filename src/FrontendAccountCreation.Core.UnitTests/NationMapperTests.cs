using FrontendAccountCreation.Core.Services;
using FrontendAccountCreation.Core.Sessions;

namespace FrontendAccountCreation.Core.UnitTests
{
    [TestClass]
    public class NationMapperTests
    {
        [DataTestMethod]
        [DataRow("England", Nation.England)]
        [DataRow("Scotland", Nation.Scotland)]
        [DataRow("Wales", Nation.Wales)]
        [DataRow("Northern Ireland", Nation.NorthernIreland)]
        [DataRow("1", Nation.England)]
        [DataRow("2", Nation.NorthernIreland)]
        [DataRow("3", Nation.Scotland)]
        [DataRow("4", Nation.Wales)]
        [DataRow("0", Nation.NotSet)]

        public void TryMapToNation_ValidInput_ReturnsTrueAndCorrectEnum(string input, Nation expected)
        {
            // Act
            var result = NationMapper.TryMapToNation(input, out Nation actual);
            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(expected, actual);
        }

        [DataTestMethod]
        [DataRow("XYZ")]
        [DataRow("5")]
        [DataRow("5.50")]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("Great Britain")]
        [DataRow("Not specified")]
        [DataRow("United Kingdom")]
        [DataRow(null)]
        public void TryMapToNation_InvalidInput_ReturnsFalseAndDefaultEnum(string input)
        {
            // Act
            var result = NationMapper.TryMapToNation(input, out Nation actual);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(default(Nation), actual); // default should be 0
        }
    }
}