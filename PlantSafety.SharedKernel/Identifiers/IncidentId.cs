// =====================================================================================================
//  FILE: IncidentId.cs
//  LAYER: SharedKernel
//  MODULE: Identifiers
//  SCHEMA VERSION: 2026.1
//  AUTHOR: David (Ted) + Copilot
//  CREATED: 2026-05-27
//
//  PURPOSE:
//      Strong identifier for incidents. Used across Alerts, Incident Management, Safety,
//      and Reporting modules. Immutable, JSON-safe, deterministic.
//
// =====================================================================================================

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using PlantSafety.SharedKernel.Primitives.StrongIds;

namespace PlantSafety.SharedKernel.Identifiers;

[JsonConverter(typeof(StrongIdJsonConverter<IncidentId>))]
public readonly record struct IncidentId(Guid Value)
    : IStrongId, IParsable<IncidentId>, ISpanParsable<IncidentId>
{
    public Guid Value { get; init; } = Value;

    public static IncidentId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static IncidentId Parse(string s, IFormatProvider? provider = null)
        => new(Guid.Parse(s));

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out IncidentId result)
    {
        if (Guid.TryParse(s, out var g))
        {
            result = new IncidentId(g);
            return true;
        }

        result = default;
        return false;
    }

    public static IncidentId Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null)
        => new(Guid.Parse(s));

    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        out IncidentId result)
    {
        if (Guid.TryParse(s, out var g))
        {
            result = new IncidentId(g);
            return true;
        }

        result = default;
        return false;
    }

    public bool IsEmpty => Value == Guid.Empty;
}
