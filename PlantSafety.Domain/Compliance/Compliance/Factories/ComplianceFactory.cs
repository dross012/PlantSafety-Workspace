// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Domain
//  File:           ComplianceFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing Compliance aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Compliance.Factories;

/// <summary>Default factory implementation for constructing <see cref="Compliance"/> aggregates.</summary>
public sealed class ComplianceFactory : IComplianceFactory
{
    /// <inheritdoc/>
    public Compliance Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => Compliance.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
