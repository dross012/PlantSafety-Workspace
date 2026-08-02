// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Application
//  File:           GetPreventiveMaintenanceTaskByIdQueryHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for GetPreventiveMaintenanceTaskByIdQuery. Reads from the read-side
//                  repository (optimised stored proc; no domain-aggregate reconstruction). Returns
//                  a NotFound failure both when the row does not exist and when it belongs to a
//                  different tenant, so tenant membership is never revealed to an unauthorized
//                  caller. tenantId is pushed into the repository call itself
//                  (usp_PreventiveMaintenanceTask_GetById's WHERE clause), not just checked here
//                  after the fact -- the post-fetch TenantId comparison below is defense-in-depth,
//                  not the primary control. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PreventiveMaintenance.Models;
using PlantSafety.Application.Operational.PreventiveMaintenance.Queries;
using PlantSafety.Application.Operational.PreventiveMaintenance.Repositories;

namespace PlantSafety.Application.Operational.PreventiveMaintenance.Handlers;

/// <summary>Handles <see cref="GetPreventiveMaintenanceTaskByIdQuery"/>.</summary>
public sealed class GetPreventiveMaintenanceTaskByIdQueryHandler : IRequestHandler<GetPreventiveMaintenanceTaskByIdQuery, Result<PreventiveMaintenanceTaskItemDto>>
{
    private readonly IPreventiveMaintenanceTaskRepository _repository;
    private readonly ILogger<GetPreventiveMaintenanceTaskByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPreventiveMaintenanceTaskByIdQueryHandler(IPreventiveMaintenanceTaskRepository repository, ILogger<GetPreventiveMaintenanceTaskByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PreventiveMaintenanceTaskItemDto>> Handle(GetPreventiveMaintenanceTaskByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.PreventiveMaintenanceTaskId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("PreventiveMaintenanceTask {PreventiveMaintenanceTaskId} not found for Tenant {TenantId}", request.PreventiveMaintenanceTaskId, request.TenantId);

                return Result<PreventiveMaintenanceTaskItemDto>.Failure(new ApplicationError(
                    "Operational.PreventiveMaintenance.NotFound",
                    "The requested preventive maintenance task could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<PreventiveMaintenanceTaskItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PreventiveMaintenanceTask {PreventiveMaintenanceTaskId}", request.PreventiveMaintenanceTaskId);

            return Result<PreventiveMaintenanceTaskItemDto>.Failure(new ApplicationError(
                "Operational.PreventiveMaintenance.RetrievalFailed",
                "Unable to retrieve preventive maintenance task data at this time.",
                Guid.NewGuid()));
        }
    }
}
