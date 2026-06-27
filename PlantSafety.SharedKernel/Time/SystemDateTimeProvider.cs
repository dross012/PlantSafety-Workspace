// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Time
//  Layer:          SharedKernel
//  File:           SystemDateTimeProvider.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Production implementation of IDateTimeProvider that delegates
//                  to the system clock. Registered as a singleton. Additive-only.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.SharedKernel.Time;

/// <summary>
/// Production implementation of <see cref="IDateTimeProvider"/>.
/// Delegates to <see cref="DateTime.UtcNow"/>.
/// Register as a singleton in DI.
/// </summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc/>
    public UtcTimestamp UtcNow => UtcTimestamp.From(DateTime.UtcNow);

    /// <inheritdoc/>
    public DateTime UtcDateTime => DateTime.UtcNow;

    /// <inheritdoc/>
    public DateTimeOffset UtcDateTimeOffset => DateTimeOffset.UtcNow;
}
