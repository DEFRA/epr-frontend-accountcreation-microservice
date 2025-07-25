﻿using FrontendAccountCreation.Core.Services.Dto.Company;
using FrontendAccountCreation.Core.Sessions.ReEx.Partnership;

namespace FrontendAccountCreation.Core.Sessions.ReEx;

public class ReExCompaniesHouseSession
{
    public Company Company { get; set; }

    public RoleInOrganisation? RoleInOrganisation { get; set; }

    public bool IsComplianceScheme { get; set; }

    public List<ReExCompanyTeamMember>? TeamMembers { get; set; }

    public ReExPartnership? Partnership { get; set; }

    public bool IsInEligibleToBeApprovedPerson { get; set; }

    public ProducerType? ProducerType { get; set; }
}
