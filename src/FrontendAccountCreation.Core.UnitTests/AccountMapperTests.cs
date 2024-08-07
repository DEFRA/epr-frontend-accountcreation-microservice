using System.Text.Json;
using FluentAssertions;
using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Services;
using FrontendAccountCreation.Core.Services.Dto.CompaniesHouse;
using FrontendAccountCreation.Core.Services.Dto.Company;
using FrontendAccountCreation.Core.Services.FacadeModels;
using FrontendAccountCreation.Core.Sessions;
using Moq;

namespace FrontendAccountCreation.Core.UnitTests;

[TestClass]
public class AccountMapperTests
{
    [TestMethod]
    public void testCreateAccountModel()
    {
        const string json = @"
        {
            ""organisation"":{
                ""name"":""KAINOS SOFTWARE LIMITED"",
                ""registrationNumber"":""NI019370"",
                ""registeredOffice"":{
                    ""subBuildingName"":null,
                    ""buildingName"":""Kainos House"",
                    ""buildingNumber"":""4-6"",
                    ""street"":""Upper Crescent"",
                    ""town"":""Belfast"",
                    ""county"":null,
                    ""postcode"":""BT7 1NT"",
                    ""locality"":null,
                    ""dependentLocality"":null,
                    ""country"":{
                        ""name"":null,
                        ""iso"":""GBR""
                    },
                    ""isUkAddress"":true
                },
                ""organisationData"":{
                    ""dateOfCreation"":""2023-02-23T15:27:30.681749+00:00"",
                    ""status"":""some-status"",
                    ""type"":""some-type""
                }
            }
        }";

        var organisation = JsonSerializer.Deserialize<CompaniesHouseCompany>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.AreEqual("KAINOS SOFTWARE LIMITED", organisation.Organisation.Name);
        Assert.AreEqual("BT7 1NT", organisation.Organisation.RegisteredOffice.Postcode);
        Assert.AreEqual("GBR", organisation.Organisation.RegisteredOffice.Country.Iso);
        Assert.AreEqual(2023, organisation.Organisation.OrganisationData.DateOfCreation?.Year);
    }

    [TestMethod]
    public void testCreateAccountModel_AsCompaniesHouseCompany_ShouldReturnAccountModelSuccessfully()
    {
        const string json = @"
        {
            ""organisation"":{
                ""name"":""KAINOS SOFTWARE LIMITED"",
                ""registrationNumber"":""NI019370"",
                ""registeredOffice"":{
                    ""subBuildingName"":null,
                    ""buildingName"":""Kainos House"",
                    ""buildingNumber"":""4-6"",
                    ""street"":""Upper Crescent"",
                    ""town"":""Belfast"",
                    ""county"":null,
                    ""postcode"":""BT7 1NT"",
                    ""locality"":null,
                    ""dependentLocality"":null,
                    ""country"":{
                        ""name"":null,
                        ""iso"":""GBR""
                    },
                    ""isUkAddress"":true
                },
                ""organisationData"":{
                    ""dateOfCreation"":""2023-02-23T15:27:30.681749+00:00"",
                    ""status"":""some-status"",
                    ""type"":""some-type""
                }
            }
        }";

        var organisation = JsonSerializer.Deserialize<CompaniesHouseCompany>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.AreEqual("KAINOS SOFTWARE LIMITED", organisation.Organisation.Name);
        Assert.AreEqual("BT7 1NT", organisation.Organisation.RegisteredOffice.Postcode);
        Assert.AreEqual("GBR", organisation.Organisation.RegisteredOffice.Country.Iso);
        Assert.AreEqual(2023, organisation.Organisation.OrganisationData.DateOfCreation?.Year);


        AccountCreationSession accountCreationSession = new AccountCreationSession();
        accountCreationSession.OrganisationType = OrganisationType.CompaniesHouseCompany;
        CompaniesHouseSession companiesHouseSession = new CompaniesHouseSession();
        Company company = new Company(organisation);
        companiesHouseSession.Company = company;
        companiesHouseSession.RoleInOrganisation = RoleInOrganisation.Partner;
        companiesHouseSession.IsComplianceScheme = true;
        accountCreationSession.CompaniesHouseSession = companiesHouseSession;
        AccountMapper accountMapper = new AccountMapper();
        AccountModel accountModel = accountMapper.CreateAccountModel(accountCreationSession, "testaccount@gmail.com");

        Assert.IsNotNull(accountModel);
        Assert.AreEqual("KAINOS SOFTWARE LIMITED", accountModel.Organisation.Name);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Constructor_ShouldThrowArgumentNullException_WhenOrganisationIsNull()
    {
        // Arrange
        CompaniesHouseCompany organisation = null;

        // Act
        new Company(organisation);

        // Assert
        // ExpectedException attribute will handle the assertion
    }

    [TestMethod]
    public void testCreateAccountModel_AsNonCompaniesHouseCompany_ShouldReturnAccountModelSuccessfully()
    {
        const string json = @"
        {
            ""organisation"":{
                ""name"":""KAINOS SOFTWARE LIMITED"",
                ""registrationNumber"":""NI019370"",
                ""registeredOffice"":{
                    ""subBuildingName"":null,
                    ""buildingName"":""Kainos House"",
                    ""buildingNumber"":""4-6"",
                    ""street"":""Upper Crescent"",
                    ""town"":""Belfast"",
                    ""county"":null,
                    ""postcode"":""BT7 1NT"",
                    ""locality"":null,
                    ""dependentLocality"":null,
                    ""country"":{
                        ""name"":null,
                        ""iso"":""GBR""
                    },
                    ""isUkAddress"":true
                },
                ""organisationData"":{
                    ""dateOfCreation"":""2023-02-23T15:27:30.681749+00:00"",
                    ""status"":""some-status"",
                    ""type"":""some-type""
                }
            }
        }";

        var organisation = JsonSerializer.Deserialize<CompaniesHouseCompany>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.AreEqual("KAINOS SOFTWARE LIMITED", organisation.Organisation.Name);
        Assert.AreEqual("BT7 1NT", organisation.Organisation.RegisteredOffice.Postcode);
        Assert.AreEqual("GBR", organisation.Organisation.RegisteredOffice.Country.Iso);
        Assert.AreEqual(2023, organisation.Organisation.OrganisationData.DateOfCreation?.Year);


        AccountCreationSession accountCreationSession = new AccountCreationSession();
        accountCreationSession.OrganisationType = OrganisationType.NonCompaniesHouseCompany;
        ManualInputSession manualInputSession = new ManualInputSession();
        manualInputSession.BusinessAddress = new Addresses.Address();
        manualInputSession.RoleInOrganisation = RoleInOrganisation.Partner.ToString();
        manualInputSession.TradingName = "KAINOS SOFTWARE LIMITED";

        accountCreationSession.ManualInputSession = manualInputSession;
        AccountMapper accountMapper = new AccountMapper();
        AccountModel accountModel = accountMapper.CreateAccountModel(accountCreationSession, "testaccount@gmail.com");

        Assert.IsNotNull(accountModel);
        Assert.AreEqual("KAINOS SOFTWARE LIMITED", accountModel.Organisation.Name);
    }

    [TestMethod]
    public void Separator_ShouldReturnSpace_WhenBuildingNumberIsNotEmpty()
    {
        // Arrange
        var address = new Address
        {
            BuildingNumber = "123"
        };

        // Act
        var separator = address.GetType().GetProperty("Separator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(address, null);

        // Assert
        separator.Should().Be(" ");
    }

    [TestMethod]
    public void Separator_ShouldReturnEmptyString_WhenBuildingNumberIsEmpty()
    {
        // Arrange
        var address = new Address
        {
            BuildingNumber = ""
        };

        // Act
        var separator = address.GetType().GetProperty("Separator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(address, null);

        // Assert
        separator.Should().Be("");
    }

    [TestMethod]
    public void BuildingNumberAndStreet_ShouldReturnFormattedString()
    {
        // Arrange
        var address = new Address
        {
            BuildingNumber = "123",
            Street = "Main St"
        };

        // Act
        var buildingNumberAndStreet = address.GetType().GetProperty("BuildingNumberAndStreet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(address, null);

        // Assert
        buildingNumberAndStreet.Should().Be("123 Main St");
    }

    [TestMethod]
    public void AddressFields_ShouldReturnCorrectArray()
    {
        // Arrange
        var address = new Address
        {
            SubBuildingName = "SubBuilding",
            BuildingName = "Building",
            BuildingNumber = "123",
            Street = "Main St",
            Town = "Town",
            County = "County",
            Postcode = "12345"
        };

        // Act
        var addressFields = address.AddressFields;

        // Assert
        addressFields.Should().BeEquivalentTo(new[] { "SubBuilding", "Building", "123 Main St", "Town", "County", "12345" });
    }

    [TestMethod]
    public void testCreateAccountModel_DeclarationProperties_ShouldReturnSetValues()
    {
        var accountMapper = new AccountMapper();
        var declarationName = "my name";
        var declarationTime = new DateTime(2024, 01, 01);
        var mockSession = new Mock<AccountCreationSession>();
        mockSession.Object.IsApprovedUser = true;
        mockSession.Object.OrganisationType = OrganisationType.CompaniesHouseCompany;

        mockSession.Object.CompaniesHouseSession =
            new CompaniesHouseSession()
            {
                Company = new Company
                {
                    AccountCreatedOn = DateTime.Now,
                    BusinessAddress = new Addresses.Address { BuildingName = "building name" },
                    CompaniesHouseNumber = "123",
                    Name = "unit test name",
                    OrganisationId = "123"
                },
                IsComplianceScheme = false,
                RoleInOrganisation = RoleInOrganisation.Partner
            };

        mockSession.Object.Contact = new Contact() { FirstName = "Firstname", LastName = "Lastname", TelephoneNumber = "0133 256 7845" };

        mockSession.Object.DeclarationFullName = declarationName;
        mockSession.Object.DeclarationTimestamp = declarationTime;

        var result = accountMapper.CreateAccountModel(mockSession.Object, "test@email.com");

        Assert.IsNotNull(result);
        Assert.IsTrue(result.DeclarationFullName.Equals(declarationName));
        Assert.IsTrue(result.DeclarationTimeStamp.Equals(declarationTime));
    }
}
