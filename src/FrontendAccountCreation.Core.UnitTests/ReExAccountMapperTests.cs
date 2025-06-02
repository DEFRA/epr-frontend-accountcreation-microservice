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
        [DataRow(null, null, null, null)]
        [DataRow(RoleInOrganisation.Director, ReExTeamMemberRole.Director, "Director", "Director")]
        [DataRow(RoleInOrganisation.Director, ReExTeamMemberRole.CompanySecretary, "Director", "CompanySecretary")]
        public void CreateReExOrganisationModel_Returns_ValidModel_FromOrganisationSession(RoleInOrganisation? roleInOrg, ReExTeamMemberRole? teamMemberRole, string? expectedRole, string? expectedTeamMember)
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
                    },
                    TeamMembers =
                    [
                        new() {
                            FirstName = "John",
                            LastName = "Smith",
                            Role = teamMemberRole,
                            Email = "john.smith@tester.com",
                            TelephoneNumber = "07880809087"
                        },
                        new() {
                            FirstName = "Jill",
                            LastName = "Handerson",
                            Role = ReExTeamMemberRole.CompanySecretary,
                            Email = "jill.henderson.smith@tester.com",
                            TelephoneNumber = "07880809088"
                        }
                    ],
                    RoleInOrganisation = roleInOrg
                },
                UkNation = Nation.England                
            };

            // Act
            var result = _mapper.CreateReExOrganisationModel(orgSession);

            // Assert
            result.Should().NotBeNull();
            result.UserRoleInOrganisation.Should().Be(expectedRole);
            result.Company.CompanyName.Should().Be("ReEx Test Ltd");
            result.Company.CompaniesHouseNumber.Should().Be("12345678");
            result.Company.CompanyRegisteredAddress.BuildingName.Should().Be("ReEx House");
            result.Company.CompanyRegisteredAddress.Street.Should().Be("High street");
            result.Company.Nation.Should().Be(Nation.England);
            result.Company.ValidatedWithCompaniesHouse.Should().Be(true);
            result.Company.OrganisationId.Should().Be("06352abc-bb77-4855-9705-cf06ae88f5a8");            
           
            // Assert collection
            result.InvitedApprovedPersons.Should().NotBeNull();
            result.InvitedApprovedPersons.Should().HaveCount(2);
            result.InvitedApprovedPersons.Should().SatisfyRespectively(
                first =>
                {
                    first.FirstName.Should().Be("John");
                    first.LastName.Should().Be("Smith");
                    first.Role.Should().Be(expectedTeamMember);
                    first.Email.Should().NotBeNullOrWhiteSpace();
                },
                second =>
                {
                    second.FirstName.Should().Be("Jill");
                    second.LastName.Should().Be("Handerson");
                    second.Role.Should().Be("CompanySecretary");
                    second.Email.Should().NotBeNullOrWhiteSpace();
                });
        }

        [TestMethod]
        public void CreateReExOrganisationModel_Returns_Model_FromOrganisationSession_WithoutAddress()
        {
            // Arrange
            var orgSession = new OrganisationSession
            {
                OrganisationType = null, // OrganisationType.CompaniesHouseCompany,
                ReExCompaniesHouseSession = new ReExCompaniesHouseSession
                {
                    Company = new Services.Dto.Company.Company
                    {
                        Name = null,
                        CompaniesHouseNumber = null,
                        OrganisationId = null,                 
                        AccountCreatedOn = DateTime.Now,
                        BusinessAddress = null
                    },
                    RoleInOrganisation = null                    
                },
                IsApprovedUser = false,
                UkNation = null
            };

            // Act
            var result = _mapper.CreateReExOrganisationModel(orgSession);

            // Assert
            result.Should().NotBeNull();
            result.Company.CompanyRegisteredAddress.Should().BeNull();
            result.Company.CompanyName.Should().BeNull();
            result.Company.CompaniesHouseNumber.Should().BeNull();
            result.Company.ValidatedWithCompaniesHouse.Should().Be(false);
            result.Company.Nation.Should().Be(Nation.NotSet);
            result.InvitedApprovedPersons.Should().NotBeNull();
            result.InvitedApprovedPersons.Should().HaveCount(0);
        }
    }
}