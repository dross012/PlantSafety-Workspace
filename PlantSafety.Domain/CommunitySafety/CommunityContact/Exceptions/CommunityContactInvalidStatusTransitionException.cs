// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityContact
//  Layer:          Domain
//  File:           CommunityContactInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a CommunityContact lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityContact.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityContact.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.CommunityContact"/> lifecycle transition is not permitted.</summary>
public sealed class CommunityContactInvalidStatusTransitionException : CommunityContactDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public CommunityContactInvalidStatusTransitionException(
        CommunityContactId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition CommunityContact '{id}' from '{from}' to '{to}'.",
            "CommunityContact.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
