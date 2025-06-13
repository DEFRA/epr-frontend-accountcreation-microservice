using FluentAssertions;
using FrontendAccountCreation.Web.Extensions;

namespace FrontendAccountCreation.Web.UnitTests.Extensions;

[TestClass]
public class StringExtensionTests
{
    [TestMethod]
    [DataRow(null, null)]
    [DataRow("", "")]
    [DataRow("Bla", "Bla")]
    [DataRow("Controller1", "Controller1")]
    [DataRow("Controller", "")]
    [DataRow("MyController", "My")]
    [DataRow("MyCONTROLLER", "My")]
    public void WithoutControllerSuffix_Returns_Name_With_The_Word_Controller_Stripped_From_Ending(string rawName, string expectedControllerName)
    {
        rawName.WithoutControllerSuffix().Should().Be(expectedControllerName);
    }
}