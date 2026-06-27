// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Domain
//  File:           ManagementOfChangeFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing ManagementOfChange aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.ManagementOfChange.Factories;

/// <summary>Default factory implementation for constructing <see cref="ManagementOfChange"/> aggregates.</summary>
public sealed class ManagementOfChangeFactory : IManagementOfChangeFactory
{
    /// <inheritdoc/>
    public ManagementOfChange Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => ManagementOfChange.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
