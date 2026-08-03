// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           GetPredictiveMaintenanceSensorAnomaliesQueryHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handles GetPredictiveMaintenanceSensorAnomaliesQuery. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;
using PlantSafety.Application.Operational.PredictiveMaintenance.Queries;
using PlantSafety.Application.Operational.PredictiveMaintenance.Repositories;
using PlantSafety.Application.Operational.PredictiveMaintenance.Services;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Handlers;

/// <summary>Handles <see cref="GetPredictiveMaintenanceSensorAnomaliesQuery"/>.</summary>
public sealed class GetPredictiveMaintenanceSensorAnomaliesQueryHandler
    : IRequestHandler<GetPredictiveMaintenanceSensorAnomaliesQuery, Result<PaginatedResult<PredictiveMaintenanceSensorAnomalyDto>>>
{
    private readonly IPredictiveMaintenanceAlertRepository _repository;
    private readonly PredictiveMaintenanceTelemetrySynthesizer _synthesizer = new();
    private readonly ILogger<GetPredictiveMaintenanceSensorAnomaliesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPredictiveMaintenanceSensorAnomaliesQueryHandler(
        IPredictiveMaintenanceAlertRepository repository,
        ILogger<GetPredictiveMaintenanceSensorAnomaliesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<PredictiveMaintenanceSensorAnomalyDto>>> Handle(GetPredictiveMaintenanceSensorAnomaliesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var equipmentId = request.EquipmentId ?? await ResolveMostRecentEquipmentIdAsync(request.TenantId, cancellationToken);
            var pageNumber = Math.Max(1, request.PageNumber);
            var pageSize = Math.Max(1, request.PageSize);

            if (equipmentId is null)
            {
                return Result<PaginatedResult<PredictiveMaintenanceSensorAnomalyDto>>.Success(
                    PaginatedResult<PredictiveMaintenanceSensorAnomalyDto>.Empty(pageNumber, pageSize));
            }

            var anomalies = _synthesizer.DetectAnomalies(equipmentId.Value, DateTime.UtcNow);
            var page = anomalies.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Result<PaginatedResult<PredictiveMaintenanceSensorAnomalyDto>>.Success(
                new PaginatedResult<PredictiveMaintenanceSensorAnomalyDto>(page, anomalies.Count, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect sensor anomalies for Tenant {TenantId} Equipment {EquipmentId}", request.TenantId, request.EquipmentId);

            return Result<PaginatedResult<PredictiveMaintenanceSensorAnomalyDto>>.Failure(new ApplicationError(
                "Operational.PredictiveMaintenance.SensorAnomalies.RetrievalFailed",
                "Unable to retrieve sensor anomalies at this time.",
                Guid.NewGuid()));
        }
    }

    private async Task<Guid?> ResolveMostRecentEquipmentIdAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var (items, _) = await _repository.GetPagedAsync(tenantId, equipmentId: null, activeOnly: false, pageNumber: 1, pageSize: 1, cancellationToken);
        return items.Count > 0 ? items[0].EquipmentId : null;
    }
}
