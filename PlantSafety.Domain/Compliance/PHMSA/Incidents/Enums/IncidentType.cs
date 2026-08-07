// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Domain
//  File:           IncidentType.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Incident classification for a DOT Form 5800.1 Hazardous Materials Incident
//                  Report.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Incidents.Enums;

/// <summary>Classification of a hazmat incident reported on DOT Form 5800.1.</summary>
public enum IncidentType
{
    /// <summary>Uncontrolled release of a liquid or solid hazardous material.</summary>
    Spill = 0,

    /// <summary>Uncontrolled release of a gaseous or vapor hazardous material.</summary>
    Leak = 1,

    /// <summary>Fire or explosion involving hazardous material.</summary>
    FireOrExplosion = 2,

    /// <summary>Incident resulted in personal injury.</summary>
    Injury = 3,

    /// <summary>Incident resulted in property damage without a primary release event.</summary>
    PropertyDamage = 4,

    /// <summary>Incident occurred during transport as a vehicle accident.</summary>
    VehicleAccident = 5,

    /// <summary>Any incident that does not fit the other classifications.</summary>
    Other = 6,
}
