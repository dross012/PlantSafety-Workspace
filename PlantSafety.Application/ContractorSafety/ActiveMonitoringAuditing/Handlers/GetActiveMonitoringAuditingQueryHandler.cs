// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetActiveMonitoringAuditingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ContractorSafety.ActiveMonitoringAuditing.Queries;
using PlantSafety.Application.ContractorSafety.ActiveMonitoringAuditing.Repositories;

namespace PlantSafety.Application.ContractorSafety.ActiveMonitoringAuditing.Handlers;

/// <summary>Handles <see cref="GetActiveMonitoringAuditingQuery"/>.</summary>
public sealed class GetActiveMonitoringAuditingQueryHandler : IRequestHandler<GetActiveMonitoringAuditingQuery, Result<ActiveMonitoringAuditingQueryResult>>
{
    private readonly IActiveMonitoringAuditingRepository _repository;
    private readonly ILogger<GetActiveMonitoringAuditingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetActiveMonitoringAuditingQueryHandler(IActiveMonitoringAuditingRepository repository, ILogger<GetActiveMonitoringAuditingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ActiveMonitoringAuditingQueryResult>> Handle(GetActiveMonitoringAuditingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetActiveMonitoringAuditingItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ActiveMonitoringAuditingQueryResult>.Success(new ActiveMonitoringAuditingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ActiveMonitoringAuditing records for Tenant {TenantId}", request.TenantId);

            return Result<ActiveMonitoringAuditingQueryResult>.Failure(new ApplicationError(
                "ContractorSafety.ActiveMonitoringAuditing.RetrievalFailed",
                "Unable to retrieve ActiveMonitoringAuditing records at this time.",
                Guid.NewGuid()));
        }
    }
}