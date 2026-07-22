// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.ActiveMonitoringAuditing
//  Layer:          Domain
//  File:           ContractorMonitoringCategory.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Classification of active monitoring/auditing observation categories per the
//                  PlantSafety Program Structure Active Monitoring & Auditing component.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Enums;

/// <summary>Classification of active monitoring/auditing observation categories.</summary>
public enum ContractorMonitoringCategory
{
    /// <summary>Routine site visit / safety observation.</summary>
    SiteVisit = 0,

    /// <summary>PPE compliance verification.</summary>
    PpeCompliance = 1,

    /// <summary>Tools, equipment, and LOTO device inspection.</summary>
    EquipmentInspection = 2,

    /// <summary>Verification of adherence to permit conditions.</summary>
    PermitConditionAdherence = 3,
}
