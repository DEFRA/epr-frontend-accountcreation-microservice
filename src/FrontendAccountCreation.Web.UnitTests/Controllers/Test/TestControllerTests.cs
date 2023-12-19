using FluentAssertions;
using FrontendAccountCreation.Core.Services;
using FrontendAccountCreation.Web.Controllers.Test;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.Test;

[TestClass]
public class TestControllerTests
{
    private Mock<IFacadeService> _facadeServiceMock = null;
    private TestController _testController;

    [TestInitialize]
    public void SetUp()
    {
        _facadeServiceMock = new Mock<IFacadeService>();
        _facadeServiceMock.Setup(x => x.GetTestMessageAsync());
        _testController = new TestController(_facadeServiceMock.Object);
    }
    
    [TestMethod]
    public async Task Test_ReturnsToCorrectView()
    {
        //Act
        var result = await _testController.Test("/test");
        
        //Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewResult>();
    }
}