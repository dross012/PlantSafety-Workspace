// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetMaintenanceQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Procedures.Maintenance.Queries;
using PlantSafety.Application.Compliance.Procedures.Maintenance.Repositories;

namespace PlantSafety.Application.Compliance.Procedures.Maintenance.Handlers;

/// <summary>Handles <see cref="GetMaintenanceQuery"/>.</summary>
public sealed class GetMaintenanceQueryHandler : IRequestHandler<GetMaintenanceQuery, Result<MaintenanceQueryResult>>
{
    private readonly IMaintenanceRepository _repository;
    private readonly ILogger<GetMaintenanceQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetMaintenanceQueryHandler(IMaintenanceRepository repository, ILogger<GetMaintenanceQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MaintenanceQueryResult>> Handle(GetMaintenanceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetMaintenanceProceduresAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<MaintenanceQueryResult>.Success(new MaintenanceQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Maintenance for Tenant {TenantId}", request.TenantId);

            return Result<MaintenanceQueryResult>.Failure(new ApplicationError(
                "Compliance.Procedures.Maintenance.RetrievalFailed",
                "Unable to retrieve Maintenance data at this time.",
                Guid.NewGuid()));
        }
    }
}
