// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           SystemDateTimeProvider.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Real IDateTimeProvider implementation backed by the system UTC clock.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Clock;

namespace PlantSafety.Infrastructure.Common.Clock;

/// <summary>Real <see cref="IDateTimeProvider"/> backed by the system UTC clock.</summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc/>
    public DateTime UtcNow => DateTime.UtcNow;

    /// <inheritdoc/>
    public DateTimeOffset UtcNowOffset => DateTimeOffset.UtcNow;

    /// <inheritdoc/>
    public DateOnly UtcToday => DateOnly.FromDateTime(DateTime.UtcNow);
}
