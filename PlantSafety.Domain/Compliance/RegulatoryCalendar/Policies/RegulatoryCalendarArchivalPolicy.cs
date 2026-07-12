// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Domain
//  File:           RegulatoryCalendarArchivalPolicy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for RegulatoryCalendar aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.RegulatoryCalendar.Policies;

/// <summary>Determines whether a <see cref="RegulatoryCalendar"/> aggregate is eligible for archival.</summary>
public static class RegulatoryCalendarArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the RegulatoryCalendar may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar aggregate)
        => aggregate.Status == RegulatoryCalendarStatus.Closed;
}
