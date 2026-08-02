// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Application
//  File:           DrillsAndExercisesStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the DrillsAndExercises lifecycle-transition commands
//                  (Conduct, Complete, Cancel, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.PermitsStatusResult's precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.SafetyEvents.DrillsAndExercises.Commands;

/// <summary>Result payload shared by DrillsAndExercises lifecycle-transition commands.</summary>
/// <param name="DrillsAndExercisesId">Identity of the affected DrillsAndExercises aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record DrillsAndExercisesStatusResult(Guid DrillsAndExercisesId, string Status);
