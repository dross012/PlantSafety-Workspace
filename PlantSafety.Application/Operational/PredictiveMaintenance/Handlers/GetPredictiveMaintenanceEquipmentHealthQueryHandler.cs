// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           GetPredictiveMaintenanceEquipmentHealthQueryHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handles GetPredictiveMaintenanceEquipmentHealthQuery by bucketing each
//                  equipment's most recent PredictiveMaintenanceAlert into Healthy/Warning/
//                  Critical. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Constants;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;
using PlantSafety.Application.Operational.PredictiveMaintenance.Queries;
using PlantSafety.Application.Operational.PredictiveMaintenance.Repositories;
using PlantSafety.Application.Operational.PredictiveMaintenance.Services;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Handlers;

/// <summary>Handles <see cref="GetPredictiveMaintenanceEquipmentHealthQuery"/>.</summary>
public sealed class GetPredictiveMaintenanceEquipmentHealthQueryHandler
    : IRequestHandler<GetPredictiveMaintenanceEquipmentHealthQuery, Result<PredictiveMaintenanceEquipmentHealthDto>>
{
    private readonly IPredictiveMaintenanceAlertRepository _repository;
    private readonly ILogger<GetPredictiveMaintenanceEquipmentHealthQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPredictiveMaintenanceEquipmentHealthQueryHandler(
        IPredictiveMaintenanceAlertRepository repository,
        ILogger<GetPredictiveMaintenanceEquipmentHealthQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PredictiveMaintenanceEquipmentHealthDto>> Handle(GetPredictiveMaintenanceEquipmentHealthQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (items, _) = await _repository.GetPagedAsync(
                request.TenantId, equipmentId: null, activeOnly: false, pageNumber: 1, pageSize: PaginationConstants.MaxPageSize, cancellationToken);

            var latestPerEquipment = items
                .GroupBy(i => i.EquipmentId)
                .Select(g => g.OrderByDescending(i => i.PredictedAtUtc).First())
                .ToList();

            if (latestPerEquipment.Count == 0)
            {
                return Result<PredictiveMaintenanceEquipmentHealthDto>.Success(new PredictiveMaintenanceEquipmentHealthDto
                {
                    HealthyPercent = 100,
                    WarningPercent = 0,
                    CriticalPercent = 0,
                });
            }

            var healthy = 0;
            var warning = 0;
            var critical = 0;

            foreach (var item in latestPerEquipment)
            {
                switch (PredictiveMaintenanceInsightsCalculator.ClassifyStatus(item.Status))
                {
                    case PredictiveMaintenanceRiskBucket.Low: healthy++; break;
                    case PredictiveMaintenanceRiskBucket.Medium: warning++; break;
                    default: critical++; break;
                }
            }

            var total = (double)latestPerEquipment.Count;

            return Result<PredictiveMaintenanceEquipmentHealthDto>.Success(new PredictiveMaintenanceEquipmentHealthDto
            {
                HealthyPercent = Math.Round(healthy / total * 100.0, 1),
                WarningPercent = Math.Round(warning / total * 100.0, 1),
                CriticalPercent = Math.Round(critical / total * 100.0, 1),
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute PredictiveMaintenance equipment health for Tenant {TenantId}", request.TenantId);

            return Result<PredictiveMaintenanceEquipmentHealthDto>.Failure(new ApplicationError(
                "Operational.PredictiveMaintenance.EquipmentHealth.RetrievalFailed",
                "Unable to retrieve the predictive maintenance equipment health summary at this time.",
                Guid.NewGuid()));
        }
    }
}
