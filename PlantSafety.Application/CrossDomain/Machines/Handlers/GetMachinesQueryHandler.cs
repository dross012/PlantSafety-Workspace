// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetMachinesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.CrossDomain.Machines.Queries;
using PlantSafety.Application.CrossDomain.Machines.Repositories;

namespace PlantSafety.Application.CrossDomain.Machines.Handlers;

/// <summary>Handles <see cref="GetMachinesQuery"/>.</summary>
public sealed class GetMachinesQueryHandler : IRequestHandler<GetMachinesQuery, Result<MachinesQueryResult>>
{
    private readonly IMachinesRepository _repository;
    private readonly ILogger<GetMachinesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetMachinesQueryHandler(IMachinesRepository repository, ILogger<GetMachinesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MachinesQueryResult>> Handle(GetMachinesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetMachinesAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<MachinesQueryResult>.Success(new MachinesQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Machines for Tenant {TenantId}", request.TenantId);

            return Result<MachinesQueryResult>.Failure(new ApplicationError(
                "CrossDomain.Machines.RetrievalFailed",
                "Unable to retrieve machines data at this time.",
                Guid.NewGuid()));
        }
    }
}
