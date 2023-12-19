using FrontendAccountCreation.Core.Validations;

namespace FrontendAccountCreation.IntegrationTests.Validations;

[TestClass]
public class TelephoneNumberValidatorTests
{
    [TestMethod]
    [DataRow("020 1212 1212")]
    [DataRow("078 1212 1212")]
    [DataRow("78 1212 1212")]
    [DataRow("+44 078 1212 1212")]
    [DataRow("+44 78 1212 1212")]
    [DataRow("(+44) 78 1212 1212")]
    [DataRow("0044 078 1212 1212")]
    [DataRow("02012121212")]
    [DataRow("07812121212")]
    [DataRow("7812121212")]
    [DataRow("+4407812121212")]
    [DataRow("+447812121212")]
    [DataRow("004407812121212")]
    [DataRow("+49 30 901820")]
    [DataRow("+34919931307")]
    public void GivenValidUKNumberProvided_ServiceShouldReturnTrue(string phoneNumber)
    {
        Assert.IsTrue(TelephoneNumberValidator.IsValid(phoneNumber));
    }
    
    [TestMethod]
    [DataRow("020 1212 121")]
    [DataRow("020 1212 121")]
    [DataRow("078 1212 121A")]
    [DataRow("")]
    [DataRow("a")]
    [DataRow("800 890 567sad")]
    [DataRow("800 890 567123")]
    [DataRow("asd800 890 567")]
    [DataRow("123800 890 567")]
    [DataRow("07812121212!!")]
    [DataRow("..07812121212")]
    [DataRow("!@£$%800 890 567")]
    [DataRow("072121212^&*()_+")]
    [DataRow("0721^&*()_+2121")]
    [DataRow("078 1(212 12)1A")]
    public void GivenInvalidUKNumberProvided_ServiceShouldReturnFalse(string phoneNumber)
    {
        Assert.IsFalse(TelephoneNumberValidator.IsValid(phoneNumber));
    }
}