// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetLotoSystemsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.MachineSafety.LotoSystems.Queries;
using PlantSafety.Application.MachineSafety.LotoSystems.Repositories;

namespace PlantSafety.Application.MachineSafety.LotoSystems.Handlers;

/// <summary>Handles <see cref="GetLotoSystemsQuery"/>.</summary>
public sealed class GetLotoSystemsQueryHandler : IRequestHandler<GetLotoSystemsQuery, Result<LotoSystemsQueryResult>>
{
    private readonly ILotoSystemsRepository _repository;
    private readonly ILogger<GetLotoSystemsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetLotoSystemsQueryHandler(ILotoSystemsRepository repository, ILogger<GetLotoSystemsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LotoSystemsQueryResult>> Handle(GetLotoSystemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetLotoSystemsItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<LotoSystemsQueryResult>.Success(new LotoSystemsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve LotoSystems records for Tenant {TenantId}", request.TenantId);

            return Result<LotoSystemsQueryResult>.Failure(new ApplicationError(
                "MachineSafety.LotoSystems.RetrievalFailed",
                "Unable to retrieve LotoSystems records at this time.",
                Guid.NewGuid()));
        }
    }
}