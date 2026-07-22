// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           AccidentArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for Accident aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.AccidentManagement.Aggregates;
using PlantSafety.Domain.AccidentManagement.Enums;

namespace PlantSafety.Domain.AccidentManagement.Policies;

/// <summary>Determines whether a Accident aggregate is eligible for archival.</summary>
public static class AccidentArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the Accident may be archived.</summary>
    public static bool IsEligibleForArchival(Accident aggregate)
        => aggregate.Status == AccidentStatus.Closed;
}