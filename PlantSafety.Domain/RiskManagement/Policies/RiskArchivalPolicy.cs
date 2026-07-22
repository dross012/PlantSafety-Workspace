// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           RiskArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for Risk aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.RiskManagement.Aggregates;
using PlantSafety.Domain.RiskManagement.Enums;

namespace PlantSafety.Domain.RiskManagement.Policies;

/// <summary>Determines whether a Risk aggregate is eligible for archival.</summary>
public static class RiskArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the Risk may be archived.</summary>
    public static bool IsEligibleForArchival(Risk aggregate)
        => aggregate.Status == RiskStatus.Closed;
}