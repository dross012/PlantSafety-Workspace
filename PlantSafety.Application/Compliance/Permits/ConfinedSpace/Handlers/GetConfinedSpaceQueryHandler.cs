// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetConfinedSpaceQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Permits.ConfinedSpace.Queries;
using PlantSafety.Application.Compliance.Permits.ConfinedSpace.Repositories;

namespace PlantSafety.Application.Compliance.Permits.ConfinedSpace.Handlers;

/// <summary>Handles <see cref="GetConfinedSpaceQuery"/>.</summary>
public sealed class GetConfinedSpaceQueryHandler : IRequestHandler<GetConfinedSpaceQuery, Result<ConfinedSpaceQueryResult>>
{
    private readonly IConfinedSpaceRepository _repository;
    private readonly ILogger<GetConfinedSpaceQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetConfinedSpaceQueryHandler(IConfinedSpaceRepository repository, ILogger<GetConfinedSpaceQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ConfinedSpaceQueryResult>> Handle(GetConfinedSpaceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetConfinedSpaceAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ConfinedSpaceQueryResult>.Success(new ConfinedSpaceQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ConfinedSpace for Tenant {TenantId}", request.TenantId);

            return Result<ConfinedSpaceQueryResult>.Failure(new ApplicationError(
                "Compliance.Permits.ConfinedSpace.RetrievalFailed",
                "Unable to retrieve confined space permit data at this time.",
                Guid.NewGuid()));
        }
    }
}
