/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               EventName.cs
    DESCRIPTION:        Strong-typed canonical event name ValueObject. Enforces normalization, validation, SIEM-safety,
                        and deterministic behavior. MUST follow Canonical Event Naming Rule (Module.Entity.Action).

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Events.Base;

/// <summary>
/// Strong-typed canonical event name.
/// <para>
/// Invariants:
/// - MUST NOT be null or empty.
/// - MUST be trimmed and normalized.
/// - MUST follow Module.Entity.Action naming (e.g., Routing.Job.Assigned).
/// </para>
/// </summary>
public sealed record EventName
{
    /// <summary>
    /// The normalized canonical event name value.
    /// </summary>
    public string Value { get; }

    private EventName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="EventName"/> after applying normalization and validation.
    /// </summary>
    /// <param name="value">Raw event name string.</param>
    /// <returns>A valid <see cref="EventName"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is empty or invalid.</exception>
    public static EventName Create(string value)
    {
        value = value?.Trim() ?? throw new ArgumentNullException(nameof(value));

        if (value.Length == 0)
            throw new ArgumentException("EventName cannot be empty.", nameof(value));

        if (!value.Contains('.'))
            throw new ArgumentException("EventName must follow Module.Entity.Action format.", nameof(value));

        return new EventName(value);
    }

    /// <summary>
    /// Attempts to parse a raw string into a valid <see cref="EventName"/> without throwing.
    /// </summary>
    /// <param name="value">Raw event name string.</param>
    /// <param name="result">Parsed <see cref="EventName"/> if successful; otherwise null.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise <c>false</c>.</returns>
    public static bool TryParse(string? value, out EventName? result)
    {
        try
        {
            result = Create(value!);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}
