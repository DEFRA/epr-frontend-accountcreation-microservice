using FluentAssertions;
using FrontendAccountCreation.Core.Services;
using FrontendAccountCreation.Core.Services.FacadeModels;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;

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

        [TestMethod]
        public void CreateReExOrganisationModel_Returns_ValidModel_FromOrganisationSession()
        {
            // Arrange
            var orgSession = new OrganisationSession
            {
                OrganisationType = OrganisationType.CompaniesHouseCompany,
                ReExCompaniesHouseSession = new ReExCompaniesHouseSession
                {
                    Company = new Services.Dto.Company.Company
                    {
                        AccountCreatedOn = DateTime.Now,
                        Name = "ReEx Test Ltd",
                        CompaniesHouseNumber = "12345678",
                        OrganisationId = "06352abc-bb77-4855-9705-cf06ae88f5a8",
                        BusinessAddress = new Addresses.Address
                        {
                            BuildingName = "ReEx House",
                            BuildingNumber = "14",
                            Street = "High street",
                            Town = "Lodnon",
                            Postcode = "E10 6PN",
                            Locality = "XYZ",
                            DependentLocality = "ABC",
                            County = "London",
                            Country = "England"
                        }
                    }
                },
                UkNation = Nation.England                
            };

            // Act
            var result = _mapper.CreateReExOrganisationModel(orgSession);

            // Assert
            result.Should().NotBeNull();
            result.CompanyName.Should().Be("ReEx Test Ltd");
            result.CompaniesHouseNumber.Should().Be("12345678");
            result.CompanyAddress.BuildingName.Should().Be("ReEx House");
            result.CompanyAddress.Street.Should().Be("High street");
            result.Nation.Should().Be(Nation.England);
            result.ValidatedWithCompaniesHouse.Should().Be(true);
            result.OrganisationId.Should().Be("06352abc-bb77-4855-9705-cf06ae88f5a8");
        }

        [TestMethod]
        public void CreateReExOrganisationModel_Returns_Model_FromOrganisationSession_WithoutAddress()
        {
            // Arrange
            var orgSession = new OrganisationSession
            {
                OrganisationType = OrganisationType.CompaniesHouseCompany,
                ReExCompaniesHouseSession = new ReExCompaniesHouseSession
                {
                    Company = new Services.Dto.Company.Company
                    {
                        AccountCreatedOn = DateTime.Now,
                        BusinessAddress = null
                    }
                },
                UkNation = Nation.NotSet
            };

            // Act
            var result = _mapper.CreateReExOrganisationModel(orgSession);

            // Assert
            result.Should().NotBeNull();
            result.CompanyAddress.Should().BeNull();
            result.ValidatedWithCompaniesHouse.Should().Be(false);
            result.Nation.Should().Be(Nation.NotSet);
        }
    }
}