// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitExpiration
//  Layer:          Domain
//  File:           PermitExpirationArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for PermitExpiration aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitExpiration.Policies;

/// <summary>Determines whether a <see cref="PermitExpiration"/> aggregate is eligible for archival.</summary>
public static class PermitExpirationArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the PermitExpiration may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates.PermitExpiration aggregate)
        => aggregate.Status == PermitExpirationStatus.Closed;
}