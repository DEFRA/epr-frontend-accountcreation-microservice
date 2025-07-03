using System.Collections.Immutable;

namespace FrontendAccountCreation.Web.ErrorNext;

public static class ErrorDictionaryExtensions
{
    public static ImmutableDictionary<int, PossibleError> Add<T>(
        this ImmutableDictionary<int, PossibleError> dictionary,
        T errorId,
        string errorMessage)
        where T : struct, Enum, IConvertible
    {
        return dictionary.Add((int)(IConvertible)errorId, new PossibleError((int)(IConvertible)errorId, errorMessage));
    }
}