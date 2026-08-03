// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Domain
//  File:           AuditTimelineEntryId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for a single unified AuditTimelineEntry
//                  value (Handbook Phase 5 §6.1). Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Auditing.AuditTimeline.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a single AuditTimelineEntry.</summary>
public readonly record struct AuditTimelineEntryId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an AuditTimelineEntryId wrapping a non-empty GUID.</summary>
    public AuditTimelineEntryId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AuditTimelineEntryId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AuditTimelineEntryId using Uuid7.</summary>
    public static AuditTimelineEntryId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an AuditTimelineEntryId.</summary>
    public static AuditTimelineEntryId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
