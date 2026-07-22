// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetEnvironmentalAssetMonitoringQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Observability.EnvironmentalAssetMonitoring.Queries;
using PlantSafety.Application.Observability.EnvironmentalAssetMonitoring.Repositories;

namespace PlantSafety.Application.Observability.EnvironmentalAssetMonitoring.Handlers;

/// <summary>Handles <see cref="GetEnvironmentalAssetMonitoringQuery"/>.</summary>
public sealed class GetEnvironmentalAssetMonitoringQueryHandler : IRequestHandler<GetEnvironmentalAssetMonitoringQuery, Result<EnvironmentalAssetMonitoringQueryResult>>
{
    private readonly IEnvironmentalAssetMonitoringRepository _repository;
    private readonly ILogger<GetEnvironmentalAssetMonitoringQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEnvironmentalAssetMonitoringQueryHandler(IEnvironmentalAssetMonitoringRepository repository, ILogger<GetEnvironmentalAssetMonitoringQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EnvironmentalAssetMonitoringQueryResult>> Handle(GetEnvironmentalAssetMonitoringQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetEnvironmentalAssetMonitoringAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EnvironmentalAssetMonitoringQueryResult>.Success(new EnvironmentalAssetMonitoringQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EnvironmentalAssetMonitoring for Tenant {TenantId}", request.TenantId);

            return Result<EnvironmentalAssetMonitoringQueryResult>.Failure(new ApplicationError(
                "Observability.EnvironmentalAssetMonitoring.RetrievalFailed",
                "Unable to retrieve EnvironmentalAssetMonitoring data at this time.",
                Guid.NewGuid()));
        }
    }
}