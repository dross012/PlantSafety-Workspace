// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitWorkflow
//  Layer:          Domain
//  File:           PermitWorkflowFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing PermitWorkflow aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitWorkflow.Factories;

/// <summary>Default factory implementation for constructing <see cref="PermitWorkflow"/> aggregates.</summary>
public sealed class PermitWorkflowFactory : IPermitWorkflowFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}