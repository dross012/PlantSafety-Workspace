// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetMachineGuardingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EmployeeSafety.MachineGuarding.Queries;
using PlantSafety.Application.EmployeeSafety.MachineGuarding.Repositories;

namespace PlantSafety.Application.EmployeeSafety.MachineGuarding.Handlers;

/// <summary>Handles <see cref="GetMachineGuardingQuery"/>.</summary>
public sealed class GetMachineGuardingQueryHandler : IRequestHandler<GetMachineGuardingQuery, Result<MachineGuardingQueryResult>>
{
    private readonly IMachineGuardingRepository _repository;
    private readonly ILogger<GetMachineGuardingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetMachineGuardingQueryHandler(IMachineGuardingRepository repository, ILogger<GetMachineGuardingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MachineGuardingQueryResult>> Handle(GetMachineGuardingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetMachineGuardingItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<MachineGuardingQueryResult>.Success(new MachineGuardingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve MachineGuarding records for Tenant {TenantId}", request.TenantId);

            return Result<MachineGuardingQueryResult>.Failure(new ApplicationError(
                "EmployeeSafety.MachineGuarding.RetrievalFailed",
                "Unable to retrieve MachineGuarding records at this time.",
                Guid.NewGuid()));
        }
    }
}