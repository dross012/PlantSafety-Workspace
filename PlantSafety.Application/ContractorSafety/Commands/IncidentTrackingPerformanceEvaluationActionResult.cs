// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for IncidentTrackingPerformanceEvaluation lifecycle commands ("ContractorSafety.IncidentTrackingPerformanceEvaluation").
// SchemaVersion: 1

namespace PlantSafety.Application.ContractorSafety.Commands;

/// <summary>Result of a successful IncidentTrackingPerformanceEvaluation lifecycle command execution.</summary>
/// <param name="IncidentTrackingPerformanceEvaluationId">Identity of the affected IncidentTrackingPerformanceEvaluation aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record IncidentTrackingPerformanceEvaluationActionResult(
    Guid IncidentTrackingPerformanceEvaluationId,
    string Status);