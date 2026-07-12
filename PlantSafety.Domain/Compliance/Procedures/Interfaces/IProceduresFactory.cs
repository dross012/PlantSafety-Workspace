// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Domain
//  File:           IProceduresFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing Procedures aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Procedures.Interfaces;

/// <summary>Factory interface for constructing <see cref="Procedures"/> aggregates.</summary>
public interface IProceduresFactory
{
    /// <summary>Creates a new Procedures aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
