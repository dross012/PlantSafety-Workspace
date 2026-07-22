// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           WidgetRefreshInterval.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Value Object representing a Widget's refresh cadence, per "2. Widget: Has:
//                  ... RefreshInterval" and "6. Performance &amp; Real-Time Design: Heavy metrics
//                  ... pre-aggregated. Real-time streams ... push updates. Caching at dashboard
//                  and widget level with configurable TTL."
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Dashboards.Widget.ValueObjects;

/// <summary>Immutable value object representing a widget's refresh/TTL cadence in seconds.</summary>
public readonly record struct WidgetRefreshInterval
{
    /// <summary>Minimum permitted refresh interval, in seconds (prevents excessive real-time polling load).</summary>
    public const int MinSeconds = 1;

    /// <summary>Maximum permitted refresh interval, in seconds (24 hours; heavy/pre-aggregated metrics).</summary>
    public const int MaxSeconds = 86_400;

    /// <summary>Refresh cadence, in seconds.</summary>
    public int Seconds { get; }

    private WidgetRefreshInterval(int seconds) => Seconds = seconds;

    /// <summary>Creates a validated <see cref="WidgetRefreshInterval"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="seconds"/> is outside [<see cref="MinSeconds"/>, <see cref="MaxSeconds"/>].</exception>
    public static WidgetRefreshInterval FromSeconds(int seconds)
    {
        if (seconds is < MinSeconds or > MaxSeconds)
            throw new ArgumentOutOfRangeException(nameof(seconds), $"WidgetRefreshInterval must be between {MinSeconds} and {MaxSeconds} seconds.");

        return new WidgetRefreshInterval(seconds);
    }

    /// <summary>Canonical real-time cadence (5 seconds) for live streams (alerts, observability, man-down, E-Stops).</summary>
    public static readonly WidgetRefreshInterval RealTime = FromSeconds(5);

    /// <summary>Canonical near-real-time cadence (60 seconds) for frequently-changing operational widgets.</summary>
    public static readonly WidgetRefreshInterval NearRealTime = FromSeconds(60);

    /// <summary>Canonical heavy-metric cadence (1 hour) for pre-aggregated KPIs (TRIR, DART, monthly trends).</summary>
    public static readonly WidgetRefreshInterval HeavyMetric = FromSeconds(3_600);

    /// <inheritdoc/>
    public override string ToString() => $"{Seconds}s";
}
