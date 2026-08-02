// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Application
//  File:           ObservationStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the Observation lifecycle-transition commands
//                  (Activate, Complete, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.PermitsStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.SafetyEvents.Observation.Commands;

/// <summary>Result payload shared by Observation lifecycle-transition commands.</summary>
/// <param name="ObservationId">Identity of the affected Observation aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record ObservationStatusResult(Guid ObservationId, string Status);
