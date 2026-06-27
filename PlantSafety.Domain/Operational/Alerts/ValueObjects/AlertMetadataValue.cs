// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Operational
//  Module:         Alerts
//  Layer:          Domain (Value Objects)
//  File:           AlertMetadataValue.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      Strong-typed value object representing the value portion of an alert
//      metadata entry. Metadata values are free-form, operator- or system-
//      provided strings that enrich an alert with contextual information.
//      This type ensures metadata values are never passed as raw strings.
//
//  Architectural Notes:
//      • Value objects MUST be immutable (Rule 3.1).
//      • Value objects MUST NOT allow null, empty, or whitespace values
//        (Rule 3.2).
//      • Value objects MUST NOT contain domain behavior (Rule 1.1).
//      • Value objects MUST be deterministic and serialization-safe.
//      • Metadata values MUST be treated as opaque text — no parsing,
//        no embedded semantics, no formatting assumptions.
//
//  Onboarding Notes:
//      • Used by AddMetadata, UpdateMetadata, and metadata-related domain
//        events.
//      • AlertMetadataValue is NOT responsible for business rule enforcement —
//        the aggregate enforces metadata invariants.
//      • Validation occurs at construction time; invalid metadata cannot enter
//        the domain.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.ValueObjects;

public sealed record AlertMetadataValue
{
    public string Value { get; }

    public AlertMetadataValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Metadata value cannot be null, empty, or whitespace.", nameof(value));

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
