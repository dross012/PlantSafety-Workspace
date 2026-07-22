// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetFacilityTrafficMonitoringQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Observability.FacilityTrafficMonitoring.Queries;
using PlantSafety.Application.Observability.FacilityTrafficMonitoring.Repositories;

namespace PlantSafety.Application.Observability.FacilityTrafficMonitoring.Handlers;

/// <summary>Handles <see cref="GetFacilityTrafficMonitoringQuery"/>.</summary>
public sealed class GetFacilityTrafficMonitoringQueryHandler : IRequestHandler<GetFacilityTrafficMonitoringQuery, Result<FacilityTrafficMonitoringQueryResult>>
{
    private readonly IFacilityTrafficMonitoringRepository _repository;
    private readonly ILogger<GetFacilityTrafficMonitoringQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetFacilityTrafficMonitoringQueryHandler(IFacilityTrafficMonitoringRepository repository, ILogger<GetFacilityTrafficMonitoringQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<FacilityTrafficMonitoringQueryResult>> Handle(GetFacilityTrafficMonitoringQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetFacilityTrafficMonitoringAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<FacilityTrafficMonitoringQueryResult>.Success(new FacilityTrafficMonitoringQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve FacilityTrafficMonitoring for Tenant {TenantId}", request.TenantId);

            return Result<FacilityTrafficMonitoringQueryResult>.Failure(new ApplicationError(
                "Observability.FacilityTrafficMonitoring.RetrievalFailed",
                "Unable to retrieve FacilityTrafficMonitoring data at this time.",
                Guid.NewGuid()));
        }
    }
}