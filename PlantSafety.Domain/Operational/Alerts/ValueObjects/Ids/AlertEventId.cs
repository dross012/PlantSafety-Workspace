// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Operational
//  Module:         Alerts
//  Layer:          Domain (Value Objects)
//  File:           AlertEventId.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      Strong-typed identifier representing the unique identity of an Alert
//      Event within the Alerts domain. AlertEventId is used for lifecycle
//      history entries, metadata change events, and audit trails. This type
//      ensures event identifiers are never passed as raw GUIDs.
//
//  Architectural Notes:
//      • Value objects MUST be immutable (Rule 3.1).
//      • Identifiers MUST NOT allow Guid.Empty (Rule 3.2).
//      • Identifiers MUST be deterministic and serialization-safe.
//      • Uuid7 MUST be used for new identifiers to ensure monotonic ordering
//        and high-entropy uniqueness (Rule 3.7).
//      • Value objects MUST NOT contain domain behavior (Rule 1.1).
//
//  Onboarding Notes:
//      • Used by domain events, lifecycle history, and read models.
//      • AlertEventId is NOT responsible for business rule enforcement —
//        the aggregate enforces event invariants.
//      • Validation occurs at construction time; invalid IDs cannot enter
//        the domain.
// ============================================================================

using PlantSafety.SharedKernel.Types;

namespace PlantSafety.Domain.Operational.Alerts.ValueObjects.Ids;

public readonly record struct AlertEventId
{
    public Guid Value { get; }

    public AlertEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AlertEventId cannot be empty.", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Generates a new monotonic UUIDv7 identifier.
    /// </summary>
    public static AlertEventId New() => new(Uuid7.New());

    /// <summary>
    /// Creates an AlertEventId from an existing GUID.
    /// </summary>
    public static AlertEventId FromGuid(Guid value) => new(value);

    public override string ToString() => Value.ToString("D");
}
