using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.Extensions;

public static class UrlHelperExtension
{
    public static string HomePath(this IUrlHelper url)
    {
        return url.Action("RegisteredAsCharity", "AccountCreation");
    }
    public static string HomePathReEx(this IUrlHelper url)
    {
        return url.Action("ReExAccountFullName", "User");
    }
    public static string CurrentPath(this IUrlHelper url)
    {
        return url.Action(null, "AccountCreation");
    }
    public static string CurrentPathReEx(this IUrlHelper url)
    {
        return url.Action(null, "User");
    }
}