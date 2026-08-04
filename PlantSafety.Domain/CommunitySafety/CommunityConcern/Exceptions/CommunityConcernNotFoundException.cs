// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityConcern
//  Layer:          Domain
//  File:           CommunityConcernNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a CommunityConcern aggregate lookup finds no match. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityConcern.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityConcern.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.CommunityConcern"/> aggregate lookup finds no match.</summary>
public sealed class CommunityConcernNotFoundException : CommunityConcernDomainException
{
    /// <summary>Creates a not-found exception for the given CommunityConcern identity.</summary>
    public CommunityConcernNotFoundException(CommunityConcernId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"CommunityConcern with id '{id}' was not found.",
            "CommunityConcern.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
