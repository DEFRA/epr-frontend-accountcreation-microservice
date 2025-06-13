using FrontendAccountCreation.Core.Sessions;
namespace FrontendAccountCreation.Core.Services;

public static class NationMapper
{

    // Optional: Custom code mapping
    private static readonly Dictionary<string, Nation> CustomCodeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "England", Nation.England },
        { "Scotland", Nation.Scotland },
        { "Wales", Nation.Wales },
        { "Northern Ireland", Nation.NorthernIreland }
    };

    public static bool TryMapToNation(string input, out Nation nation)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            nation = default;
            return false;
        }
        // Check if input is a custom code
        if (CustomCodeMap.TryGetValue(input, out nation))
            return true;
        // Check if input is a number
        if (int.TryParse(input, out int intValue) && Enum.IsDefined(typeof(Nation), intValue))
        {
            nation = (Nation)intValue;
            return true;
        }
        // Check if input is a valid enum name
        if (Enum.TryParse(input, true, out nation) && Enum.IsDefined(typeof(Nation), nation))
            return true;
 
        nation = default;
        return false;
    }
}