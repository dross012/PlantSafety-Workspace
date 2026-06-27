// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Operational
//  Module:         Alerts
//  Layer:          Domain (Value Objects)
//  File:           AlertNotes.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      Strong-typed value object representing operator-authored notes attached
//      to an alert. AlertNotes encapsulates free-form explanatory text used
//      during acknowledgment, escalation, in-progress updates, and closure.
//
//  Architectural Notes:
//      • Value objects MUST be immutable (Rule 3.1).
//      • Value objects MUST NOT allow null, empty, or whitespace values
//        (Rule 3.2).
//      • Value objects MUST NOT contain domain behavior (Rule 1.1).
//      • Value objects MUST be deterministic and serialization-safe.
//      • Notes MUST be treated as opaque text — no parsing, no embedded
//        semantics, no formatting assumptions.
//
//  Onboarding Notes:
//      • Used by multiple lifecycle transitions (Acknowledge, InProgress,
//        Escalate, Close).
//      • AlertNotes is NOT responsible for business rule enforcement —
//        the aggregate enforces lifecycle invariants.
//      • Validation occurs at construction time; invalid notes cannot enter
//        the domain.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.ValueObjects;

public sealed record AlertNotes
{
    public string Value { get; }

    public AlertNotes(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Alert notes cannot be null, empty, or whitespace.", nameof(value));

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
