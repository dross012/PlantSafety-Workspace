// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetTwoHandControlsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.MachineSafety.TwoHandControls.Queries;
using PlantSafety.Application.MachineSafety.TwoHandControls.Repositories;

namespace PlantSafety.Application.MachineSafety.TwoHandControls.Handlers;

/// <summary>Handles <see cref="GetTwoHandControlsQuery"/>.</summary>
public sealed class GetTwoHandControlsQueryHandler : IRequestHandler<GetTwoHandControlsQuery, Result<TwoHandControlsQueryResult>>
{
    private readonly ITwoHandControlsRepository _repository;
    private readonly ILogger<GetTwoHandControlsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetTwoHandControlsQueryHandler(ITwoHandControlsRepository repository, ILogger<GetTwoHandControlsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TwoHandControlsQueryResult>> Handle(GetTwoHandControlsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetTwoHandControlsItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<TwoHandControlsQueryResult>.Success(new TwoHandControlsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve TwoHandControls records for Tenant {TenantId}", request.TenantId);

            return Result<TwoHandControlsQueryResult>.Failure(new ApplicationError(
                "MachineSafety.TwoHandControls.RetrievalFailed",
                "Unable to retrieve TwoHandControls records at this time.",
                Guid.NewGuid()));
        }
    }
}