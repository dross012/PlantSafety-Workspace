// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           GetPredictiveMaintenanceAlertByIdQueryHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for GetPredictiveMaintenanceAlertByIdQuery. Reads from the read-side
//                  repository (optimised stored proc; no domain-aggregate reconstruction). Returns
//                  a NotFound failure both when the row does not exist and when it belongs to a
//                  different tenant, so tenant membership is never revealed to an unauthorized
//                  caller. tenantId is pushed into the repository call itself
//                  (usp_PredictiveMaintenanceAlert_GetById's WHERE clause), not just checked here
//                  after the fact -- the post-fetch TenantId comparison below is defense-in-depth,
//                  not the primary control. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;
using PlantSafety.Application.Operational.PredictiveMaintenance.Queries;
using PlantSafety.Application.Operational.PredictiveMaintenance.Repositories;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Handlers;

/// <summary>Handles <see cref="GetPredictiveMaintenanceAlertByIdQuery"/>.</summary>
public sealed class GetPredictiveMaintenanceAlertByIdQueryHandler : IRequestHandler<GetPredictiveMaintenanceAlertByIdQuery, Result<PredictiveMaintenanceAlertItemDto>>
{
    private readonly IPredictiveMaintenanceAlertRepository _repository;
    private readonly ILogger<GetPredictiveMaintenanceAlertByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPredictiveMaintenanceAlertByIdQueryHandler(IPredictiveMaintenanceAlertRepository repository, ILogger<GetPredictiveMaintenanceAlertByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PredictiveMaintenanceAlertItemDto>> Handle(GetPredictiveMaintenanceAlertByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.PredictiveMaintenanceAlertId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("PredictiveMaintenanceAlert {PredictiveMaintenanceAlertId} not found for Tenant {TenantId}", request.PredictiveMaintenanceAlertId, request.TenantId);

                return Result<PredictiveMaintenanceAlertItemDto>.Failure(new ApplicationError(
                    "Operational.PredictiveMaintenance.NotFound",
                    "The requested predictive maintenance alert could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<PredictiveMaintenanceAlertItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PredictiveMaintenanceAlert {PredictiveMaintenanceAlertId}", request.PredictiveMaintenanceAlertId);

            return Result<PredictiveMaintenanceAlertItemDto>.Failure(new ApplicationError(
                "Operational.PredictiveMaintenance.RetrievalFailed",
                "Unable to retrieve predictive maintenance alert data at this time.",
                Guid.NewGuid()));
        }
    }
}
