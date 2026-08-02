// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Application
//  File:           AITrainingJobStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the AITrainingJob lifecycle commands (Create, Start,
//                  Complete, Fail, Archive). Mirrors
//                  PlantSafety.Application.ContinuousImprovement.RCA.Commands.RCAStatusResult's
//                  precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Intelligence.AITraining.Commands;

/// <summary>Result payload shared by AITrainingJob lifecycle commands.</summary>
/// <param name="AITrainingJobId">Identity of the affected AITrainingJob aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record AITrainingJobStatusResult(Guid AITrainingJobId, string Status);
