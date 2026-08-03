// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Application
//  File:           WorkOrderToCanonicalMapper.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Maps the real WorkOrder aggregate to the enterprise canonical model
//                  (PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel.CanonicalWorkOrderV1
//                  /V2/V3) for enterprise integration consumers (ERP/MES/CMMS adapters, event
//                  mesh). Lives in this module's Application layer (not SharedKernel) because
//                  SharedKernel must never reference module Domain/Application types. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.Operational.WorkOrders.Aggregates;
using PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Operational.WorkOrders.Mappings;

/// <summary>Maps a real <see cref="WorkOrder"/> aggregate to the enterprise canonical work-order shapes.</summary>
public static class WorkOrderToCanonicalMapper
{
    /// <summary>Maps <paramref name="workOrder"/> to its V1 (stable) canonical projection.</summary>
    public static CanonicalWorkOrderV1 ToCanonicalV1(this WorkOrder workOrder)
    {
        ArgumentNullException.ThrowIfNull(workOrder);

        return new CanonicalWorkOrderV1
        {
            WorkOrderId = workOrder.WorkOrderId.Value,
            TenantId = workOrder.TenantId,
            SiteId = workOrder.SiteId,
            WorkOrderCode = workOrder.WorkOrderCode,
            AssetId = workOrder.MachineId,
            AssetCode = workOrder.MachineCode,
            Priority = workOrder.Priority.ToString(),
            Description = workOrder.Description,
            Status = workOrder.Status.ToString(),
            DueAtUtc = workOrder.DueAtUtc,
            CreatedAtUtc = workOrder.CreatedAtUtc,
            UpdatedAtUtc = workOrder.UpdatedAtUtc,
        };
    }

    /// <summary>Maps <paramref name="workOrder"/> to its V2 (extended) canonical projection.</summary>
    public static CanonicalWorkOrderV2 ToCanonicalV2(this WorkOrder workOrder)
    {
        ArgumentNullException.ThrowIfNull(workOrder);

        return new CanonicalWorkOrderV2
        {
            WorkOrderId = workOrder.WorkOrderId.Value,
            TenantId = workOrder.TenantId,
            SiteId = workOrder.SiteId,
            WorkOrderCode = workOrder.WorkOrderCode,
            AssetId = workOrder.MachineId,
            AssetCode = workOrder.MachineCode,
            Priority = workOrder.Priority.ToString(),
            Description = workOrder.Description,
            Status = workOrder.Status.ToString(),
            DueAtUtc = workOrder.DueAtUtc,
            CreatedAtUtc = workOrder.CreatedAtUtc,
            UpdatedAtUtc = workOrder.UpdatedAtUtc,
            TechnicianId = workOrder.TechnicianId,
            TechnicianName = workOrder.TechnicianName,
            AssignedAtUtc = workOrder.AssignedAtUtc,
            StartedAtUtc = workOrder.StartedAtUtc,
            OnHoldAtUtc = workOrder.OnHoldAtUtc,
            CompletedAtUtc = workOrder.CompletedAtUtc,
            CompletionNotes = workOrder.CompletionNotes,
            CancelledAtUtc = workOrder.CancelledAtUtc,
            CancelledReason = workOrder.CancelledReason,
        };
    }

    /// <summary>
    /// Maps <paramref name="workOrder"/> to its V3 (predictive) canonical projection. Predictive
    /// fields are populated only when <paramref name="predictedCompletionAtUtc"/> /
    /// <paramref name="predictedRiskScore"/> genuinely originate from a predictive-insights result
    /// (e.g. Scheduling.Rollup's PredictiveInsightsDto, itself backed by
    /// Operational.PredictiveMaintenance); callers with no genuine prediction should pass nulls
    /// rather than fabricate values.
    /// </summary>
    public static CanonicalWorkOrderV3 ToCanonicalV3(
        this WorkOrder workOrder,
        UtcTimestamp? predictedCompletionAtUtc = null,
        double? predictedRiskScore = null,
        string? predictionModelVersion = null,
        UtcTimestamp? predictionGeneratedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(workOrder);

        return new CanonicalWorkOrderV3
        {
            WorkOrderId = workOrder.WorkOrderId.Value,
            TenantId = workOrder.TenantId,
            SiteId = workOrder.SiteId,
            WorkOrderCode = workOrder.WorkOrderCode,
            AssetId = workOrder.MachineId,
            AssetCode = workOrder.MachineCode,
            Priority = workOrder.Priority.ToString(),
            Description = workOrder.Description,
            Status = workOrder.Status.ToString(),
            DueAtUtc = workOrder.DueAtUtc,
            CreatedAtUtc = workOrder.CreatedAtUtc,
            UpdatedAtUtc = workOrder.UpdatedAtUtc,
            TechnicianId = workOrder.TechnicianId,
            TechnicianName = workOrder.TechnicianName,
            AssignedAtUtc = workOrder.AssignedAtUtc,
            StartedAtUtc = workOrder.StartedAtUtc,
            OnHoldAtUtc = workOrder.OnHoldAtUtc,
            CompletedAtUtc = workOrder.CompletedAtUtc,
            CompletionNotes = workOrder.CompletionNotes,
            CancelledAtUtc = workOrder.CancelledAtUtc,
            CancelledReason = workOrder.CancelledReason,
            PredictedCompletionAtUtc = predictedCompletionAtUtc,
            PredictedRiskScore = predictedRiskScore,
            PredictionModelVersion = predictionModelVersion,
            PredictionGeneratedAtUtc = predictionGeneratedAtUtc,
        };
    }
}
