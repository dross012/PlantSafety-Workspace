// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Domain
//  File:           AuditTimelineEntryType.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the four kinds of history merged into a single AuditTimeline
//                  read model per Handbook Phase 5 §6.1: EventRaised, StateChange,
//                  ConcurrencyConflict, SchedulingDecision. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditTimeline.Enums;

/// <summary>Smart Enum categorizing a single entry within a unified audit timeline.</summary>
public sealed class AuditTimelineEntryType : IEquatable<AuditTimelineEntryType>
{
    /// <summary>A domain event raised by the entity/aggregate.</summary>
    public static readonly AuditTimelineEntryType EventRaised = new("EventRaised", "Domain Event Raised");

    /// <summary>A before/after state-change snapshot pair for the entity/aggregate.</summary>
    public static readonly AuditTimelineEntryType StateChange = new("StateChange", "State Change");

    /// <summary>An optimistic concurrency conflict captured during a write attempt.</summary>
    public static readonly AuditTimelineEntryType ConcurrencyConflict = new("ConcurrencyConflict", "Concurrency Conflict");

    /// <summary>A scheduling orchestration decision (Phase 5 §1) linked to the entity/aggregate.</summary>
    public static readonly AuditTimelineEntryType SchedulingDecision = new("SchedulingDecision", "Scheduling Decision");

    /// <summary>A compliance violation detected by the Compliance Rules Engine (Phase 5 §6.3).</summary>
    public static readonly AuditTimelineEntryType ComplianceViolation = new("ComplianceViolation", "Compliance Violation");

    /// <summary>All defined AuditTimelineEntryType instances.</summary>
    public static readonly IReadOnlyList<AuditTimelineEntryType> All =
        [EventRaised, StateChange, ConcurrencyConflict, SchedulingDecision, ComplianceViolation];

    /// <summary>The default AuditTimelineEntryType instance.</summary>
    public static AuditTimelineEntryType Default => EventRaised;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    private AuditTimelineEntryType(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    /// <summary>Resolves an AuditTimelineEntryType instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static AuditTimelineEntryType From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown AuditTimelineEntryType code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve an AuditTimelineEntryType instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out AuditTimelineEntryType value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(AuditTimelineEntryType? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as AuditTimelineEntryType);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
