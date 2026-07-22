// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.HazardCommunicationRightToKnow
//  Layer:          Domain
//  File:           HazardCommunicationRightToKnowStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real document lifecycle states for the HazardCommunicationRightToKnow aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Enums;

/// <summary>Lifecycle states for the <see cref="HazardCommunicationRightToKnow"/> aggregate root.</summary>
public enum HazardCommunicationRightToKnowStatus
{
    /// <summary>Procedure document drafted.</summary>
    Draft = 0,

    /// <summary>Submitted and pending approval.</summary>
    PendingApproval = 1,

    /// <summary>Approved and published for use.</summary>
    Published = 2,

    /// <summary>Under revision (triggered by scheduled review or regulatory change).</summary>
    UnderRevision = 3,

    /// <summary>Retired/superseded.</summary>
    Retired = 4,
}
