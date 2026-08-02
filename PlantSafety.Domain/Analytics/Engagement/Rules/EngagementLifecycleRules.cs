// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Domain
//  File:           EngagementLifecycleRules.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//                  Additive-only evolution.
//  UPDATED:        2026-08-02 -- Phase 4 full build-out: added CanRecordTouchpoint (Active-only
//                  same-status guard for the non-transitioning RecordTouchpoint lifecycle method)
//                  alongside the pre-existing CanActivate/CanComplete/CanClose/CanArchive, and
//                  wired all five into real aggregate methods (Review/RecordTouchpoint/Complete/
//                  Close/Archive) -- previously these predicates existed but nothing in the
//                  aggregate called them. Mirrors PermitsLifecycleRules.cs's identical precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Engagement.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Engagement"/> aggregate.</summary>
public static class EngagementLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(EngagementStatus current) => current == EngagementStatus.Created;

    /// <summary>Returns <see langword="true"/> when a touchpoint may be recorded while <paramref name="current"/>.</summary>
    public static bool CanRecordTouchpoint(EngagementStatus current) => current == EngagementStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(EngagementStatus current) => current == EngagementStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(EngagementStatus current) => current == EngagementStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(EngagementStatus current) => current == EngagementStatus.Closed;
}
