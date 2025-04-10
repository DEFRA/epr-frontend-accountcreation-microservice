using FluentAssertions;
using FrontendAccountCreation.Core.Services;
using FrontendAccountCreation.Core.Sessions;

namespace FrontendAccountCreation.Core.UnitTests
{
    [TestClass]
    public class ReExAccountMapperTests
    {
        private ReExAccountMapper? _mapper;

        [TestInitialize]
        public void Setup()
        {
            _mapper = new ReExAccountMapper();
        }

        [TestMethod]
        public void CreateReprocessorExporterAccountModel_ShouldReturnValidModel_WhenSessionIsValid()
        {
            // Arrange
            const string email = "john.doe@example.com";

            var session = new ReExAccountCreationSession
            {
                Contact = new ReExContact
                {
                    FirstName = "John",
                    LastName = "Doe",
                    TelephoneNumber = "123456789"
                }
            };
 
            // Act
            var result = _mapper!.CreateReprocessorExporterAccountModel(session, email);

            // Assert
            result.Should().NotBeNull();
            result.Person.Should().NotBeNull();
            result.Person.FirstName.Should().Be("John");
            result.Person.LastName.Should().Be("Doe");
            result.Person.ContactEmail.Should().Be(email);
            result.Person.TelephoneNumber.Should().Be("123456789");
        }

        //todo: check throws?
        //[TestMethod]
        //public void CreateReprocessorExporterAccountModel_ShouldHandleNullContact_WhenSessionContactIsNull()
        //{
        //    // Arrange
        //    var session = new ReExAccountCreationSession
        //    {
        //        Contact = null
        //    };
        //    var email = "john.doe@example.com";

        //    // Act
        //    var result = _mapper!.CreateReprocessorExporterAccountModel(session, email);

        //    // Assert
        //    result.Should().NotBeNull();
        //    result.Person.Should().NotBeNull();
        //    result.Person.FirstName.Should().BeNull();
        //    result.Person.LastName.Should().BeNull();
        //    result.Person.ContactEmail.Should().Be(email);
        //    result.Person.TelephoneNumber.Should().BeNull();
        //}
    }
}