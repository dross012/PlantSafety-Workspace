// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.PpePolicies
//  Layer:          Domain
//  File:           IPpePoliciesFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing PpePolicies aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.PpePolicies.Interfaces;

/// <summary>Factory interface for constructing <see cref="PpePolicies"/> aggregates.</summary>
public interface IPpePoliciesFactory
{
    /// <summary>Creates a new PpePolicies aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.SafetyProcedures.PpePolicies.Aggregates.PpePolicies Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}