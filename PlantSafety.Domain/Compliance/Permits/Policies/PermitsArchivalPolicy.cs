// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Domain
//  File:           PermitsArchivalPolicy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for Permits aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Permits.Policies;

/// <summary>Determines whether a <see cref="Permits"/> aggregate is eligible for archival.</summary>
public static class PermitsArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the Permits may be archived.</summary>
    public static bool IsEligibleForArchival(Permits aggregate)
        => aggregate.Status == PermitsStatus.Closed;
}
