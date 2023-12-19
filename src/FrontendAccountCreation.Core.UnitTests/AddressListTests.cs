using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Services.Dto.Address;
using System.Text.Json;

namespace FrontendAccountCreation.Core.UnitTests;

[TestClass]
public class AddressListTests
{
    [TestMethod]
    public void WhenValidAddressLookupResponseJsonIsDeserialized_ThenCompleteObjectIsCreated_ThenCreateAddressListWithTheResponse()
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

        var addressResponse = JsonSerializer.Deserialize<AddressLookupResponse>(addressJson, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        AddressList addressList = new AddressList(addressResponse);
        Assert.AreEqual(true, addressList.Addresses.Count == 2);

    }

    [TestMethod]
    public void WhenAddressLookupResponseJsonNull_ThenThrowsException()
    {
        //Arrange
        Exception expectedException = null;
        //Act
        try
        {
            AddressList addressList = new AddressList(null);
        }
        catch (Exception e)
        {
            expectedException = e;
        }
        
        
        //Assert
        Assert.IsNotNull(expectedException);
    }
}
