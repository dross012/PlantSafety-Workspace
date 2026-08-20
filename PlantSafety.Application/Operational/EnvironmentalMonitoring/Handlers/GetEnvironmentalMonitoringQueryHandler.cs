// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-20
// FILE DESCRIPTION: Handler for GetEnvironmentalMonitoringQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.EnvironmentalMonitoring.Queries;
using PlantSafety.Application.Operational.EnvironmentalMonitoring.Repositories;

namespace PlantSafety.Application.Operational.EnvironmentalMonitoring.Handlers;

/// <summary>Handles <see cref="GetEnvironmentalMonitoringQuery"/>.</summary>
public sealed class GetEnvironmentalMonitoringQueryHandler : IRequestHandler<GetEnvironmentalMonitoringQuery, Result<EnvironmentalMonitoringQueryResult>>
{
    private readonly IEnvironmentalMonitoringRepository _repository;
    private readonly ILogger<GetEnvironmentalMonitoringQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEnvironmentalMonitoringQueryHandler(IEnvironmentalMonitoringRepository repository, ILogger<GetEnvironmentalMonitoringQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EnvironmentalMonitoringQueryResult>> Handle(GetEnvironmentalMonitoringQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetEnvironmentalMonitoringAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EnvironmentalMonitoringQueryResult>.Success(new EnvironmentalMonitoringQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EnvironmentalMonitoring for Tenant {TenantId}", request.TenantId);

            return Result<EnvironmentalMonitoringQueryResult>.Failure(new ApplicationError(
                "Operational.EnvironmentalMonitoring.RetrievalFailed",
                "Unable to retrieve EnvironmentalMonitoring data at this time.",
                Guid.NewGuid()));
        }
    }
}