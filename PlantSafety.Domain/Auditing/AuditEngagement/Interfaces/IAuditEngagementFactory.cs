// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement
//  Layer:          Domain
//  File:           IAuditEngagementFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing AuditEngagement aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditEngagement.Interfaces;

/// <summary>Factory interface for constructing <see cref="AuditEngagement"/> aggregates.</summary>
public interface IAuditEngagementFactory
{
    /// <summary>Creates a new AuditEngagement aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}