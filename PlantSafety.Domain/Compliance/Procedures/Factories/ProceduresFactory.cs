// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Domain
//  File:           ProceduresFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing Procedures aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Procedures.Factories;

/// <summary>Default factory implementation for constructing <see cref="Procedures"/> aggregates.</summary>
public sealed class ProceduresFactory : IProceduresFactory
{
    /// <inheritdoc/>
    public Procedures Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => Procedures.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
