/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               EventIdentity.cs
    DESCRIPTION:        Strong-typed identity ValueObject for domain events. Wraps a deterministic, time-ordered UUIDv7
                        identifier assigned at event construction. MUST follow the Deterministic Identifier Rule.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.SharedKernel.Events.Metadata;

/// <summary>
/// Strong-typed identity for a single domain event instance.
/// </summary>
public sealed record EventIdentity
{
    public Guid Value { get; }

    private EventIdentity(Guid value)
    {
        Value = value;
    }

    public static EventIdentity Create()
        => new(Uuid7.New());

    public static EventIdentity From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EventIdentity value cannot be empty.", nameof(value));

        return new EventIdentity(value);
    }

    public override string ToString() => Value.ToString();
}
