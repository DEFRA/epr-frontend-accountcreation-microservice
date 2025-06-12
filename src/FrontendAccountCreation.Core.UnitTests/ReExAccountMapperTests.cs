using FluentAssertions;
using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Services;
using FrontendAccountCreation.Core.Services.Dto.Company;
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
        [DataRow(OrganisationType.NotSet, null, null, null, null, false, Nation.NotSet)]
        [DataRow(OrganisationType.CompaniesHouseCompany, RoleInOrganisation.Director, ReExTeamMemberRole.Director, "Director", "Director", true, Nation.Wales)]
        [DataRow(OrganisationType.CompaniesHouseCompany, RoleInOrganisation.CompanySecretary, ReExTeamMemberRole.CompanySecretary, "CompanySecretary", "CompanySecretary", false, Nation.Scotland)]
        [DataRow(OrganisationType.CompaniesHouseCompany, RoleInOrganisation.Director, ReExTeamMemberRole.Director, "Director", "Director", false, Nation.NorthernIreland)]
        [DataRow(OrganisationType.CompaniesHouseCompany, RoleInOrganisation.CompanySecretary, ReExTeamMemberRole.CompanySecretary, "CompanySecretary", "CompanySecretary", false, Nation.England)]
        public void CreateReExOrganisationModel_Returns_ValidModel_FromOrganisationSession(OrganisationType orgType, RoleInOrganisation? roleInOrg, ReExTeamMemberRole? teamMemberRole, string? expectedRole, string? expectedTeamMember, bool isCompliance, Nation nation)
        {
            // Arrange
            var orgSession = new OrganisationSession
            {
                OrganisationType = orgType,
                IsTheOrganisationCharity = false,
                IsApprovedUser = true,
                ReExCompaniesHouseSession = new ReExCompaniesHouseSession
                {
                    IsComplianceScheme = isCompliance,
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
                ReExManualInputSession = null,
                UkNation = nation,
                DeclarationFullName = "Test 01",
                DeclarationTimestamp = DateTime.UtcNow
            };

            // Act
            var result = _mapper!.CreateReExOrganisationModel(orgSession);

            // Assert
            result.Should().NotBeNull();
            result.Company.OrganisationType.Should().Be(orgType);
            result.UserRoleInOrganisation.Should().Be(expectedRole);
            result.Company.CompanyName.Should().Be("ReEx Test Ltd");
            result.Company.CompaniesHouseNumber.Should().Be("12345678");
            result.Company.CompanyRegisteredAddress.BuildingName.Should().Be("ReEx House");
            result.Company.CompanyRegisteredAddress.Street.Should().Be("High street");
            result.Company.Nation.Should().Be(nation);
            result.Company.ValidatedWithCompaniesHouse.Should().Be(true);
            result.Company.OrganisationId.Should().Be("06352abc-bb77-4855-9705-cf06ae88f5a8");

            result.ManualInput.Should().BeNull();

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
                IsTheOrganisationCharity = false,
                OrganisationType = null,
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
                UkNation = null,
                IsOrganisationAPartnership = false,
                IsTradingNameDifferent = false,
                IsUkMainAddress = true
            };

            // Act
            var result = _mapper!.CreateReExOrganisationModel(orgSession);

            // Assert
            result.Should().NotBeNull();
            result.Company.CompanyRegisteredAddress.Should().BeNull();
            result.Company.CompanyName.Should().BeNull();
            result.Company.CompaniesHouseNumber.Should().BeNull();
            result.Company.ValidatedWithCompaniesHouse.Should().Be(false);
            result.Company.Nation.Should().Be(Nation.NotSet);
            result.InvitedApprovedPersons.Should().NotBeNull();
            result.InvitedApprovedPersons.Should().HaveCount(0);

            result.ManualInput.Should().BeNull();
        }

        [TestMethod]
        [DataRow(ReExTeamMemberRole.Director, "Director", OrganisationType.NonCompaniesHouseCompany, Nation.Scotland)]
        [DataRow(ReExTeamMemberRole.CompanySecretary, "CompanySecretary", OrganisationType.NonCompaniesHouseCompany, Nation.England)]
        [DataRow(ReExTeamMemberRole.IndividualPartner, "IndividualPartner", OrganisationType.NonCompaniesHouseCompany, Nation.Wales)]
        [DataRow(ReExTeamMemberRole.PartnerDirector, "PartnerDirector", OrganisationType.NonCompaniesHouseCompany, Nation.NorthernIreland)]
        [DataRow(ReExTeamMemberRole.None, "None", OrganisationType.NonCompaniesHouseCompany, Nation.NotSet)]
        public void CreateReExOrganisationModel_Returns_Model_FromOrganisationSession_With_ManualInputSession_Data(ReExTeamMemberRole memberRole, string expectedRole, OrganisationType organisationType, Nation nation)
        {
            // Arrange
            var orgSession = new OrganisationSession
            {
                OrganisationType = organisationType,
                ReExCompaniesHouseSession = null,
                IsApprovedUser = false,
                UkNation = nation,
                ReExManualInputSession = new ReExManualInputSession
                {
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
                    },
                    ProducerType = ProducerType.SoleTrader,
                    TeamMember = new ReExCompanyTeamMember
                    {
                        FirstName = "John",
                        LastName = "Smith",
                        Role = memberRole,
                        Email = "john.smith@tester.com",
                        TelephoneNumber = "07880809087"
                    },
                    TradingName = "test sole trader"
                }
            };

            // Act
            var result = _mapper!.CreateReExOrganisationModel(orgSession);

            // Assert
            result.Should().NotBeNull();
            result.Company.Should().BeNull();
            result.ManualInput.BusinessAddress.Should().NotBeNull();
            result.ManualInput.OrganisationType.Should().Be(organisationType);
            result.ManualInput.Nation.Should().Be(nation);
            result.ManualInput.ProducerType.Should().Be(ProducerType.SoleTrader);
            result.ManualInput.TradingName.Should().Be("test sole trader");

            result.InvitedApprovedPersons.Should().HaveCount(1);
            result.InvitedApprovedPersons[0].FirstName.Should().Be("John");
            result.InvitedApprovedPersons[0].LastName.Should().Be("Smith");
        }


        // ***
        [TestMethod]
        public void CreateReprocessorExporterAccountModel_MapsContactCorrectly()
        {
            // Arrange
            var session = new ReExAccountCreationSession
            {
                Contact = new ReExContact
                {
                    FirstName = "John",
                    LastName = "Doe",
                    TelephoneNumber = "1234567890"
                }
            };
            var mapper = new ReExAccountMapper();
            var email = "john.doe@example.com";

            // Act
            var result = mapper.CreateReprocessorExporterAccountModel(session, email);

            // Assert
            result.Should().NotBeNull();
            result.Person.Should().NotBeNull();
            result.Person.FirstName.Should().Be("John");
            result.Person.LastName.Should().Be("Doe");
            result.Person.TelephoneNumber.Should().Be("1234567890");
            result.Person.ContactEmail.Should().Be(email);
        }

        [TestMethod]
        public void CreateReExOrganisationModel_MapsCompanyAndManualInput()
        {
            // Arrange
            var company = new Company
            {
                OrganisationId = "org-123",
                Name = "Test Company",
                CompaniesHouseNumber = "CH123456",
                BusinessAddress = new Address
                {
                    BuildingName = "Building",
                    Street = "Street",
                    Town = "Town",
                    Country = "Country",
                    Postcode = "PC1 1AA"
                }
            };
            var companiesHouseSession = new ReExCompaniesHouseSession
            {
                Company = company,
                RoleInOrganisation = RoleInOrganisation.Director,
                IsComplianceScheme = true,
                TeamMembers = new List<ReExCompanyTeamMember>
            {
                new ReExCompanyTeamMember
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Alice",
                    LastName = "Smith",
                    Email = "alice@example.com",
                    Role = ReExTeamMemberRole.CompanySecretary,
                    TelephoneNumber = "111222333"
                }
            }
            };
            var manualInputSession = new ReExManualInputSession
            {
                TradingName = "Trading",
                ProducerType = ProducerType.SoleTrader,
                BusinessAddress = new Address
                {
                    BuildingName = "Manual",
                    Street = "Manual Street",
                    Town = "Manual Town",
                    Country = "Manual Country",
                    Postcode = "PC2 2BB"
                },
                TeamMember = new ReExCompanyTeamMember
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Bob",
                    LastName = "Brown",
                    Email = "bob@example.com",
                    Role = ReExTeamMemberRole.Director,
                    TelephoneNumber = "444555666"
                }
            };
            var session = new OrganisationSession
            {
                ReExCompaniesHouseSession = companiesHouseSession,
                ReExManualInputSession = manualInputSession,
                IsApprovedUser = true,
                OrganisationType = OrganisationType.CompaniesHouseCompany,
                UkNation = Nation.England
            };
            var mapper = new ReExAccountMapper();

            // Act
            var result = mapper.CreateReExOrganisationModel(session);

            // Assert
            result.Should().NotBeNull();
            result.UserRoleInOrganisation.Should().Be("Director");
            result.IsApprovedUser.Should().BeTrue();
            result.Company.Should().NotBeNull();

            result.Company.OrganisationId.Should().Be("org-123");
            result.Company.CompanyName.Should().Be("Test Company");
            result.Company.CompaniesHouseNumber.Should().Be("CH123456");
            result.Company.CompanyRegisteredAddress.Should().NotBeNull();
            result.Company.CompanyRegisteredAddress.BuildingName.Should().Be("Building");
            result.ManualInput.TradingName.Should().Be("Trading");
            result.ManualInput.ProducerType.Should().Be(ProducerType.SoleTrader);
            result.InvitedApprovedPersons.Should().NotBeNull();
        }

        [TestMethod]
        public void CreateReExOrganisationModel_ReturnsNulls_WhenNoCompanyOrManualInput()
        {
            // Arrange
            var session = new OrganisationSession
            {
                IsApprovedUser = false
            };
            var mapper = new ReExAccountMapper();

            // Act
            var result = mapper.CreateReExOrganisationModel(session);

            // Assert
            result.Should().NotBeNull();
            result.UserRoleInOrganisation.Should().BeNull();
            result.IsApprovedUser.Should().BeFalse();
            result.Company.Should().BeNull();
            result.ManualInput?.Should().BeNull();
            result.InvitedApprovedPersons?.Should().NotBeNull().And.BeEmpty();
        }
    }
}