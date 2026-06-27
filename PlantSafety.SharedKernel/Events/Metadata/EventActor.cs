/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               EventActor.cs
    DESCRIPTION:        Strong-typed actor identifier ValueObject. Represents the actor responsible for the event. Supports
                        human users, system actors, workflows, sensors, and external systems.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Events.Metadata;

/// <summary>
/// Strong-typed actor identifier.
/// </summary>
public sealed record EventActor
{
    public string Value { get; }

    private EventActor(string value)
    {
        Value = value;
    }

    public static EventActor Create(string value)
    {
        value = value?.Trim() ?? throw new ArgumentNullException(nameof(value));

        if (value.Length == 0)
            throw new ArgumentException("Actor cannot be empty.");

        return new EventActor(value);
    }

    public override string ToString() => Value;
}
