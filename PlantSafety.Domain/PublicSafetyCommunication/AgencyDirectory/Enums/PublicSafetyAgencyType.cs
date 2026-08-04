// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.AgencyDirectory
//  Layer:          Domain
//  File:           PublicSafetyAgencyType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Classification of external public-safety stakeholder agencies that this
//                  module maintains outbound-communication relationships with.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Enums;

/// <summary>Classification of an external public-safety stakeholder agency.</summary>
public enum PublicSafetyAgencyType
{
    /// <summary>Municipal, county, or industrial fire department/brigade.</summary>
    Fire = 0,

    /// <summary>Municipal, county, or state police/sheriff department.</summary>
    Police = 1,

    /// <summary>Emergency Medical Services / ambulance provider.</summary>
    EmergencyMedicalServices = 2,

    /// <summary>Regulatory reporting hotline (e.g., NRC, state environmental agency).</summary>
    RegulatoryHotline = 3,

    /// <summary>Authority operating public alert/warning systems (EAS/WEA/IPAWS).</summary>
    PublicAlertAuthority = 4,

    /// <summary>Hazardous-materials response team.</summary>
    HazmatResponse = 5,

    /// <summary>Regional poison control center.</summary>
    PoisonControl = 6,

    /// <summary>Local emergency management / Local Emergency Planning Committee (LEPC).</summary>
    EmergencyManagementAgency = 7,
}
