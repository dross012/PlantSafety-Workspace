// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.RiskManagement
//  Layer:          Domain
//  File:           RiskManagementArchivalPolicy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for RiskManagement aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.RiskManagement.Policies;

/// <summary>Determines whether a <see cref="RiskManagement"/> aggregate is eligible for archival.</summary>
public static class RiskManagementArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the RiskManagement may be archived.</summary>
    public static bool IsEligibleForArchival(RiskManagement aggregate)
        => aggregate.Status == RiskManagementStatus.Closed;
}
