﻿using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Core.Services.FacadeModels;

[ExcludeFromCodeCoverage]
public class ReExOrganisationModel
{
    public string? TradingName { get; set; }

    /// <summary>
    /// User Role/Job title can be Director, CompanySecretary, Partner or Member
    /// </summary>
    public string? UserRoleInOrganisation { get; set; }

    public bool IsApprovedUser { get; set; }

    public ReExCompanyModel? Company { get; set; }

    /// <summary>
    /// Used for non-company journey i.e. SoleTrader, partnership
    /// </summary>
    public ReExManualInputModel? ManualInput { get; set; }

    /// <summary>
    /// Approved persons information related to names/emails
    /// </summary>
    public List<ReExInvitedApprovedPerson?> InvitedApprovedPersons { get; set; }

    /// <summary>
    /// Partners Details for partnership org fullName and type
    /// </summary>
    public List<ReExPartnerModel>? Partners { get; set; }
}
