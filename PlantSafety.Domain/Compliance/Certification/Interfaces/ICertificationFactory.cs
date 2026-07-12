// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Domain
//  File:           ICertificationFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing Certification aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Certification.Interfaces;

/// <summary>Factory interface for constructing <see cref="Certification"/> aggregates.</summary>
public interface ICertificationFactory
{
    /// <summary>Creates a new Certification aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Compliance.Certification.Aggregates.Certification Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
