// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Handler for GetRiskDashboardQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Dashboard.RiskDashboard.Queries;
using PlantSafety.Application.Analytics.Dashboard.RiskDashboard.Repositories;

namespace PlantSafety.Application.Analytics.Dashboard.RiskDashboard.Handlers;

/// <summary>Handles <see cref="GetRiskDashboardQuery"/>.</summary>
public sealed class GetRiskDashboardQueryHandler : IRequestHandler<GetRiskDashboardQuery, Result<RiskDashboardQueryResult>>
{
    private readonly IRiskDashboardRepository _repository;
    private readonly ILogger<GetRiskDashboardQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRiskDashboardQueryHandler(IRiskDashboardRepository repository, ILogger<GetRiskDashboardQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RiskDashboardQueryResult>> Handle(GetRiskDashboardQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRiskDashboardAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<RiskDashboardQueryResult>.Success(new RiskDashboardQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve RiskDashboard for Tenant {TenantId}", request.TenantId);

            return Result<RiskDashboardQueryResult>.Failure(new ApplicationError(
                "Analytics.Dashboard.RiskDashboard.RetrievalFailed",
                "Unable to retrieve Risk dashboard data at this time.",
                Guid.NewGuid()));
        }
    }
}
