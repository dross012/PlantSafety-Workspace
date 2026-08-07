// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for SearchNioshHazardsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NIOSHStandards.Models;
using PlantSafety.Application.Compliance.NIOSHStandards.Repositories;

namespace PlantSafety.Application.Compliance.NIOSHStandards.Queries;

/// <summary>Handles <see cref="SearchNioshHazardsQuery"/>.</summary>
public sealed class SearchNioshHazardsQueryHandler(
    INioshReferenceRepository repository,
    ILogger<SearchNioshHazardsQueryHandler> logger)
    : IRequestHandler<SearchNioshHazardsQuery, Result<IReadOnlyList<NioshHazardProfileDto>>>
{
    private const int MaxTake = 500;

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<NioshHazardProfileDto>>> Handle(SearchNioshHazardsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var take = request.Take is <= 0 or > MaxTake ? MaxTake : request.Take;
            var skip = request.Skip < 0 ? 0 : request.Skip;

            var results = await repository.SearchHazardsAsync(request.Keyword, skip, take, cancellationToken);
            return Result<IReadOnlyList<NioshHazardProfileDto>>.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to search NIOSH hazard profiles (Keyword={Keyword})", request.Keyword);

            return Result<IReadOnlyList<NioshHazardProfileDto>>.Failure(new ApplicationError(
                "Compliance.NIOSHStandards.SearchFailed",
                "Unable to search NIOSH hazard profiles at this time.",
                Guid.NewGuid()));
        }
    }
}
