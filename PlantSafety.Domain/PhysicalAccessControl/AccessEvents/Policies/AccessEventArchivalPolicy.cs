// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessEvents
//  Layer:          Domain
//  File:           AccessEventArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival/anonymization eligibility for AccessEvent
//                  aggregates. Raw access events are retained ~1 year then aggregated/
//                  anonymized per data-governance policy.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Policies;

/// <summary>Determines whether an <see cref="AccessEvent"/> aggregate is eligible for archival/anonymization.</summary>
public static class AccessEventArchivalPolicy
{
    /// <summary>Retention window (~1 year) after which raw access events are eligible for aggregation/anonymization.</summary>
    public static readonly TimeSpan RetentionWindow = TimeSpan.FromDays(365);

    /// <summary>Returns <see langword="true"/> when the access event has aged past the retention window and may be archived/anonymized.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates.AccessEvent aggregate, DateTime nowUtc)
        => aggregate.Status == AccessEventStatus.Processed
           && (nowUtc - aggregate.OccurredAtUtc.Value) >= RetentionWindow;
}
