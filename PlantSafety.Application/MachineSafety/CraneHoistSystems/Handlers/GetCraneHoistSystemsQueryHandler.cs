// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetCraneHoistSystemsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Repositories;

namespace PlantSafety.Application.MachineSafety.CraneHoistSystems.Handlers;

/// <summary>Handles <see cref="GetCraneHoistSystemsQuery"/>.</summary>
public sealed class GetCraneHoistSystemsQueryHandler : IRequestHandler<GetCraneHoistSystemsQuery, Result<CraneHoistSystemsQueryResult>>
{
    private readonly ICraneHoistSystemsRepository _repository;
    private readonly ILogger<GetCraneHoistSystemsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCraneHoistSystemsQueryHandler(ICraneHoistSystemsRepository repository, ILogger<GetCraneHoistSystemsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CraneHoistSystemsQueryResult>> Handle(GetCraneHoistSystemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetCraneHoistSystemsItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<CraneHoistSystemsQueryResult>.Success(new CraneHoistSystemsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve CraneHoistSystems records for Tenant {TenantId}", request.TenantId);

            return Result<CraneHoistSystemsQueryResult>.Failure(new ApplicationError(
                "MachineSafety.CraneHoistSystems.RetrievalFailed",
                "Unable to retrieve CraneHoistSystems records at this time.",
                Guid.NewGuid()));
        }
    }
}
