// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityContact
//  Layer:          Domain
//  File:           CommunityContactNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a CommunityContact aggregate lookup finds no match. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityContact.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityContact.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.CommunityContact"/> aggregate lookup finds no match.</summary>
public sealed class CommunityContactNotFoundException : CommunityContactDomainException
{
    /// <summary>Creates a not-found exception for the given CommunityContact identity.</summary>
    public CommunityContactNotFoundException(CommunityContactId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"CommunityContact with id '{id}' was not found.",
            "CommunityContact.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
