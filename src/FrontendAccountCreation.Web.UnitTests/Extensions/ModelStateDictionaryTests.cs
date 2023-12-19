using FrontendAccountCreation.Web.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FrontendAccountCreation.Web.UnitTests.Extensions;

[TestClass]
public class ModelStateDictionaryTests
{
    [TestMethod]
    public void ToErrorDictionary_GivenModelErrors_ReturnsDictionaryOfSizeTwo()
    {
        // Arrange
        var modelDictionary = new ModelStateDictionary
        {
            MaxAllowedErrors = 5
        };
        
        modelDictionary.AddModelError("Err1","TestErr1");
        modelDictionary.AddModelError("Err1","TestErr2");
        modelDictionary.AddModelError("Err2","TestErr3");
        
        // Act
        var result =  modelDictionary.ToErrorDictionary();
        
        // Assert
        Assert.AreEqual(expected: 2, result.Count);
    }
}