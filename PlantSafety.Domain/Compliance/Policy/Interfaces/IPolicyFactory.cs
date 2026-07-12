// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Domain
//  File:           IPolicyFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing Policy aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Policy.Interfaces;

/// <summary>Factory interface for constructing <see cref="Policy"/> aggregates.</summary>
public interface IPolicyFactory
{
    /// <summary>Creates a new Policy aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Compliance.Policy.Aggregates.Policy Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
