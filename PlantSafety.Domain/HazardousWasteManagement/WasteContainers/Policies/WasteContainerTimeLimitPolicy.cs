// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteContainers
//  Layer:          Domain
//  File:           WasteContainerTimeLimitPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy encoding the RCRA-style 90/180-day accumulation time-limit
//                  invariant and the escalation threshold for "approaching" warnings.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Aggregates;

namespace PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Policies;

/// <summary>Encodes RCRA-style accumulation time-limit compliance rules for WasteContainer.</summary>
public static class WasteContainerTimeLimitPolicy
{
    /// <summary>Number of days before the regulatory deadline that a container is considered "approaching" its time limit.</summary>
    public const int ApproachingWarningWindowDays = 10;

    /// <summary>Returns <see langword="true"/> when the container is within the warning window of its regulatory deadline.</summary>
    public static bool IsApproachingTimeLimit(WasteContainer container, DateTime nowUtc)
        => container.TimeLimitDeadlineUtc().AddDays(-ApproachingWarningWindowDays) <= nowUtc
           && !container.IsPastTimeLimit(nowUtc);

    /// <summary>Returns <see langword="true"/> when the container has breached its regulatory time limit and must escalate to RiskManagement.</summary>
    public static bool RequiresRiskEscalation(WasteContainer container, DateTime nowUtc)
        => container.IsPastTimeLimit(nowUtc);
}
