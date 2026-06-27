// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Operational
//  Module:         Alerts
//  Layer:          Domain (Value Objects)
//  File:           SystemCode.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      Strong-typed value object representing a system-origin identifier used
//      by the Alerts module. SystemCode identifies the originating subsystem,
//      integration, or upstream component responsible for generating or
//      forwarding an alert.
//
//  Architectural Notes:
//      • Value objects MUST be immutable (Rule 3.1).
//      • Value objects MUST NOT allow null, empty, or whitespace values
//        (Rule 3.2).
//      • Value objects MUST NOT contain domain behavior (Rule 1.1).
//      • Value objects MUST be serialization-safe and deterministic.
//      • System codes MUST be treated as opaque identifiers — no parsing,
//        no assumptions, no embedded semantics.
//
//  Onboarding Notes:
//      • Used by domain logic, read models, and application events.
//      • SystemCode is NOT a lookup code — it is a free-form, validated,
//        strong-typed identifier.
//      • Validation occurs at construction time; invalid values cannot enter
//        the domain.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.ValueObjects;

public sealed record SystemCode
{
    public string Value { get; }

    public SystemCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SystemCode cannot be null, empty, or whitespace.", nameof(value));

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
