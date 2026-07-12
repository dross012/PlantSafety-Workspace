// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.RiskManagement
//  Layer:          Domain
//  File:           RiskManagementFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing RiskManagement aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.RiskManagement.Factories;

/// <summary>Default factory implementation for constructing <see cref="RiskManagement"/> aggregates.</summary>
public sealed class RiskManagementFactory : IRiskManagementFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Operational.RiskManagement.Aggregates.RiskManagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Operational.RiskManagement.Aggregates.RiskManagement.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
