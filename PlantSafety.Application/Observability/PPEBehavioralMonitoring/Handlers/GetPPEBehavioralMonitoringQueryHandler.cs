// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetPPEBehavioralMonitoringQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Observability.PPEBehavioralMonitoring.Queries;
using PlantSafety.Application.Observability.PPEBehavioralMonitoring.Repositories;

namespace PlantSafety.Application.Observability.PPEBehavioralMonitoring.Handlers;

/// <summary>Handles <see cref="GetPPEBehavioralMonitoringQuery"/>.</summary>
public sealed class GetPPEBehavioralMonitoringQueryHandler : IRequestHandler<GetPPEBehavioralMonitoringQuery, Result<PPEBehavioralMonitoringQueryResult>>
{
    private readonly IPPEBehavioralMonitoringRepository _repository;
    private readonly ILogger<GetPPEBehavioralMonitoringQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPPEBehavioralMonitoringQueryHandler(IPPEBehavioralMonitoringRepository repository, ILogger<GetPPEBehavioralMonitoringQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PPEBehavioralMonitoringQueryResult>> Handle(GetPPEBehavioralMonitoringQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPPEBehavioralMonitoringAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PPEBehavioralMonitoringQueryResult>.Success(new PPEBehavioralMonitoringQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PPEBehavioralMonitoring for Tenant {TenantId}", request.TenantId);

            return Result<PPEBehavioralMonitoringQueryResult>.Failure(new ApplicationError(
                "Observability.PPEBehavioralMonitoring.RetrievalFailed",
                "Unable to retrieve PPEBehavioralMonitoring data at this time.",
                Guid.NewGuid()));
        }
    }
}