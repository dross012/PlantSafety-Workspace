// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetMachineProcessMonitoringQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Observability.MachineProcessMonitoring.Queries;
using PlantSafety.Application.Observability.MachineProcessMonitoring.Repositories;

namespace PlantSafety.Application.Observability.MachineProcessMonitoring.Handlers;

/// <summary>Handles <see cref="GetMachineProcessMonitoringQuery"/>.</summary>
public sealed class GetMachineProcessMonitoringQueryHandler : IRequestHandler<GetMachineProcessMonitoringQuery, Result<MachineProcessMonitoringQueryResult>>
{
    private readonly IMachineProcessMonitoringRepository _repository;
    private readonly ILogger<GetMachineProcessMonitoringQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetMachineProcessMonitoringQueryHandler(IMachineProcessMonitoringRepository repository, ILogger<GetMachineProcessMonitoringQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MachineProcessMonitoringQueryResult>> Handle(GetMachineProcessMonitoringQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetMachineProcessMonitoringAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<MachineProcessMonitoringQueryResult>.Success(new MachineProcessMonitoringQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve MachineProcessMonitoring for Tenant {TenantId}", request.TenantId);

            return Result<MachineProcessMonitoringQueryResult>.Failure(new ApplicationError(
                "Observability.MachineProcessMonitoring.RetrievalFailed",
                "Unable to retrieve MachineProcessMonitoring data at this time.",
                Guid.NewGuid()));
        }
    }
}