// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditFinding
//  Layer:          Domain
//  File:           IAuditFindingFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing AuditFinding aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditFinding.Interfaces;

/// <summary>Factory interface for constructing <see cref="AuditFinding"/> aggregates.</summary>
public interface IAuditFindingFactory
{
    /// <summary>Creates a new AuditFinding aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Auditing.AuditFinding.Aggregates.AuditFinding Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}