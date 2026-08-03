// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           CreateRouteDefinitionResult.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateRouteDefinitionCommand. Mirrors
//                  PlantSafety.Application.ProcessManagement.Commands.CreateProcessDefinitionResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.ProcessManagement.Routing.Commands;

/// <summary>Result of a successful <see cref="CreateRouteDefinitionCommand"/> execution.</summary>
public sealed record CreateRouteDefinitionResult(
    Guid RouteDefinitionId,
    string Status);
