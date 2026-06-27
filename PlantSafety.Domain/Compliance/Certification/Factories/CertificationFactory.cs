// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Domain
//  File:           CertificationFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing Certification aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Certification.Factories;

/// <summary>Default factory implementation for constructing <see cref="Certification"/> aggregates.</summary>
public sealed class CertificationFactory : ICertificationFactory
{
    /// <inheritdoc/>
    public Certification Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => Certification.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
