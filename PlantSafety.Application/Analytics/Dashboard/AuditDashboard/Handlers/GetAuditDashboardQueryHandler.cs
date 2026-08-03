// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Handler for GetAuditDashboardQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Dashboard.AuditDashboard.Queries;
using PlantSafety.Application.Analytics.Dashboard.AuditDashboard.Repositories;

namespace PlantSafety.Application.Analytics.Dashboard.AuditDashboard.Handlers;

/// <summary>Handles <see cref="GetAuditDashboardQuery"/>.</summary>
public sealed class GetAuditDashboardQueryHandler : IRequestHandler<GetAuditDashboardQuery, Result<AuditDashboardQueryResult>>
{
    private readonly IAuditDashboardRepository _repository;
    private readonly ILogger<GetAuditDashboardQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAuditDashboardQueryHandler(IAuditDashboardRepository repository, ILogger<GetAuditDashboardQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AuditDashboardQueryResult>> Handle(GetAuditDashboardQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetAuditDashboardAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<AuditDashboardQueryResult>.Success(new AuditDashboardQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve AuditDashboard for Tenant {TenantId}", request.TenantId);

            return Result<AuditDashboardQueryResult>.Failure(new ApplicationError(
                "Analytics.Dashboard.AuditDashboard.RetrievalFailed",
                "Unable to retrieve Audit dashboard data at this time.",
                Guid.NewGuid()));
        }
    }
}
