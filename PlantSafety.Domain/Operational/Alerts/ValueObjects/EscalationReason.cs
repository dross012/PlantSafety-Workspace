// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Operational
//  Module:         Alerts
//  Layer:          Domain (Value Objects)
//  File:           EscalationReason.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      Strong-typed value object representing the human-authored justification
//      for escalating an alert. EscalationReason captures the operator's or
//      system's explanation for why the alert required a higher level of
//      attention, ensuring that escalation reasons are never passed as raw
//      strings.
//
//  Architectural Notes:
//      • Value objects MUST be immutable (Rule 3.1).
//      • Value objects MUST NOT allow null, empty, or whitespace values
//        (Rule 3.2).
//      • Value objects MUST NOT contain domain behavior (Rule 1.1).
//      • Value objects MUST be deterministic and serialization-safe.
//      • Escalation reasons MUST be treated as opaque text — no parsing,
//        no embedded semantics, no formatting assumptions.
//
//  Onboarding Notes:
//      • Used by EscalateAlertCommand, domain logic, and read models.
//      • EscalationReason is NOT responsible for business rule enforcement —
//        the aggregate enforces escalation invariants.
//      • Validation occurs at construction time; invalid reasons cannot enter
//        the domain.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.ValueObjects;

public sealed record EscalationReason
{
    public string Value { get; }

    public EscalationReason(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Escalation reason cannot be null, empty, or whitespace.", nameof(value));

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
