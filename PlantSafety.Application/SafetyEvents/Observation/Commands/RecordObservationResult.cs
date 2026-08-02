// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Application
//  File:           RecordObservationResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for RecordObservationCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.SafetyEvents.Observation.Commands;

/// <summary>Result of a successful <see cref="RecordObservationCommand"/> execution.</summary>
/// <param name="ObservationId">Identity of the newly recorded Observation aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record RecordObservationResult(
    Guid ObservationId,
    string Status);
