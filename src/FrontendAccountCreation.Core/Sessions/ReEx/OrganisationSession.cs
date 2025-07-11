﻿using FrontendAccountCreation.Core.Models;
using FrontendAccountCreation.Core.Sessions.Interfaces;

namespace FrontendAccountCreation.Core.Sessions.ReEx;

public class OrganisationSession : ILocalSession
{
    public string? TradingName { get; set; }

    public bool? IsTheOrganisationCharity { get; set; }

    public OrganisationType? OrganisationType { get; set; }

    public Nation? UkNation { get; set; }

    /// <summary>
    /// ReEx Companies-House session
    /// </summary>
    public ReExCompaniesHouseSession? ReExCompaniesHouseSession { get; set; }

    /// <summary>
    /// ReEx Manual input session
    /// </summary>
    public ReExManualInputSession? ReExManualInputSession { get; set; }

    public Contact? Contact { get; set; } = new();

    public string? DeclarationFullName { get; set; }

    public DateTime DeclarationTimestamp { get; set; }

    public bool IsCompaniesHouseFlow => OrganisationType == Sessions.OrganisationType.CompaniesHouseCompany;

    public bool IsApprovedUser { get; set; }

    public bool? IsTradingNameDifferent { get; set; }

    public bool? IsOrganisationAPartnership { get; set; }

    public bool? IsUkMainAddress { get; set; }

    public bool? IsIndividualInCharge { get; set; }

    /// <summary>
    /// Are they individual in-charge of the business
    /// </summary>
    public bool? AreTheyIndividualInCharge { get; set; }

    public bool IsUserChangingDetails { get; set; }

    public YesNoNotSure? UserManagesOrControls { get; set; }

    public YesNoNotSure? TheyManageOrControlOrganisation { get; set; }

    public List<string> Journey { get; set; } = [];

    public HashSet<string> WhiteList { get; set; } = [];

    public InviteUserOptions? InviteUserOption { get; set; }
}