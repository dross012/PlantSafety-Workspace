// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertId.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Strongly-typed identifier for the Alert aggregate root.
//      Complies with the Deterministic Identifier Rule (1.2),
//      StrongId Enforcement Rule (5.5), and uses UUIDv7 for
//      time-ordered, SIEM-safe, replay-safe identifiers.
//      Additive-only evolution.
// ============================================================================

using System;
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Operational.Alerts.ValueObjects.Ids;

public readonly record struct AlertId
{
    public Guid Value { get; }

    public AlertId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AlertId value must not be empty.", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Creates a new AlertId using a freshly generated UUIDv7.
    /// </summary>
    public static AlertId New() => new(Uuid7.New());

    /// <summary>
    /// Creates an AlertId from an existing Guid (e.g., from persistence).
    /// </summary>
    public static AlertId FromGuid(Guid value) => new(value);

    /// <summary>
    /// Parses an AlertId from a string representation of a Guid.
    /// </summary>
    public static AlertId Parse(string value)
        => new(Guid.Parse(value));

    public override string ToString() => Value.ToString("D");
}
