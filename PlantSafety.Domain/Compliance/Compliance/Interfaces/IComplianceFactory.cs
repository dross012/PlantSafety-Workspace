// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Domain
//  File:           IComplianceFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing Compliance aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Compliance.Interfaces;

/// <summary>Factory interface for constructing <see cref="Compliance"/> aggregates.</summary>
public interface IComplianceFactory
{
    /// <summary>Creates a new Compliance aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Compliance.Compliance.Aggregates.Compliance Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
