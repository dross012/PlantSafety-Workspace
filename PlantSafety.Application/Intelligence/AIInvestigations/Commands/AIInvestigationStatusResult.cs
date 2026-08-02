// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Application
//  File:           AIInvestigationStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the AIInvestigation lifecycle commands (Create,
//                  StartReview, Close, Archive). Mirrors
//                  PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Commands
//                  .RootCauseAnalysisStatusResult's precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Intelligence.AIInvestigations.Commands;

/// <summary>Result payload shared by AIInvestigation lifecycle commands.</summary>
/// <param name="AIInvestigationId">Identity of the affected AIInvestigation aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record AIInvestigationStatusResult(Guid AIInvestigationId, string Status);
