// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           RouteStepInput.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Shared command-input shape for a single ordered step, used by both
//                  CreateRouteDefinitionCommand (ExistingStepId always null) and
//                  ReviseRouteDefinitionCommand (ExistingStepId set to preserve identity across
//                  an in-place edit, or null for a newly added step).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.ProcessManagement.Routing.Models;

/// <summary>Shared command-input shape for a single ordered route step.</summary>
public sealed record RouteStepInput(
    Guid? ExistingStepId,
    string Name,
    string Instructions,
    IReadOnlyList<string> Resources,
    int EstimatedDurationMinutes);
