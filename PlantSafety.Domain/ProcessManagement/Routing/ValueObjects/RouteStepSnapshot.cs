// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Domain
//  File:           RouteStepSnapshot.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Immutable value-object snapshot of a single RouteStep's content at the moment
//                  a RouteVersionSnapshot was captured. StepId is preserved across revisions when
//                  a step is edited in place (not removed/re-added), which is what lets
//                  RouteDiffEngine tell "modified" apart from "removed + added". Genuinely new --
//                  ProcessDefinition has no ordered-step concept to snapshot.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ProcessManagement.Routing.ValueObjects;

/// <summary>Immutable snapshot of a single step's content within a captured <c>RouteVersionSnapshot</c>.</summary>
/// <param name="StepId">Stable identity carried across revisions when the step is edited in place.</param>
/// <param name="StepNumber">1-based sequential position at the time of capture.</param>
/// <param name="Name">Step display name at the time of capture.</param>
/// <param name="Instructions">Step instructions at the time of capture.</param>
/// <param name="Resources">Assigned resource names at the time of capture.</param>
/// <param name="EstimatedDurationMinutes">Estimated execution duration at the time of capture.</param>
public sealed record RouteStepSnapshot(
    Guid StepId,
    int StepNumber,
    string Name,
    string Instructions,
    IReadOnlyList<string> Resources,
    int EstimatedDurationMinutes);
