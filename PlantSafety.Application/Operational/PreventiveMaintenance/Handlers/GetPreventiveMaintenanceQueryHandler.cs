// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetPreventiveMaintenanceQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PreventiveMaintenance.Queries;
using PlantSafety.Application.Operational.PreventiveMaintenance.Repositories;

namespace PlantSafety.Application.Operational.PreventiveMaintenance.Handlers;

/// <summary>Handles <see cref="GetPreventiveMaintenanceQuery"/>.</summary>
public sealed class GetPreventiveMaintenanceQueryHandler : IRequestHandler<GetPreventiveMaintenanceQuery, Result<PreventiveMaintenanceQueryResult>>
{
    private readonly IPreventiveMaintenanceRepository _repository;
    private readonly ILogger<GetPreventiveMaintenanceQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPreventiveMaintenanceQueryHandler(IPreventiveMaintenanceRepository repository, ILogger<GetPreventiveMaintenanceQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PreventiveMaintenanceQueryResult>> Handle(GetPreventiveMaintenanceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPreventiveMaintenanceAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PreventiveMaintenanceQueryResult>.Success(new PreventiveMaintenanceQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PreventiveMaintenance for Tenant {TenantId}", request.TenantId);

            return Result<PreventiveMaintenanceQueryResult>.Failure(new ApplicationError(
                "Operational.PreventiveMaintenance.RetrievalFailed",
                "Unable to retrieve PreventiveMaintenance data at this time.",
                Guid.NewGuid()));
        }
    }
}
