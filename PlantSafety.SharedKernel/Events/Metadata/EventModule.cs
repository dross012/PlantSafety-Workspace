/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               EventModule.cs
    DESCRIPTION:        Strong-typed module identifier ValueObject. Represents the module that emitted the event. MUST be
                        normalized and SIEM-safe.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Events.Metadata;

/// <summary>
/// Strong-typed module identifier.
/// </summary>
public sealed record EventModule
{
    public string Value { get; }

    private EventModule(string value)
    {
        Value = value;
    }

    public static EventModule Create(string value)
    {
        value = value?.Trim() ?? throw new ArgumentNullException(nameof(value));

        if (value.Length == 0)
            throw new ArgumentException("Module cannot be empty.");

        return new EventModule(value);
    }

    public override string ToString() => Value;
}
