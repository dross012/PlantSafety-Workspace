// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Domain
//  File:           ConcurrencyConflictRecordId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the ConcurrencyConflictRecord
//                  aggregate root, which captures optimistic concurrency conflicts instead of
//                  silently dropping them (Handbook Phase 5 §6.1). Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Auditing.AuditTimeline.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a ConcurrencyConflictRecord.</summary>
public readonly record struct ConcurrencyConflictRecordId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ConcurrencyConflictRecordId wrapping a non-empty GUID.</summary>
    public ConcurrencyConflictRecordId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ConcurrencyConflictRecordId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ConcurrencyConflictRecordId using Uuid7.</summary>
    public static ConcurrencyConflictRecordId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ConcurrencyConflictRecordId.</summary>
    public static ConcurrencyConflictRecordId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
