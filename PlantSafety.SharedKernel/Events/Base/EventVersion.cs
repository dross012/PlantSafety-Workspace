/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               EventVersion.cs
    DESCRIPTION:        Strong-typed canonical event version ValueObject. Enforces the Canonical Event Versioning Rule:
                        versions start at 1 and are incremented only on breaking schema changes.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Events.Base;

/// <summary>
/// Strong-typed canonical event version.
/// </summary>
public sealed record EventVersion
{
    public int Value { get; }

    public EventVersion(int value)
    {
        Value = value;
    }

    public static EventVersion Initial { get; } = new(1);

    public static EventVersion Create(int value)
    {
        if (value < 1)
            throw new ArgumentOutOfRangeException(nameof(value), "EventVersion must be 1 or greater.");

        return new EventVersion(value);
    }

    public override string ToString() => Value.ToString();
}
