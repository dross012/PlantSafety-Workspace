// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetNearMissesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.NearMisses.Queries;
using PlantSafety.Application.SafetyEvents.NearMisses.Repositories;

namespace PlantSafety.Application.SafetyEvents.NearMisses.Handlers;

/// <summary>Handles <see cref="GetNearMissesQuery"/>.</summary>
public sealed class GetNearMissesQueryHandler : IRequestHandler<GetNearMissesQuery, Result<NearMissesQueryResult>>
{
    private readonly INearMissesRepository _repository;
    private readonly ILogger<GetNearMissesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetNearMissesQueryHandler(INearMissesRepository repository, ILogger<GetNearMissesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<NearMissesQueryResult>> Handle(GetNearMissesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetNearMissesAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<NearMissesQueryResult>.Success(new NearMissesQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve NearMisses for Tenant {TenantId}", request.TenantId);

            return Result<NearMissesQueryResult>.Failure(new ApplicationError(
                "SafetyEvents.NearMisses.RetrievalFailed",
                "Unable to retrieve near misses data at this time.",
                Guid.NewGuid()));
        }
    }
}
