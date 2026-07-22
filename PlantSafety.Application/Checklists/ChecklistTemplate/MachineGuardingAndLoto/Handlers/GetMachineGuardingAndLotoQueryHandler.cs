// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetMachineGuardingAndLotoQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Checklists.ChecklistTemplate.MachineGuardingAndLoto.Queries;
using PlantSafety.Application.Checklists.ChecklistTemplate.MachineGuardingAndLoto.Repositories;

namespace PlantSafety.Application.Checklists.ChecklistTemplate.MachineGuardingAndLoto.Handlers;

/// <summary>Handles <see cref="GetMachineGuardingAndLotoQuery"/>.</summary>
public sealed class GetMachineGuardingAndLotoQueryHandler : IRequestHandler<GetMachineGuardingAndLotoQuery, Result<MachineGuardingAndLotoQueryResult>>
{
    private readonly IMachineGuardingAndLotoRepository _repository;
    private readonly ILogger<GetMachineGuardingAndLotoQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetMachineGuardingAndLotoQueryHandler(IMachineGuardingAndLotoRepository repository, ILogger<GetMachineGuardingAndLotoQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MachineGuardingAndLotoQueryResult>> Handle(GetMachineGuardingAndLotoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<MachineGuardingAndLotoQueryResult>.Success(new MachineGuardingAndLotoQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve MachineGuardingAndLoto for Tenant {TenantId}", request.TenantId);

            return Result<MachineGuardingAndLotoQueryResult>.Failure(new ApplicationError(
                "Checklists.ChecklistTemplate.MachineGuardingAndLoto.RetrievalFailed",
                "Unable to retrieve Machine Guarding & LOTO Checklists data at this time.",
                Guid.NewGuid()));
        }
    }
}