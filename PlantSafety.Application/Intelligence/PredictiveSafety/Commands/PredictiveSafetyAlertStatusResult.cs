// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.PredictiveSafety
//  Layer:          Application
//  File:           PredictiveSafetyAlertStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the PredictiveSafetyAlert lifecycle commands (Create,
//                  Acknowledge, Resolve, Archive). Mirrors
//                  PlantSafety.Application.ContinuousImprovement.RCA.Commands.RCAStatusResult's
//                  precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Intelligence.PredictiveSafety.Commands;

/// <summary>Result payload shared by PredictiveSafetyAlert lifecycle commands.</summary>
/// <param name="PredictiveSafetyAlertId">Identity of the affected PredictiveSafetyAlert aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record PredictiveSafetyAlertStatusResult(Guid PredictiveSafetyAlertId, string Status);
