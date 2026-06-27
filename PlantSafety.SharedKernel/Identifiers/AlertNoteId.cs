// =====================================================================================================
//  FILE: AlertNoteId.cs
//  LAYER: SharedKernel
//  MODULE: Identifiers
//  SCHEMA VERSION: 2026.1
//  AUTHOR: David (Ted) + Copilot
//  CREATED: 2026-05-27
//
//  PURPOSE:
//      Strong identifier for alert notes. Used by Alerts, Auditing, and UI detail views.
//      Immutable, JSON-safe, deterministic.
//
// =====================================================================================================

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using PlantSafety.SharedKernel.Primitives.StrongIds;

namespace PlantSafety.SharedKernel.Identifiers;

[JsonConverter(typeof(StrongIdJsonConverter<AlertNoteId>))]
public readonly record struct AlertNoteId(Guid Value)
    : IStrongId, IParsable<AlertNoteId>, ISpanParsable<AlertNoteId>
{
    public Guid StrongIdValue => Value;

    public static AlertNoteId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static AlertNoteId Parse(string s, IFormatProvider? provider = null)
        => new(Guid.Parse(s));

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out AlertNoteId result)
    {
        if (Guid.TryParse(s, out var g))
        {
            result = new AlertNoteId(g);
            return true;
        }

        result = default;
        return false;
    }

    public static AlertNoteId Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null)
        => new(Guid.Parse(s));

    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        out AlertNoteId result)
    {
        if (Guid.TryParse(s, out var g))
        {
            result = new AlertNoteId(g);
            return true;
        }

        result = default;
        return false;
    }

    public bool IsEmpty => Value == Guid.Empty;
}
