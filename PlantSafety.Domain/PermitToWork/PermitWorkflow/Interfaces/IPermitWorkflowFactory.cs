// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitWorkflow
//  Layer:          Domain
//  File:           IPermitWorkflowFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing PermitWorkflow aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitWorkflow.Interfaces;

/// <summary>Factory interface for constructing <see cref="PermitWorkflow"/> aggregates.</summary>
public interface IPermitWorkflowFactory
{
    /// <summary>Creates a new PermitWorkflow aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}