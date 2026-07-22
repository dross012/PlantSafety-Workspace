// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetMachineMaintenanceInspectionQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyPolicies.MachineMaintenanceInspection.Queries;
using PlantSafety.Application.SafetyPolicies.MachineMaintenanceInspection.Repositories;

namespace PlantSafety.Application.SafetyPolicies.MachineMaintenanceInspection.Handlers;

/// <summary>Handles <see cref="GetMachineMaintenanceInspectionQuery"/>.</summary>
public sealed class GetMachineMaintenanceInspectionQueryHandler : IRequestHandler<GetMachineMaintenanceInspectionQuery, Result<MachineMaintenanceInspectionQueryResult>>
{
    private readonly IMachineMaintenanceInspectionRepository _repository;
    private readonly ILogger<GetMachineMaintenanceInspectionQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetMachineMaintenanceInspectionQueryHandler(IMachineMaintenanceInspectionRepository repository, ILogger<GetMachineMaintenanceInspectionQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MachineMaintenanceInspectionQueryResult>> Handle(GetMachineMaintenanceInspectionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetMachineMaintenanceInspectionAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<MachineMaintenanceInspectionQueryResult>.Success(new MachineMaintenanceInspectionQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve MachineMaintenanceInspection for Tenant {TenantId}", request.TenantId);

            return Result<MachineMaintenanceInspectionQueryResult>.Failure(new ApplicationError(
                "SafetyPolicies.MachineMaintenanceInspection.RetrievalFailed",
                "Unable to retrieve MachineMaintenanceInspection data at this time.",
                Guid.NewGuid()));
        }
    }
}