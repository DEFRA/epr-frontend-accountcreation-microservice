using System.Text.Json;
using FrontendAccountCreation.Core.Services;
using FrontendAccountCreation.Core.Services.Dto.CompaniesHouse;
using FrontendAccountCreation.Core.Services.Dto.Company;
using FrontendAccountCreation.Core.Services.FacadeModels;
using FrontendAccountCreation.Core.Sessions;

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

        Assert.AreEqual("KAINOS SOFTWARE LIMITED",organisation.Organisation.Name);
        Assert.AreEqual("BT7 1NT",organisation.Organisation.RegisteredOffice.Postcode);
        Assert.AreEqual("GBR",organisation.Organisation.RegisteredOffice.Country.Iso);
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
}
