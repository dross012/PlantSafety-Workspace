// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePermit
//  Layer:          Domain
//  File:           DischargePermitStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the DischargePermit aggregate root. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Enums;

/// <summary>Lifecycle states for the <see cref="DischargePermit"/> aggregate root.</summary>
public enum DischargePermitStatus
{
    /// <summary>Permit issued and in force.</summary>
    Active = 0,

    /// <summary>Permit approaching its expiration date and requires renewal.</summary>
    ExpiringSoon = 1,

    /// <summary>Permit temporarily suspended by the issuing authority.</summary>
    Suspended = 2,

    /// <summary>Permit lapsed without renewal.</summary>
    Expired = 3,

    /// <summary>Permit permanently revoked by the issuing authority.</summary>
    Revoked = 4,
}