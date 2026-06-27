// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Operational
//  Module:         Alerts
//  Layer:          Domain (Value Objects)
//  File:           AlertMetadataKey.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      Strong-typed value object representing the key portion of an alert
//      metadata entry. Metadata keys uniquely identify contextual attributes
//      attached to an alert (e.g., "EquipmentId", "Shift", "Zone") and ensure
//      that metadata keys are never passed as raw strings.
//
//  Architectural Notes:
//      • Value objects MUST be immutable (Rule 3.1).
//      • Value objects MUST NOT allow null, empty, or whitespace values
//        (Rule 3.2).
//      • Value objects MUST NOT contain domain behavior (Rule 1.1).
//      • Value objects MUST be deterministic and serialization-safe.
//      • Metadata keys MUST be treated as opaque identifiers — no parsing,
//        no embedded semantics, no formatting assumptions.
//      • Metadata keys MUST remain stable for analytics and projections.
//
//  Onboarding Notes:
//      • Used by AddMetadata, UpdateMetadata, RemoveMetadata, and metadata-
//        related domain events.
//      • AlertMetadataKey is NOT responsible for enforcing metadata rules —
//        the aggregate enforces metadata invariants.
//      • Validation occurs at construction time; invalid keys cannot enter
//        the domain.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.ValueObjects;

public sealed record AlertMetadataKey
{
    public string Value { get; }

    public AlertMetadataKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Metadata key cannot be null, empty, or whitespace.", nameof(value));

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
