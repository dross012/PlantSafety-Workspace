// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.PpePolicies
//  Layer:          Domain
//  File:           PpePoliciesNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PpePolicies aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.PpePolicies.Exceptions;

/// <summary>Thrown when a <see cref="PpePolicies"/> aggregate lookup finds no match.</summary>
public sealed class PpePoliciesNotFoundException : PpePoliciesDomainException
{
    /// <summary>Creates a not-found exception for the given PpePolicies identity.</summary>
    public PpePoliciesNotFoundException(PpePoliciesId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "PpePolicies with id '{id}' was not found.",
            "PpePolicies.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}