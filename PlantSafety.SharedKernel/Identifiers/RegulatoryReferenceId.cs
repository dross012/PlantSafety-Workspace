// =====================================================================================================
//  FILE: RegulatoryReferenceId.cs
//  LAYER: SharedKernel
//  MODULE: Identifiers
//  SCHEMA VERSION: 2026.1
//  AUTHOR: David (Ted) + Copilot
//  CREATED: 2026-05-27
//
//  PURPOSE:
//      Strong identifier for regulatory reference records. Used across Alerts, Compliance,
//      Auditing, and Reporting modules. Immutable, JSON-safe, deterministic.
//
// =====================================================================================================

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using PlantSafety.SharedKernel.Primitives.StrongIds;

namespace PlantSafety.SharedKernel.Identifiers;

[JsonConverter(typeof(StrongIdJsonConverter<RegulatoryReferenceId>))]
public readonly record struct RegulatoryReferenceId(Guid Value)
    : IStrongId, IParsable<RegulatoryReferenceId>, ISpanParsable<RegulatoryReferenceId>
{
    public Guid Value { get; init; } = Value;

    public static RegulatoryReferenceId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static RegulatoryReferenceId Parse(string s, IFormatProvider? provider = null)
        => new(Guid.Parse(s));

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out RegulatoryReferenceId result)
    {
        if (Guid.TryParse(s, out var g))
        {
            result = new RegulatoryReferenceId(g);
            return true;
        }

        result = default;
        return false;
    }

    public static RegulatoryReferenceId Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null)
        => new(Guid.Parse(s));

    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        out RegulatoryReferenceId result)
    {
        if (Guid.TryParse(s, out var g))
        {
            result = new RegulatoryReferenceId(g);
            return true;
        }

        result = default;
        return false;
    }

    public bool IsEmpty => Value == Guid.Empty;
}

