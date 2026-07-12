// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Operational
//  Module:         Alerts
//  Layer:          Domain (Value Objects / Identifiers)
//  File:           AlertAssignmentId.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      Strong-typed identifier representing the unique identity of an Alert
//      Assignment entry. AlertAssignmentId is used for assignment history,
//      lifecycle transitions, and audit trails. This type ensures assignment
//      identifiers are never passed as raw GUIDs.
//
//  Architectural Notes:
//      • Identifiers MUST be immutable (Rule 3.1).
//      • Identifiers MUST NOT allow Guid.Empty (Rule 3.2).
//      • Identifiers MUST be deterministic and serialization-safe.
//      • Uuid7 MUST be used for new identifiers to ensure monotonic ordering
//        and high-entropy uniqueness (Rule 3.7).
//      • Value objects MUST NOT contain domain behavior (Rule 1.1).
//
//  Onboarding Notes:
//      • Used by assignment domain events, lifecycle history, and read models.
//      • AlertAssignmentId is NOT responsible for business rule enforcement —
//        the aggregate enforces assignment invariants.
//      • Validation occurs at construction time; invalid IDs cannot enter
//        the domain.
// ============================================================================

using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Operational.Alerts.ValueObjects.Ids;

public readonly record struct AlertAssignmentId
{
    public Guid Value { get; }

    public AlertAssignmentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AlertAssignmentId cannot be empty.", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Generates a new monotonic UUIDv7 identifier.
    /// </summary>
    public static AlertAssignmentId New() => new(Uuid7.New());

    /// <summary>
    /// Creates an AlertAssignmentId from an existing GUID.
    /// </summary>
    public static AlertAssignmentId FromGuid(Guid value) => new(value);

    public override string ToString() => Value.ToString("D");
}
