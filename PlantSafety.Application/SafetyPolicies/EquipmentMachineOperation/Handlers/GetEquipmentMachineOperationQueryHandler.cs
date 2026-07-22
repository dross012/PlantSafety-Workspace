// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetEquipmentMachineOperationQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyPolicies.EquipmentMachineOperation.Queries;
using PlantSafety.Application.SafetyPolicies.EquipmentMachineOperation.Repositories;

namespace PlantSafety.Application.SafetyPolicies.EquipmentMachineOperation.Handlers;

/// <summary>Handles <see cref="GetEquipmentMachineOperationQuery"/>.</summary>
public sealed class GetEquipmentMachineOperationQueryHandler : IRequestHandler<GetEquipmentMachineOperationQuery, Result<EquipmentMachineOperationQueryResult>>
{
    private readonly IEquipmentMachineOperationRepository _repository;
    private readonly ILogger<GetEquipmentMachineOperationQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEquipmentMachineOperationQueryHandler(IEquipmentMachineOperationRepository repository, ILogger<GetEquipmentMachineOperationQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EquipmentMachineOperationQueryResult>> Handle(GetEquipmentMachineOperationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetEquipmentMachineOperationAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EquipmentMachineOperationQueryResult>.Success(new EquipmentMachineOperationQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EquipmentMachineOperation for Tenant {TenantId}", request.TenantId);

            return Result<EquipmentMachineOperationQueryResult>.Failure(new ApplicationError(
                "SafetyPolicies.EquipmentMachineOperation.RetrievalFailed",
                "Unable to retrieve EquipmentMachineOperation data at this time.",
                Guid.NewGuid()));
        }
    }
}