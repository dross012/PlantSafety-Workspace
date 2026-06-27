// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Domain
//  File:           PolicyNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Policy.Exceptions;

/// <summary>Thrown when a <see cref="Policy"/> aggregate lookup finds no match.</summary>
public sealed class PolicyNotFoundException : PolicyDomainException
{
    /// <summary>Creates a not-found exception for the given Policy identity.</summary>
    public PolicyNotFoundException(PolicyId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "Policy with id '{id}' was not found.",
            "Policy.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
