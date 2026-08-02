// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Application
//  File:           RCAStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the RCA lifecycle commands (Create, StartAnalysis,
//                  SubmitForApproval, Close, Archive). Mirrors
//                  PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Commands
//                  .RootCauseAnalysisStatusResult's precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.ContinuousImprovement.RCA.Commands;

/// <summary>Result payload shared by RCA lifecycle commands.</summary>
/// <param name="RcaId">Identity of the affected RCA aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record RCAStatusResult(Guid RcaId, string Status);
