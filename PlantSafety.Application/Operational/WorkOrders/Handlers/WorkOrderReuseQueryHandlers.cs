// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Application
//  File:           WorkOrderReuseQueryHandlers.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    The two "genuinely overlapping" sub-resources identified by
//                  docs/Phase5/ui-gaps-scoping.md's Maintenance scoping pass: work-order predictive
//                  insights (delegates to the real Operational.PredictiveMaintenance module's
//                  IPredictiveMaintenanceAlertRepository + PredictiveMaintenanceInsightsCalculator)
//                  and the preventive task list (delegates via ISender to the real
//                  Operational.PreventiveMaintenance module's GetPreventiveMaintenanceQuery).
//                  Neither handler reimplements prediction/scheduling logic -- both call straight
//                  into the sibling module's existing, real Application-layer surface. Additive-only
//                  evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Extensions;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PredictiveMaintenance.Repositories;
using PlantSafety.Application.Operational.PredictiveMaintenance.Services;
using PlantSafety.Application.Operational.PreventiveMaintenance.Queries;
using PlantSafety.Application.Operational.WorkOrders.Models;
using PlantSafety.Application.Operational.WorkOrders.Queries;
using PlantSafety.Application.Operational.WorkOrders.Repositories;

namespace PlantSafety.Application.Operational.WorkOrders.Handlers;

/// <summary>
/// Handles <see cref="GetWorkOrderPredictiveInsightsQuery"/> by delegating to the real
/// Operational.PredictiveMaintenance module's alert repository and insights calculator, scoped to
/// the work order's target machine (equipment) identifier.
/// </summary>
public sealed class GetWorkOrderPredictiveInsightsQueryHandler(
    IWorkOrderReadRepository workOrderRepository,
    IPredictiveMaintenanceAlertRepository predictiveAlertRepository,
    ILogger<GetWorkOrderPredictiveInsightsQueryHandler> logger)
    : IRequestHandler<GetWorkOrderPredictiveInsightsQuery, Result<MaintenancePredictiveInsightsDto>>
{
    private readonly PredictiveMaintenanceInsightsCalculator _calculator = new();

    /// <inheritdoc/>
    public async Task<Result<MaintenancePredictiveInsightsDto>> Handle(GetWorkOrderPredictiveInsightsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var workOrder = await workOrderRepository.GetByIdAsync(request.TenantId, request.WorkOrderId, cancellationToken);
            if (workOrder is null)
            {
                return Result<MaintenancePredictiveInsightsDto>.Failure(new ApplicationError(
                    "Operational.WorkOrders.NotFound",
                    "The requested work order could not be found.",
                    request.WorkOrderId,
                    ErrorSeverity.NotFound));
            }

            var (alerts, _) = await predictiveAlertRepository.GetPagedAsync(
                request.TenantId, equipmentId: workOrder.MachineId, activeOnly: false, pageNumber: 1, pageSize: 50, cancellationToken);

            var now = DateTime.UtcNow;
            var latest = alerts.OrderByDescending(a => a.PredictedAtUtc).FirstOrDefault();

            if (latest is null)
            {
                return Result<MaintenancePredictiveInsightsDto>.Success(new MaintenancePredictiveInsightsDto
                {
                    RiskScore = 0.0,
                    ContributingFactors = [],
                    RecommendedAction = "No predictive maintenance alerts recorded for this machine.",
                    ModelVersion = _calculator.Predictor.ModelVersion,
                    GeneratedAtUtc = now,
                });
            }

            var prediction = _calculator.PredictForAlert(latest.Status, latest.PredictedAtUtc, now);
            var factors = PredictiveMaintenanceInsightsCalculator.BuildFeatureImportance()
                .OrderByDescending(f => f.Contribution)
                .Select(f => f.FeatureName)
                .ToList();

            return Result<MaintenancePredictiveInsightsDto>.Success(new MaintenancePredictiveInsightsDto
            {
                RiskScore = Math.Round(prediction.Output.FailureLikelihood, 3),
                ContributingFactors = factors,
                RecommendedAction = $"Review predicted {latest.PredictedFailureType} risk and schedule corrective maintenance before the forecasted window.",
                ModelVersion = prediction.ModelVersion,
                GeneratedAtUtc = now,
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve predictive insights for WorkOrder {WorkOrderId}", request.WorkOrderId);
            return Result<MaintenancePredictiveInsightsDto>.Failure(new ApplicationError(
                "Operational.WorkOrders.PredictiveInsightsFailed", "Unable to retrieve predictive maintenance insights at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>
/// Handles <see cref="GetWorkOrderPreventiveTasksQuery"/> by delegating to the real
/// Operational.PreventiveMaintenance module's GetPreventiveMaintenanceQuery over a rolling
/// [-365d, +90d] window, then paginating/mapping the results into <see cref="PreventiveTaskDto"/>.
/// </summary>
public sealed class GetWorkOrderPreventiveTasksQueryHandler(ISender sender, ILogger<GetWorkOrderPreventiveTasksQueryHandler> logger)
    : IRequestHandler<GetWorkOrderPreventiveTasksQuery, Result<PaginatedResult<PreventiveTaskDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<PreventiveTaskDto>>> Handle(GetWorkOrderPreventiveTasksQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = request.PageNumber.ClampPageNumber();
            var pageSize = request.PageSize.ClampPageSize();
            var now = DateTime.UtcNow;

            var innerResult = await sender.Send(
                new GetPreventiveMaintenanceQuery(request.TenantId, SiteId: null, From: now.AddDays(-365), To: now.AddDays(90)),
                cancellationToken);

            if (innerResult.IsFailure)
            {
                return Result<PaginatedResult<PreventiveTaskDto>>.Failure(innerResult.Error);
            }

            var allItems = innerResult.Value.Items
                .OrderBy(i => i.ScheduledAtUtc)
                .Select(i => new PreventiveTaskDto
                {
                    TaskId = i.PreventiveMaintenanceTaskId,
                    MachineCode = $"M-{i.EquipmentId.ToString("N")[..6].ToUpperInvariant()}",
                    Description = i.TaskDescription,
                    TriggerType = "Time",
                    DueAtUtc = new DateTimeOffset(DateTime.SpecifyKind(i.ScheduledAtUtc, DateTimeKind.Utc)),
                    IsOverdue = i.Status == "Overdue" || (i.Status == "Scheduled" && i.ScheduledAtUtc < now),
                })
                .ToList();

            var total = allItems.Count;
            var page = allItems.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Result<PaginatedResult<PreventiveTaskDto>>.Success(new PaginatedResult<PreventiveTaskDto>(page, total, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve preventive tasks for Tenant {TenantId}", request.TenantId);
            return Result<PaginatedResult<PreventiveTaskDto>>.Failure(new ApplicationError(
                "Operational.WorkOrders.PreventiveTasksFailed", "Unable to retrieve preventive maintenance tasks at this time.", Guid.NewGuid()));
        }
    }
}
