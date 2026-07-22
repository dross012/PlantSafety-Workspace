// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PermitToWork
//  Layer:          Domain
//  File:           PermitToWorkStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real permit lifecycle states for the PermitToWork aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PermitToWork.Enums;

/// <summary>Lifecycle states for the <see cref="PermitToWork"/> aggregate root.</summary>
public enum PermitToWorkStatus
{
    /// <summary>Permit request drafted, hazard assessment not yet recorded.</summary>
    Draft = 0,

    /// <summary>Hazard assessment (and atmospheric testing, if required) recorded.</summary>
    HazardAssessed = 1,

    /// <summary>Permit issued and authorized; work may begin.</summary>
    Issued = 2,

    /// <summary>Permit suspended due to an unsafe condition discovered during work.</summary>
    Suspended = 3,

    /// <summary>Work completed; permit closed out.</summary>
    Closed = 4,

    /// <summary>Permit validity period elapsed without closure.</summary>
    Expired = 5,
}
