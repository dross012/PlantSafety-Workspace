// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetHazardIsolationSequencingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scheduling.HazardIsolationSequencing.Queries;
using PlantSafety.Application.Scheduling.HazardIsolationSequencing.Repositories;

namespace PlantSafety.Application.Scheduling.HazardIsolationSequencing.Handlers;

/// <summary>Handles <see cref="GetHazardIsolationSequencingQuery"/>.</summary>
public sealed class GetHazardIsolationSequencingQueryHandler : IRequestHandler<GetHazardIsolationSequencingQuery, Result<HazardIsolationSequencingQueryResult>>
{
    private readonly IHazardIsolationSequencingRepository _repository;
    private readonly ILogger<GetHazardIsolationSequencingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetHazardIsolationSequencingQueryHandler(IHazardIsolationSequencingRepository repository, ILogger<GetHazardIsolationSequencingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazardIsolationSequencingQueryResult>> Handle(GetHazardIsolationSequencingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetHazardIsolationSequencingAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<HazardIsolationSequencingQueryResult>.Success(new HazardIsolationSequencingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve HazardIsolationSequencing for Tenant {TenantId}", request.TenantId);

            return Result<HazardIsolationSequencingQueryResult>.Failure(new ApplicationError(
                "Scheduling.HazardIsolationSequencing.RetrievalFailed",
                "Unable to retrieve HazardIsolationSequencing data at this time.",
                Guid.NewGuid()));
        }
    }
}