// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Handler for GetProductionDashboardQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Dashboard.ProductionDashboard.Queries;
using PlantSafety.Application.Analytics.Dashboard.ProductionDashboard.Repositories;

namespace PlantSafety.Application.Analytics.Dashboard.ProductionDashboard.Handlers;

/// <summary>Handles <see cref="GetProductionDashboardQuery"/>.</summary>
public sealed class GetProductionDashboardQueryHandler : IRequestHandler<GetProductionDashboardQuery, Result<ProductionDashboardQueryResult>>
{
    private readonly IProductionDashboardRepository _repository;
    private readonly ILogger<GetProductionDashboardQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetProductionDashboardQueryHandler(IProductionDashboardRepository repository, ILogger<GetProductionDashboardQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ProductionDashboardQueryResult>> Handle(GetProductionDashboardQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetProductionDashboardAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ProductionDashboardQueryResult>.Success(new ProductionDashboardQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ProductionDashboard for Tenant {TenantId}", request.TenantId);

            return Result<ProductionDashboardQueryResult>.Failure(new ApplicationError(
                "Analytics.Dashboard.ProductionDashboard.RetrievalFailed",
                "Unable to retrieve Production dashboard data at this time.",
                Guid.NewGuid()));
        }
    }
}
