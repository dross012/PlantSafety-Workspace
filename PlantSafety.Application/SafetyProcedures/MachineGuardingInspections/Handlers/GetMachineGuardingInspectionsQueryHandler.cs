// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetMachineGuardingInspectionsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyProcedures.MachineGuardingInspections.Queries;
using PlantSafety.Application.SafetyProcedures.MachineGuardingInspections.Repositories;

namespace PlantSafety.Application.SafetyProcedures.MachineGuardingInspections.Handlers;

/// <summary>Handles <see cref="GetMachineGuardingInspectionsQuery"/>.</summary>
public sealed class GetMachineGuardingInspectionsQueryHandler : IRequestHandler<GetMachineGuardingInspectionsQuery, Result<MachineGuardingInspectionsQueryResult>>
{
    private readonly IMachineGuardingInspectionsRepository _repository;
    private readonly ILogger<GetMachineGuardingInspectionsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetMachineGuardingInspectionsQueryHandler(IMachineGuardingInspectionsRepository repository, ILogger<GetMachineGuardingInspectionsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MachineGuardingInspectionsQueryResult>> Handle(GetMachineGuardingInspectionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetMachineGuardingInspectionsItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<MachineGuardingInspectionsQueryResult>.Success(new MachineGuardingInspectionsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve MachineGuardingInspections records for Tenant {TenantId}", request.TenantId);

            return Result<MachineGuardingInspectionsQueryResult>.Failure(new ApplicationError(
                "SafetyProcedures.MachineGuardingInspections.RetrievalFailed",
                "Unable to retrieve MachineGuardingInspections records at this time.",
                Guid.NewGuid()));
        }
    }
}