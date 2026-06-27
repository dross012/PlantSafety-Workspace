// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel (Identity)
//  Layer:          Domain
//  File:           UserId.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      StrongId representing a human user identity, used for CRUD authorship
//      fields (CreatedBy, UpdatedBy, DeletedBy) as required by the
//      Actor & User Identity Semantics Rule (1.8).
//      Backed by UUIDv7 for deterministic, time-ordered identifiers.
//      Additive-only evolution.
// ============================================================================

using System;

namespace PlantSafety.SharedKernel.Identifiers;

public readonly record struct UserId
{
    public Guid Value { get; }

    public UserId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId value must not be empty.", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Creates a new UserId using a freshly generated UUIDv7.
    /// </summary>
    public static UserId New() => new(Uuid7.New());

    public static UserId FromGuid(Guid value) => new(value);

    public static UserId Parse(string value)
        => new(Guid.Parse(value));

    public override string ToString() => Value.ToString("D");
}
