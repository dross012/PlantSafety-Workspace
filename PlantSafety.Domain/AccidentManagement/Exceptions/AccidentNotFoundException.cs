// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           AccidentNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a Accident aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.AccidentManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.AccidentManagement.Exceptions;

/// <summary>Thrown when a Accident aggregate lookup finds no match.</summary>
public sealed class AccidentNotFoundException : AccidentDomainException
{
    /// <summary>Creates a not-found exception for the given Accident identity.</summary>
    public AccidentNotFoundException(AccidentId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"Accident with id '{id}' was not found.",
            "Accident.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}