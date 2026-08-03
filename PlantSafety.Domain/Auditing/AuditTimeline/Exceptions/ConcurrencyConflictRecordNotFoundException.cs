// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Domain
//  File:           ConcurrencyConflictRecordNotFoundException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a ConcurrencyConflictRecord lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Auditing.AuditTimeline.ValueObjects.Ids;

namespace PlantSafety.Domain.Auditing.AuditTimeline.Exceptions;

/// <summary>Thrown when a ConcurrencyConflictRecord lookup finds no match.</summary>
public sealed class ConcurrencyConflictRecordNotFoundException : AuditTimelineDomainException
{
    /// <summary>Creates a not-found exception for the given ConcurrencyConflictRecord identity.</summary>
    public ConcurrencyConflictRecordNotFoundException(ConcurrencyConflictRecordId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"ConcurrencyConflictRecord with id '{id}' was not found.",
            "ConcurrencyConflictRecord.NotFound",
            "NotFound",
            "Low",
            correlationId,
            causationId)
    {
    }
}
