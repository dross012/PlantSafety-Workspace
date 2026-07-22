// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetMachineGuardsBarriersQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.MachineSafety.MachineGuardsBarriers.Queries;
using PlantSafety.Application.MachineSafety.MachineGuardsBarriers.Repositories;

namespace PlantSafety.Application.MachineSafety.MachineGuardsBarriers.Handlers;

/// <summary>Handles <see cref="GetMachineGuardsBarriersQuery"/>.</summary>
public sealed class GetMachineGuardsBarriersQueryHandler : IRequestHandler<GetMachineGuardsBarriersQuery, Result<MachineGuardsBarriersQueryResult>>
{
    private readonly IMachineGuardsBarriersRepository _repository;
    private readonly ILogger<GetMachineGuardsBarriersQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetMachineGuardsBarriersQueryHandler(IMachineGuardsBarriersRepository repository, ILogger<GetMachineGuardsBarriersQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MachineGuardsBarriersQueryResult>> Handle(GetMachineGuardsBarriersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetMachineGuardsBarriersItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<MachineGuardsBarriersQueryResult>.Success(new MachineGuardsBarriersQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve MachineGuardsBarriers records for Tenant {TenantId}", request.TenantId);

            return Result<MachineGuardsBarriersQueryResult>.Failure(new ApplicationError(
                "MachineSafety.MachineGuardsBarriers.RetrievalFailed",
                "Unable to retrieve MachineGuardsBarriers records at this time.",
                Guid.NewGuid()));
        }
    }
}