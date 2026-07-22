// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitExpiration
//  Layer:          Domain
//  File:           IPermitExpirationFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing PermitExpiration aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitExpiration.Interfaces;

/// <summary>Factory interface for constructing <see cref="PermitExpiration"/> aggregates.</summary>
public interface IPermitExpirationFactory
{
    /// <summary>Creates a new PermitExpiration aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates.PermitExpiration Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}