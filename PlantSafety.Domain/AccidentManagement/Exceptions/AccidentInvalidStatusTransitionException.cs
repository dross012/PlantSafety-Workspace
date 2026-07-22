// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           AccidentInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a Accident lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.AccidentManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.AccidentManagement.Exceptions;

/// <summary>Thrown when a Accident lifecycle transition is not permitted.</summary>
public sealed class AccidentInvalidStatusTransitionException : AccidentDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public AccidentInvalidStatusTransitionException(
        AccidentId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'Accident' {id} from '{from}' to '{to}'.",
            "Accident.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}