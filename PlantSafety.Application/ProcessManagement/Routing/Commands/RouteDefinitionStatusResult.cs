// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           RouteDefinitionStatusResult.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the RouteDefinition lifecycle-transition commands
//                  (SubmitForReview, Approve, Retire, Archive, Revise). Mirrors
//                  PlantSafety.Application.ProcessManagement.Commands.ProcessDefinitionStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.ProcessManagement.Routing.Commands;

/// <summary>Result payload shared by RouteDefinition lifecycle-transition commands.</summary>
public sealed record RouteDefinitionStatusResult(Guid RouteDefinitionId, string Status, int Version);
