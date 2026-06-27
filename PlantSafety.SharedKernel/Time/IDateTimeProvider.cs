// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Time
//  Layer:          SharedKernel
//  File:           IDateTimeProvider.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Canonical clock abstraction. ALL code that needs the current
//                  time MUST inject IDateTimeProvider — never call DateTime.UtcNow
//                  directly in production code. This enables deterministic testing.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.SharedKernel.Time;

/// <summary>
/// Canonical clock abstraction.
/// <para>
/// RULE: All production code that requires the current time MUST inject
/// <see cref="IDateTimeProvider"/> rather than calling <c>DateTime.UtcNow</c>
/// or <c>DateTimeOffset.UtcNow</c> directly. This ensures test determinism.
/// </para>
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>Current UTC date and time as a <see cref="UtcTimestamp"/>.</summary>
    UtcTimestamp UtcNow { get; }

    /// <summary>Current UTC date and time as a <see cref="DateTime"/> (Kind == Utc).</summary>
    DateTime UtcDateTime { get; }

    /// <summary>Current UTC date and time as a <see cref="DateTimeOffset"/>.</summary>
    DateTimeOffset UtcDateTimeOffset { get; }
}
