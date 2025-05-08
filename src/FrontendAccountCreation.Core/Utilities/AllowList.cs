using System.Collections.Immutable;

namespace FrontendAccountCreation.Core.Utilities;

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
