using System.ComponentModel;

namespace FrontendAccountCreation.Core.Sessions;

public enum ServiceRole
{
    [Description("Re-Ex.ApprovedPerson")]
    ApprovedPerson = 0,

    [Description("Re-Ex.DelegatedPerson")]
    DelegatedPerson = 1,

    [Description("Re-Ex.BasicUser")]
    BasicUser = 2,

    [Description("Re-Ex.Admin")]
    Admin = 3
}
