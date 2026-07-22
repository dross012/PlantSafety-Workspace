// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetEnvironmentalHealthMonitoringQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EmployeeSafety.EnvironmentalHealthMonitoring.Queries;
using PlantSafety.Application.EmployeeSafety.EnvironmentalHealthMonitoring.Repositories;

namespace PlantSafety.Application.EmployeeSafety.EnvironmentalHealthMonitoring.Handlers;

/// <summary>Handles <see cref="GetEnvironmentalHealthMonitoringQuery"/>.</summary>
public sealed class GetEnvironmentalHealthMonitoringQueryHandler : IRequestHandler<GetEnvironmentalHealthMonitoringQuery, Result<EnvironmentalHealthMonitoringQueryResult>>
{
    private readonly IEnvironmentalHealthMonitoringRepository _repository;
    private readonly ILogger<GetEnvironmentalHealthMonitoringQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEnvironmentalHealthMonitoringQueryHandler(IEnvironmentalHealthMonitoringRepository repository, ILogger<GetEnvironmentalHealthMonitoringQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EnvironmentalHealthMonitoringQueryResult>> Handle(GetEnvironmentalHealthMonitoringQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetEnvironmentalHealthMonitoringItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EnvironmentalHealthMonitoringQueryResult>.Success(new EnvironmentalHealthMonitoringQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EnvironmentalHealthMonitoring records for Tenant {TenantId}", request.TenantId);

            return Result<EnvironmentalHealthMonitoringQueryResult>.Failure(new ApplicationError(
                "EmployeeSafety.EnvironmentalHealthMonitoring.RetrievalFailed",
                "Unable to retrieve EnvironmentalHealthMonitoring records at this time.",
                Guid.NewGuid()));
        }
    }
}