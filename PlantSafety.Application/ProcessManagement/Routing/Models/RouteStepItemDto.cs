// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           RouteStepItemDto.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Read-model row for a single ordered route step, including its
//                  domain-computed RouteStepStatus (Valid/Warning/Blocked). Shape matches
//                  PlantSafety.UI.Modules.Routing.Models.RoutingStepDto field-for-field.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.ProcessManagement.Routing.Models;

/// <summary>Single routing step row, including its computed readiness status.</summary>
public sealed record RouteStepItemDto
{
    /// <summary>Strictly-sequential step number (1..N, no gaps).</summary>
    public required int StepNumber { get; init; }

    /// <summary>Step display name.</summary>
    public required string Name { get; init; }

    /// <summary>Domain-computed readiness status (Valid/Warning/Blocked).</summary>
    public required string Status { get; init; }

    /// <summary>Step execution instructions.</summary>
    public required string Instructions { get; init; }

    /// <summary>Assigned resource names for this step.</summary>
    public required IReadOnlyList<string> Resources { get; init; }
}
