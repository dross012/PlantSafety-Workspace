// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           RiskNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a Risk aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.RiskManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.RiskManagement.Exceptions;

/// <summary>Thrown when a Risk aggregate lookup finds no match.</summary>
public sealed class RiskNotFoundException : RiskDomainException
{
    /// <summary>Creates a not-found exception for the given Risk identity.</summary>
    public RiskNotFoundException(RiskId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"Risk with id '{id}' was not found.",
            "Risk.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}