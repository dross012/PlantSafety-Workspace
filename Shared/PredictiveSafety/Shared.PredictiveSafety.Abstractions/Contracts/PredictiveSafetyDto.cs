// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Contracts
//  File:           PredictiveSafetyDto.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. Flat, serialization-friendly DTO
//                  projected from PredictiveSafetyEvaluationResultContract
//                  for API/UI consumption -- deterministic mapping only.
// ============================================================================
namespace Shared.PredictiveSafety.Abstractions.Contracts;

/// <summary>Flat DTO projection of a PredictiveSafetyEvaluationResultContract.</summary>
public sealed record PredictiveSafetyEvaluationDto(
    string HazardType,
    string HazardSeverity,
    bool HazardRequiresEscalation,
    string HazardPriority,
    bool MaintenanceRequiresImmediate,
    string MaintenancePriority,
    bool RoutingRequiresPriority,
    string RoutingPriority,
    bool SchedulingRequiresAcceleration,
    string SchedulingPriority,
    bool AuditAnomalyDetected,
    string AuditPriority,
    string OverallPriority);

/// <summary>Deterministic, pure mapping from a PredictiveSafetyEvaluationResultContract to its DTO projection.</summary>
public static class PredictiveSafetyDtoMapper
{
    /// <summary>Projects <paramref name="result"/> into a PredictiveSafetyEvaluationDto.</summary>
    public static PredictiveSafetyEvaluationDto ToDto(PredictiveSafetyEvaluationResultContract result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new PredictiveSafetyEvaluationDto(
            HazardType: result.Hazard.PredictedHazardType.ToString(),
            HazardSeverity: result.Hazard.PredictedSeverity.ToString(),
            HazardRequiresEscalation: result.Hazard.RequiresImmediateEscalation,
            HazardPriority: result.Hazard.Priority.ToString(),
            MaintenanceRequiresImmediate: result.Maintenance.RequiresImmediateMaintenance,
            MaintenancePriority: result.Maintenance.Priority.ToString(),
            RoutingRequiresPriority: result.Routing.RequiresPriorityRouting,
            RoutingPriority: result.Routing.Priority.ToString(),
            SchedulingRequiresAcceleration: result.Scheduling.RequiresAcceleratedScheduling,
            SchedulingPriority: result.Scheduling.Priority.ToString(),
            AuditAnomalyDetected: result.Audit.AnomalyDetected,
            AuditPriority: result.Audit.Priority.ToString(),
            OverallPriority: result.OverallPriority.ToString());
    }
}
