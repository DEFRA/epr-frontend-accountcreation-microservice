using System.Text.Json;
using FrontendAccountCreation.Core.Services.Dto.Address;

namespace FrontendAccountCreation.Core.UnitTests;

[TestClass]
public class AddressLookupResponseTests
{
    private readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    [TestMethod]
    public void WhenValidAddressLookupResponseJsonIsDeserialized_ThenCompleteObjectIsCreated()
    {
        const string addressJson = @"
        {
            ""Header"": {
                ""Query"": ""postcode=bt480rz"",
                ""Offset"": ""0"",
                ""TotalResults"": ""2"",
                ""Format"": ""JSON"",
                ""Dataset"": ""DPA"",
                ""Lr"": ""EN"",
                ""MaxResults"": ""100"",
                ""Matching_totalresults"": ""2""
              },
            ""Results"": [
                {
                ""Address"": {
                    ""AddressLine"": ""1, GRANGEMORE PARK, DERRY, BT48 0RZ"",
                    ""BuildingNumber"": ""70"",
                    ""Street"": ""GRANGEMORE PARK"",
                    ""Locality"": null,
                    ""Town"": ""DERRY"",
                    ""County"": ""DERRY CITY AND STRABANE"",
                    ""Postcode"": ""BT48 0RZ"",
                    ""Country"": ""NORTHERN IRELAND"",
                    ""XCoordinate"": 58050,
                    ""YCoordinate"": 581607,
                    ""Uprn"": ""185507860"",
                    ""Match"": ""1"",
                    ""MatchDescription"": ""EXACT"",
                    ""Language"": ""EN"",
                    ""SubBuildingName"": null
                }
            },
            {
                ""Address"": {
                    ""AddressLine"": ""2, GRANGEMORE PARK, DERRY, BT48 0RZ"",
                    ""BuildingNumber"": ""70"",
                    ""Street"": ""GRANGEMORE PARK"",
                    ""Locality"": null,
                    ""Town"": ""DERRY"",
                    ""County"": ""DERRY CITY AND STRABANE"",
                    ""Postcode"": ""BT48 0RZ"",
                    ""Country"": ""NORTHERN IRELAND"",
                    ""XCoordinate"": 58050,
                    ""YCoordinate"": 581607,
                    ""Uprn"": ""185507861"",
                    ""Match"": ""1"",
                    ""MatchDescription"": ""EXACT"",
                    ""Language"": ""EN"",
                    ""SubBuildingName"": null
                }
            }]
        }";

        var addressResponse = JsonSerializer.Deserialize<AddressLookupResponse>(addressJson, jsonSerializerOptions);
        Assert.IsNotNull(addressResponse);
        Assert.IsNotNull(addressResponse.Results);
        Assert.IsNotNull(addressResponse.Header);
        Assert.AreEqual(2,addressResponse.Results.Length);
        Assert.AreEqual("100",addressResponse.Header.MaxResults);
        Assert.AreEqual("2",addressResponse.Header.TotalResults);
        Assert.AreEqual("NORTHERN IRELAND",addressResponse.Results[0].Address.Country);
    }

}
