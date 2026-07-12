// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Domain
//  File:           IAuditingFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing Auditing aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Auditing.Interfaces;

/// <summary>Factory interface for constructing <see cref="Auditing"/> aggregates.</summary>
public interface IAuditingFactory
{
    /// <summary>Creates a new Auditing aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
