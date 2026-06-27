/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               EventTimestamp.cs
    DESCRIPTION:        Strong-typed UTC timestamp ValueObject. Enforces Deterministic Time Rule and UTC-only behavior.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Events.Metadata;

/// <summary>
/// Strong-typed UTC timestamp for domain events.
/// </summary>
public sealed record EventTimestamp
{
    public DateTime Value { get; }

    private EventTimestamp(DateTime value)
    {
        Value = value;
    }

    public static EventTimestamp UtcNow()
        => new EventTimestamp(DateTime.UtcNow);

    public override string ToString() => Value.ToString("O");
}
