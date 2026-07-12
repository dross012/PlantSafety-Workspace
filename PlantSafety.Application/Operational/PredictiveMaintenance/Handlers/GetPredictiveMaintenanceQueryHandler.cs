// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetPredictiveMaintenanceQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PredictiveMaintenance.Queries;
using PlantSafety.Application.Operational.PredictiveMaintenance.Repositories;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Handlers;

/// <summary>Handles <see cref="GetPredictiveMaintenanceQuery"/>.</summary>
public sealed class GetPredictiveMaintenanceQueryHandler : IRequestHandler<GetPredictiveMaintenanceQuery, Result<PredictiveMaintenanceQueryResult>>
{
    private readonly IPredictiveMaintenanceRepository _repository;
    private readonly ILogger<GetPredictiveMaintenanceQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPredictiveMaintenanceQueryHandler(IPredictiveMaintenanceRepository repository, ILogger<GetPredictiveMaintenanceQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PredictiveMaintenanceQueryResult>> Handle(GetPredictiveMaintenanceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPredictiveMaintenanceAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PredictiveMaintenanceQueryResult>.Success(new PredictiveMaintenanceQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PredictiveMaintenance for Tenant {TenantId}", request.TenantId);

            return Result<PredictiveMaintenanceQueryResult>.Failure(new ApplicationError(
                "Operational.PredictiveMaintenance.RetrievalFailed",
                "Unable to retrieve PredictiveMaintenance data at this time.",
                Guid.NewGuid()));
        }
    }
}
