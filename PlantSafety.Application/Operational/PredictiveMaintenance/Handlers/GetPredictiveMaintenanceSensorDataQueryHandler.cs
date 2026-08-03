// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           GetPredictiveMaintenanceSensorDataQueryHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handles GetPredictiveMaintenanceSensorDataQuery. Additive-only evolution.
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

/// <summary>Handles <see cref="GetPredictiveMaintenanceSensorDataQuery"/>.</summary>
public sealed class GetPredictiveMaintenanceSensorDataQueryHandler
    : IRequestHandler<GetPredictiveMaintenanceSensorDataQuery, Result<PaginatedResult<PredictiveMaintenanceSensorDataPointDto>>>
{
    private readonly IPredictiveMaintenanceAlertRepository _repository;
    private readonly PredictiveMaintenanceTelemetrySynthesizer _synthesizer = new();
    private readonly ILogger<GetPredictiveMaintenanceSensorDataQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPredictiveMaintenanceSensorDataQueryHandler(
        IPredictiveMaintenanceAlertRepository repository,
        ILogger<GetPredictiveMaintenanceSensorDataQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<PredictiveMaintenanceSensorDataPointDto>>> Handle(GetPredictiveMaintenanceSensorDataQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var equipmentId = request.EquipmentId ?? await ResolveMostRecentEquipmentIdAsync(request.TenantId, cancellationToken);
            var pageNumber = Math.Max(1, request.PageNumber);
            var pageSize = Math.Max(1, request.PageSize);

            if (equipmentId is null)
            {
                return Result<PaginatedResult<PredictiveMaintenanceSensorDataPointDto>>.Success(
                    PaginatedResult<PredictiveMaintenanceSensorDataPointDto>.Empty(pageNumber, pageSize));
            }

            var points = _synthesizer.GenerateSensorData(equipmentId.Value, DateTime.UtcNow, request.Channel);
            var page = points.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Result<PaginatedResult<PredictiveMaintenanceSensorDataPointDto>>.Success(
                new PaginatedResult<PredictiveMaintenanceSensorDataPointDto>(page, points.Count, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve sensor data for Tenant {TenantId} Equipment {EquipmentId}", request.TenantId, request.EquipmentId);

            return Result<PaginatedResult<PredictiveMaintenanceSensorDataPointDto>>.Failure(new ApplicationError(
                "Operational.PredictiveMaintenance.SensorData.RetrievalFailed",
                "Unable to retrieve sensor data at this time.",
                Guid.NewGuid()));
        }
    }

    private async Task<Guid?> ResolveMostRecentEquipmentIdAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var (items, _) = await _repository.GetPagedAsync(tenantId, equipmentId: null, activeOnly: false, pageNumber: 1, pageSize: 1, cancellationToken);
        return items.Count > 0 ? items[0].EquipmentId : null;
    }
}
