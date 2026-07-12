// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Contractor
//  Layer:          Domain
//  File:           ContractorFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing Contractor aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Contractor.Factories;

/// <summary>Default factory implementation for constructing <see cref="Contractor"/> aggregates.</summary>
public sealed class ContractorFactory : IContractorFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Operational.Contractor.Aggregates.Contractor Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Operational.Contractor.Aggregates.Contractor.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
