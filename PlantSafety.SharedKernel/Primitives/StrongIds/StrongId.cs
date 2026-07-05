/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-29
    FILE:               StrongId.cs
    DESCRIPTION:        Canonical generic StrongId base class. Provides immutable, deterministic, JSON-safe, SQL-safe,
                        SIEM-safe identity primitives. Enforces non-empty GUIDs and additive-only evolution seams.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only. No breaking changes permitted.
==========================================================================================================================*/

using System;
using System.Text.Json.Serialization;

namespace PlantSafety.SharedKernel.Primitives.StrongIds;

/// <summary>
/// Canonical generic StrongId base class.
/// </summary>
public abstract record StrongId<TStrongId> : IStrongId
    where TStrongId : StrongId<TStrongId>, new()
{
    /// <summary>
    /// Underlying Guid value.
    /// </summary>
    [JsonInclude]
    public Guid Value { get; init; }

    /// <summary>
    /// Indicates whether the StrongId wraps Guid.Empty.
    /// </summary>
    public bool IsEmpty => Value == Guid.Empty;

    protected StrongId() { }

    protected StrongId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("StrongId cannot wrap Guid.Empty.", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Creates a new StrongId with a new Guid.
    /// </summary>
    public static TStrongId New()
        => new() { Value = Guid.NewGuid() };

    /// <summary>
    /// Creates a StrongId from an existing Guid.
    /// </summary>
    public static TStrongId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("StrongId cannot wrap Guid.Empty.", nameof(value));

        return new() { Value = value };
    }

    /// <summary>
    /// Attempts to parse a Guid into a StrongId without throwing.
    /// </summary>
    public static bool TryParse(string? input, out TStrongId? result)
    {
        if (Guid.TryParse(input, out var guid) && guid != Guid.Empty)
        {
            result = new TStrongId { Value = guid };
            return true;
        }

        result = null;
        return false;
    }

    public override string ToString()
        => Value.ToString("D");
}

