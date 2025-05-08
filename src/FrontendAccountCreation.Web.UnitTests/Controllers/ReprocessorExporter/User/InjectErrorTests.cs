using FluentAssertions;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.User
{
    [TestClass]
    public class InjectErrorTests : UserTestBase
    {
        [TestInitialize]
        public void Setup()
        {
            SetupBase();
        }

        [TestMethod]
        public void InjectError_ShouldThrowNotImplementedException()
        {
            // Act
            Action act = () => _systemUnderTest.InjectError();

            // Assert
            act.Should().Throw<NotImplementedException>();
        }
    }
}