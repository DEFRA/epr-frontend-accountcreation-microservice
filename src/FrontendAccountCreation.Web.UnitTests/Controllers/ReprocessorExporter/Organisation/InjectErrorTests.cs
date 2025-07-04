
namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class InjectErrorTests : OrganisationTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public void InjectError_ThrowsNotImplementedException()
    {
        // Act & Assert
        Assert.ThrowsException<NotImplementedException>(() => _systemUnderTest.InjectError());
    }
}