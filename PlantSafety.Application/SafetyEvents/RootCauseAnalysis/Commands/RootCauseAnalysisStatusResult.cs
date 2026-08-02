// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Application
//  File:           RootCauseAnalysisStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the RootCauseAnalysis lifecycle commands (Create,
//                  SubmitForReview, Approve, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.PermitsStatusResult's
//                  precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Commands;

/// <summary>Result payload shared by RootCauseAnalysis lifecycle commands.</summary>
/// <param name="RootCauseAnalysisId">Identity of the affected RootCauseAnalysis aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record RootCauseAnalysisStatusResult(Guid RootCauseAnalysisId, string Status);
