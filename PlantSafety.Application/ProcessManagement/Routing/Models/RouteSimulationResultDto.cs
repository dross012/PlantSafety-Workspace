// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           RouteSimulationResultDto.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Read-model projection of RouteSimulationEngine.Simulate's output. Shape
//                  matches PlantSafety.UI.Modules.Routing.Models.RoutingSimulationResultDto
//                  field-for-field. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.ProcessManagement.Routing.Models;

/// <summary>Server-computed simulation result summary.</summary>
public sealed record RouteSimulationResultDto
{
    /// <summary>Total simulated duration in minutes.</summary>
    public required int TotalDurationMinutes { get; init; }

    /// <summary>Critical path step numbers, in order.</summary>
    public required IReadOnlyList<int> CriticalPath { get; init; }

    /// <summary>Count of detected resource conflicts.</summary>
    public required int ResourceConflictCount { get; init; }

    /// <summary>Count of detected material shortages.</summary>
    public required int MaterialShortageCount { get; init; }

    /// <summary>Bottleneck step numbers flagged by the simulation.</summary>
    public required IReadOnlyList<int> BottleneckSteps { get; init; }
}
