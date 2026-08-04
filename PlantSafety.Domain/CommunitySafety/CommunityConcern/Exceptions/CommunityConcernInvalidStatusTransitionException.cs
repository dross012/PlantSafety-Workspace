// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityConcern
//  Layer:          Domain
//  File:           CommunityConcernInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a CommunityConcern lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityConcern.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityConcern.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.CommunityConcern"/> lifecycle transition is not permitted.</summary>
public sealed class CommunityConcernInvalidStatusTransitionException : CommunityConcernDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public CommunityConcernInvalidStatusTransitionException(
        CommunityConcernId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition CommunityConcern '{id}' from '{from}' to '{to}'.",
            "CommunityConcern.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
