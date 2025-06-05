using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.Extensions;

public static class UrlHelperExtension
{
    public static string HomePath(this IUrlHelper url)
    {
        return url.Action("RegisteredAsCharity", "AccountCreation");
    }
    public static string HomePathReExUser(this IUrlHelper url)
    {
        return url.Action("ReExAccountFullName", "User");
    }
    public static string HomePathReExOrganisation(this IUrlHelper url)
    {
        return url.Action("RegisteredAsCharity", "Organisation");
    }
    public static string CurrentPath(this IUrlHelper url)
    {
        return url.Action(null, "AccountCreation");
    }
    public static string CurrentPathReExUser(this IUrlHelper url)
    {
        return url.Action(null, "User");
    }
    public static string CurrentPathReExOrganisation(this IUrlHelper url)
    {
        return url.Action(null, "Organisation");
    }
}