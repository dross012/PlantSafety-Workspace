// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PPEManagement
//  Layer:          Domain
//  File:           PPEManagementFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing PPEManagement aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PPEManagement.Factories;

/// <summary>Default factory implementation for constructing <see cref="PPEManagement"/> aggregates.</summary>
public sealed class PPEManagementFactory : IPPEManagementFactory
{
    /// <inheritdoc/>
    public PPEManagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PPEManagement.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
