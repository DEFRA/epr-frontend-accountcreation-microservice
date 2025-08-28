using System.Text.Json;

using FrontendAccountCreation.Core.Services.Dto.CompaniesHouse;

namespace FrontendAccountCreation.Core.UnitTests;

[TestClass]
public class CompaniesHouseOrganisationTests
{
    private readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    [TestMethod]
    public void WhenValidOrganisationJsonIsDeserialized_ThenCompleteObjectIsCreated()
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

        var organisation = JsonSerializer.Deserialize<CompaniesHouseCompany>(json, jsonSerializerOptions);

        Assert.AreEqual("KAINOS SOFTWARE LIMITED",organisation.Organisation.Name);
        Assert.AreEqual("BT7 1NT",organisation.Organisation.RegisteredOffice.Postcode);
        Assert.AreEqual("GBR",organisation.Organisation.RegisteredOffice.Country.Iso);
        Assert.AreEqual(2023, organisation.Organisation.OrganisationData.DateOfCreation?.Year);
    }
}
