// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetMachineOperationalGuardingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.RiskManagement.MachineOperationalGuarding.Queries;
using PlantSafety.Application.RiskManagement.MachineOperationalGuarding.Repositories;

namespace PlantSafety.Application.RiskManagement.MachineOperationalGuarding.Handlers;

/// <summary>Handles <see cref="GetMachineOperationalGuardingQuery"/>.</summary>
public sealed class GetMachineOperationalGuardingQueryHandler : IRequestHandler<GetMachineOperationalGuardingQuery, Result<MachineOperationalGuardingQueryResult>>
{
    private readonly IMachineOperationalGuardingRepository _repository;
    private readonly ILogger<GetMachineOperationalGuardingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetMachineOperationalGuardingQueryHandler(IMachineOperationalGuardingRepository repository, ILogger<GetMachineOperationalGuardingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MachineOperationalGuardingQueryResult>> Handle(GetMachineOperationalGuardingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetMachineOperationalGuardingAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<MachineOperationalGuardingQueryResult>.Success(new MachineOperationalGuardingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve MachineOperationalGuarding for Tenant {TenantId}", request.TenantId);

            return Result<MachineOperationalGuardingQueryResult>.Failure(new ApplicationError(
                "RiskManagement.MachineOperationalGuarding.RetrievalFailed",
                "Unable to retrieve MachineOperationalGuarding data at this time.",
                Guid.NewGuid()));
        }
    }
}