namespace FrontendAccountCreation.Core.Constants;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public static class EnrolmentStatus
{
    public const string NotSet = "Not Set";
    public const string Enrolled = "Enrolled";
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Invited = "Invited";
    public const string OnHold = "On Hold";
    public const string Nominated = "Nominated";
}