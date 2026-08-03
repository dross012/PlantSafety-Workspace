// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.Telemetry
//  Layer:          Domain
//  File:           ProductionCountReading.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Production Tracking Intelligence)
//  SchemaVersion:  2026.1
//  Description:    Immutable aggregation of good/scrap unit counts observed for a machine
//                  over a fixed telemetry window, feeding the KPI engine's Yield/ScrapRate/
//                  Throughput calculators. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.Telemetry.ValueObjects;

/// <summary>Good/scrap production counts observed for a machine over a fixed telemetry window.</summary>
public sealed record ProductionCountReading
{
    /// <summary>Identity of the monitored machine.</summary>
    public required Guid MachineId { get; init; }

    /// <summary>UTC start of the aggregation window.</summary>
    public required UtcTimestamp WindowStartUtc { get; init; }

    /// <summary>UTC end of the aggregation window.</summary>
    public required UtcTimestamp WindowEndUtc { get; init; }

    /// <summary>Count of units produced that passed quality inspection within the window.</summary>
    public required int GoodUnitCount { get; init; }

    /// <summary>Count of units scrapped/rejected within the window.</summary>
    public required int ScrapUnitCount { get; init; }

    /// <summary>Total units produced (good + scrap) within the window.</summary>
    public int TotalUnitCount => GoodUnitCount + ScrapUnitCount;
}
