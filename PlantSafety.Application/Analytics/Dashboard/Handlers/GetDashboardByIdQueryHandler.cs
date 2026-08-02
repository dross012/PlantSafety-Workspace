// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Application
//  File:           GetDashboardByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetDashboardByIdQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-aggregate reconstruction). Returns a NotFound
//                  failure both when the row does not exist and when it belongs to a different
//                  tenant. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Dashboard.Models;
using PlantSafety.Application.Analytics.Dashboard.Queries;
using PlantSafety.Application.Analytics.Dashboard.Repositories;

namespace PlantSafety.Application.Analytics.Dashboard.Handlers;

/// <summary>Handles <see cref="GetDashboardByIdQuery"/>.</summary>
public sealed class GetDashboardByIdQueryHandler : IRequestHandler<GetDashboardByIdQuery, Result<DashboardItemDto>>
{
    private readonly IDashboardReadRepository _repository;
    private readonly ILogger<GetDashboardByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDashboardByIdQueryHandler(IDashboardReadRepository repository, ILogger<GetDashboardByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DashboardItemDto>> Handle(GetDashboardByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.DashboardId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("Dashboard {DashboardId} not found for Tenant {TenantId}", request.DashboardId, request.TenantId);

                return Result<DashboardItemDto>.Failure(new ApplicationError(
                    "Analytics.Dashboard.NotFound",
                    "The requested dashboard could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<DashboardItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Dashboard {DashboardId}", request.DashboardId);

            return Result<DashboardItemDto>.Failure(new ApplicationError(
                "Analytics.Dashboard.RetrievalFailed",
                "Unable to retrieve dashboard data at this time.",
                Guid.NewGuid()));
        }
    }
}
