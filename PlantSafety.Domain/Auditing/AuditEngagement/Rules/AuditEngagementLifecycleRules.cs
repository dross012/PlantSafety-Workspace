// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement
//  Layer:          Domain
//  File:           AuditEngagementLifecycleRules.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the AuditEngagement aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditEngagement.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="AuditEngagement"/> aggregate.</summary>
public static class AuditEngagementLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(AuditEngagementStatus current) => current == AuditEngagementStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(AuditEngagementStatus current) => current == AuditEngagementStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(AuditEngagementStatus current) => current == AuditEngagementStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(AuditEngagementStatus current) => current == AuditEngagementStatus.Closed;
}