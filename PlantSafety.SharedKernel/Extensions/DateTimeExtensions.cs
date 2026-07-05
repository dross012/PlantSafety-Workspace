// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Extensions
//  Layer:          SharedKernel
//  File:           DateTimeExtensions.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Extension methods for DateTime and DateTimeOffset.
//                  All methods are pure and deterministic. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.SharedKernel.Extensions;

/// <summary>Pure, deterministic DateTime extension methods.</summary>
public static class DateTimeExtensions
{
    /// <summary>Converts a DateTime to a <see cref="UtcTimestamp"/>, normalizing to UTC.</summary>
    public static UtcTimestamp ToUtcTimestamp(this DateTime value) => UtcTimestamp.From(value);

    /// <summary>Returns true if the value is a valid, non-empty UTC instant.</summary>
    public static bool IsValidUtc(this DateTime value)
        => value.Kind == DateTimeKind.Utc && value != DateTime.MinValue;

    /// <summary>Truncates a DateTime to whole seconds (removes sub-second precision).</summary>
    public static DateTime TruncateToSeconds(this DateTime value)
        => value.AddTicks(-(value.Ticks % TimeSpan.TicksPerSecond));

    /// <summary>Returns the start of the calendar day (00:00:00 UTC).</summary>
    public static DateTime StartOfDay(this DateTime value)
        => new(value.Year, value.Month, value.Day, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>Returns the end of the calendar day (23:59:59.9999999 UTC).</summary>
    public static DateTime EndOfDay(this DateTime value)
        => new DateTime(value.Year, value.Month, value.Day, 23, 59, 59, 999, DateTimeKind.Utc)
            .AddTicks(9999);

    /// <summary>
    /// Safely converts a nullable <see cref="UtcTimestamp"/> to a nullable <see cref="DateTime"/>.
    /// Returns null if the timestamp is empty.
    /// </summary>
    public static DateTime? ToNullableDateTime(this UtcTimestamp? stamp)
        => stamp is { IsEmpty: false } s ? s.Value : null;
}
