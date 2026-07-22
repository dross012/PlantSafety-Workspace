// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitTemplate
//  Layer:          Domain
//  File:           IPermitTemplateFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing PermitTemplate aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitTemplate.Interfaces;

/// <summary>Factory interface for constructing <see cref="PermitTemplate"/> aggregates.</summary>
public interface IPermitTemplateFactory
{
    /// <summary>Creates a new PermitTemplate aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates.PermitTemplate Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}