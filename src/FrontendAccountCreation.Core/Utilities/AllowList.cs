using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Core.Utilities;

[ExcludeFromCodeCoverage(Justification = "Sonar isn't picking up that this code is covered by the tests in AllowListTests.cs")]
public class AllowList<T>
{
    private readonly ImmutableHashSet<T> _allowList;

    public static AllowList<T> Create(params T[] items)
    {
        return new AllowList<T>(items);
    }

    private AllowList(params T[] items)
    {
        _allowList = ImmutableHashSet.Create(items);
    }

    public bool IsAllowed(T val)
    {
        return _allowList.Contains(val);
    }
}
