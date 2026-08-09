// ============================================================================
//  Project:        Enterprise.Dashboard
//  Module:         Dtos
//  File:           EnterprisePredictiveAlertsDto.cs
//  Created:        2026-08-08
//  Description:    Phase D.4: a summary of predictive-model outputs for
//                  the dashboard. TotalPredictionRecords is a real,
//                  deterministic count from Enterprise.DataLake; Alerts is
//                  caller-supplied human-readable text (e.g. produced by a
//                  real host running Shared.PredictiveModels against its
//                  own domain data) -- this project does not run
//                  predictive models itself.
// ============================================================================
namespace Enterprise.Dashboard.Dtos;

/// <summary>A summary of predictive-model outputs for the dashboard.</summary>
public sealed record EnterprisePredictiveAlertsDto(
    IReadOnlyList<string> Alerts,
    int TotalPredictionRecords);
