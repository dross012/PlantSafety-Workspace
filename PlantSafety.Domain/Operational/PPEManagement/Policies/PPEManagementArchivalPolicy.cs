// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PPEManagement
//  Layer:          Domain
//  File:           PPEManagementArchivalPolicy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for PPEManagement aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PPEManagement.Policies;

/// <summary>Determines whether a <see cref="PPEManagement"/> aggregate is eligible for archival.</summary>
public static class PPEManagementArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the PPEManagement may be archived.</summary>
    public static bool IsEligibleForArchival(PPEManagement aggregate)
        => aggregate.Status == PPEManagementStatus.Closed;
}
