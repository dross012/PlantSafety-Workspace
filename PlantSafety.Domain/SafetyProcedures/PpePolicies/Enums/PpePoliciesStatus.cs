// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.PpePolicies
//  Layer:          Domain
//  File:           PpePoliciesStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real document lifecycle states for the PpePolicies aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.PpePolicies.Enums;

/// <summary>Lifecycle states for the <see cref="PpePolicies"/> aggregate root.</summary>
public enum PpePoliciesStatus
{
    /// <summary>Policy document drafted.</summary>
    Draft = 0,

    /// <summary>Submitted and pending approval.</summary>
    PendingApproval = 1,

    /// <summary>Approved and published for enforcement.</summary>
    Published = 2,

    /// <summary>Under revision.</summary>
    UnderRevision = 3,

    /// <summary>Retired/superseded.</summary>
    Retired = 4,
}
